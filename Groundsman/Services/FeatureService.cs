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
        public async Task<bool> AddItemAsync(Feature item)
        {
            bool parseResult = ParseProperties(item);
            if (parseResult)
            {
                //EnsureUniqueID(importedFeature);
                App.featureList.Add(item);
                SaveFeaturesToFile(App.featureList, Constants.FEATURES_FILE);
            }
            return parseResult;
        }

        public async Task<int> AddItemsAsync(IEnumerable<Feature> item)
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
            SaveFeaturesToFile(App.featureList, Constants.FEATURES_FILE);
            return successfulImport;
        }


        public async Task<bool> DeleteItemAsync(Feature item)
        {
            SaveFeatureToFile(item, Constants.DELETED_FEATURE_FILE);
            bool deleteSuccessful = App.featureList.Remove(item);
            var save = SaveFeaturesToFile(App.featureList, Constants.FEATURES_FILE);
            return save;
        }

        /// <summary>
        /// Clears and resets feature list to default template items
        /// </summary>
        /// <returns>True if saving of template files to feature list was successful</returns>
        public async Task<bool> DeleteItemsAsync()
        {
            App.featureList.Clear();
            await ImportFeaturesAsync(Constants.GetTemplateFile());
            return SaveFeaturesToFile(App.featureList, Constants.FEATURES_FILE);
        }

        public async Task<bool> UpdateItemAsync(Feature item)
        {
            //bool parseResult = TryParseFeature(item); SHOULD be implemented infuture for final verification once JSON importing is split from general verification
            for (int i = 0; i < App.featureList.Count; i++)
            {
                if (App.featureList[i].Properties[Constants.IdentifierProperty] == item.Properties[Constants.IdentifierProperty])
                {
                    App.featureList[i] = item;
                }

            }
            return SaveFeaturesToFile(App.featureList, Constants.FEATURES_FILE);

        }



        /// <summary>
        /// Imports GeoJSON and adds the new features to the feature list
        /// </summary>
        /// <param name="contents">A serialised GeoJSON object</param>
        /// <returns>Number of features successfully imported</returns>
        public async Task<int> ImportFeaturesAsync(string contents)
        {
            GeoJSONObject importedGeoJSON = GeoJSONObject.ImportGeoJSON(contents);
            if (importedGeoJSON == null)
            {
                throw new ArgumentException("Import data does not contain valid GeoJSON");
            }
            return importedGeoJSON.Type switch
            {
                GeoJSONType.Point => await AddItemAsync(new Feature((Point)importedGeoJSON)) ? 1 : 0,
                GeoJSONType.LineString => await AddItemAsync(new Feature((LineString)importedGeoJSON)) ? 1 : 0,
                GeoJSONType.Polygon => await AddItemAsync(new Feature((Polygon)importedGeoJSON)) ? 1 : 0,
                GeoJSONType.Feature => await AddItemAsync((Feature)importedGeoJSON) ? 1 : 0,
                GeoJSONType.FeatureCollection => await AddItemsAsync(((FeatureCollection)importedGeoJSON).Features),
                _ => throw new ArgumentException("Import data does not contain a supported GeoJSON type."),
            };
        }

        private bool SaveFeatureToFile(Feature item, string FileName)
        {
            var json = JsonConvert.SerializeObject(item);
            File.WriteAllText(FileName, json);
            return true;
        }


        public bool SaveFeaturesToFile(IList<Feature> items, string FileName)
        {
            FeatureCollection geoJSONObject = new FeatureCollection(items);

            var json = JsonConvert.SerializeObject(geoJSONObject);
            File.WriteAllText(FileName, json);
            return true;
        }

        public ShareFileRequest ExportFeatures(IList<Feature> items)
        {
            string fileName = "";
            if (items.Count > 1)
            {
                fileName = "Groundsman Feature Collection";
                SaveFeaturesToFile(items, Constants.GetExportFile(fileName, ExportType.GeoJSON));
            }
            else if (items.Count == 1)
            {
                fileName = (string)items[0].Properties[Constants.NameProperty];
                SaveFeatureToFile(items[0], Constants.GetExportFile(fileName, ExportType.GeoJSON));
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
            string author = Preferences.Get(Constants.UserIDKey, "Groundsman");
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
