using Groundsman.Interfaces;
using Groundsman.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace Groundsman.Services
{
    public class FeatureService : IDataService<Feature>
    {
        public bool AddItem(Feature item)
        {
            if (ParseProperties(item))
            {
                App.featureList.Add(item);
                _ = SaveFeaturesToFile(App.featureList, Constants.FEATURES_FILE);
                return true;
            }
            return false;
        }

        public int AddItems(IEnumerable<Feature> item)
        {
            int successfulImport = 0;
            foreach (Feature feature in item)
            {
                if (ParseProperties(feature))
                {
                    App.featureList.Add(feature);
                    successfulImport++;
                }
            }
            _ = SaveFeaturesToFile(App.featureList, Constants.FEATURES_FILE);
            return successfulImport;
        }


        public bool DeleteItem(Feature item)
        {
            SaveFeatureToFile(item, Constants.DELETED_FEATURE_FILE);
            bool deleteSuccessful = App.featureList.Remove(item);
            _ = SaveFeaturesToFile(App.featureList, Constants.FEATURES_FILE);
            return deleteSuccessful;
        }

        /// <summary>
        /// Clears and resets feature list to default template items
        /// </summary>
        public void ResetItems()
        {
            App.featureList.Clear();
            ImportItems(Constants.GetTemplateFile());
            _ = SaveFeaturesToFile(App.featureList, Constants.FEATURES_FILE);
        }

        public bool UpdateItem(Feature item)
        {
            for (int i = 0; i < App.featureList.Count; i++)
            {
                if (App.featureList[i].Properties[Constants.IdentifierProperty] == item.Properties[Constants.IdentifierProperty])
                {
                    App.featureList[i] = item;
                    _ = SaveFeaturesToFile(App.featureList, Constants.FEATURES_FILE);
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Imports GeoJSON and adds the new features to the feature list
        /// </summary>
        /// <param name="contents">A serialised GeoJSON object</param>
        /// <returns>Number of features successfully imported</returns>
        public int ImportItems(string contents)
        {
            GeoJSONObject importedGeoJSON = GeoJSONObject.ImportGeoJSON(contents);
            if (importedGeoJSON == null)
            {
                throw new ArgumentException("Import data does not contain valid GeoJSON");
            }
            return importedGeoJSON.Type switch
            {
                GeoJSONType.Point => AddItem(new Feature((Point)importedGeoJSON)) ? 1 : 0,
                GeoJSONType.LineString => AddItem(new Feature((LineString)importedGeoJSON)) ? 1 : 0,
                GeoJSONType.Polygon => AddItem(new Feature((Polygon)importedGeoJSON)) ? 1 : 0,
                GeoJSONType.Feature => AddItem((Feature)importedGeoJSON) ? 1 : 0,
                GeoJSONType.FeatureCollection => AddItems(((FeatureCollection)importedGeoJSON).Features),
                _ => throw new ArgumentException("Import data does not contain a supported GeoJSON type."),
            };
        }

        private Task SaveFeatureToFile(Feature item, string FileName)
        {
            var json = JsonConvert.SerializeObject(item);
            return File.WriteAllTextAsync(FileName, json);
        }

        public Task SaveFeaturesToFile(IList<Feature> items, string FileName)
        {
            FeatureCollection geoJSONObject = new FeatureCollection(items);
            return File.WriteAllTextAsync(FileName, JsonConvert.SerializeObject(geoJSONObject));
        }

        public ShareFileRequest ExportFeatures(IList<Feature> items)
        {
            string fileName;
            switch (items.Count)
            {
                case 0:
                    fileName = "Groundsman Feature Collection";
                    SaveFeaturesToFile(items, Constants.GetExportFile(fileName, ExportType.GeoJSON));
                    break;
                case 1:
                    fileName = (string)items[0].Properties[Constants.NameProperty];
                    SaveFeatureToFile(items[0], Constants.GetExportFile(fileName, ExportType.GeoJSON));
                    break;
                default:
                    fileName = "Groundsman Feature Collection";
                    SaveFeaturesToFile(items, Constants.GetExportFile(fileName, ExportType.GeoJSON));
                    break;
            }

            return new ShareFileRequest
            {
                Title = "Features Export",
                File = new ShareFile(Constants.GetExportFile(fileName, ExportType.GeoJSON), "application/json"),
            };
        }

        private static bool ParseProperties(Feature feature)
        {
            if (feature.Properties == null)
            {
                feature.Properties = new Dictionary<string, object>();
            }

            if (!feature.Properties.ContainsKey(Constants.IdentifierProperty))
            {
                feature.Properties.Add(Constants.IdentifierProperty, Guid.NewGuid().ToString());
            }
            else if ((string)feature.Properties[Constants.IdentifierProperty] == Constants.NewFeatureID) //Until parsing is properly modular - brand new features are not checked again below
            {
                feature.Properties[Constants.IdentifierProperty] = Guid.NewGuid().ToString();
                return true;
            }


            // If author ID hasn't been set on the feature, default it to the user's ID.
            string author = Preferences.Get(Constants.UserIDKey, Constants.DefaultUserValue);
            if (feature.Properties.ContainsKey(Constants.AuthorProperty))
            {
                author = (string)feature.Properties[Constants.AuthorProperty];
                if (author.Length > 30)
                {
                    author = author.Substring(0, 30);
                    feature.Properties[Constants.AuthorProperty] = author;
                }
            }
            else
            {
                feature.Properties.Add(Constants.AuthorProperty, author);
            }

            // Add default name if empty
            string name = feature.Geometry.Type.ToString();
            if (feature.Properties.ContainsKey(Constants.NameProperty))
            {
                name = (string)feature.Properties[Constants.NameProperty];
                if (name.Length > 30)
                {
                    name = name.Substring(0, 30);
                    foreach (char c in Path.GetInvalidFileNameChars())
                    {
                        name = name.Replace(c, '-');
                    }
                    feature.Properties[Constants.NameProperty] = name;
                }
            }
            else
            {
                feature.Properties.Add(Constants.NameProperty, name);
            }

            DateTime date = DateTime.Now;
            if (feature.Properties.ContainsKey(Constants.DateProperty))
            {
                // If the date field is missing or invalid, convert it into DateTime.Now.
                if (DateTime.TryParse((string)feature.Properties[Constants.DateProperty], out date) == false)
                {
                    //warning date couldnt be parsed
                }

                feature.Properties[Constants.DateProperty] = date.ToShortDateString();
            }
            else
            {
                feature.Properties.Add(Constants.DateProperty, date.ToShortDateString());
            }
            return true;
        }
    }
}
