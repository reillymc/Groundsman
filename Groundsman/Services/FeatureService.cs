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
                SaveFeaturesToFile(App.featureList, AppConstants.FEATURES_FILE);
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
            SaveFeaturesToFile(App.featureList, AppConstants.FEATURES_FILE);
            return successfulImport;
        }


        public async Task<bool> DeleteItemAsync(Feature item)
        {
            SaveFeatureToFile(item, AppConstants.DELETED_FEATURE_FILE);
            bool deleteSuccessful = App.featureList.Remove(item);
            var save = SaveFeaturesToFile(App.featureList, AppConstants.FEATURES_FILE);
            return save;
        }

        /// <summary>
        /// Clears and resets feature list to default template items
        /// </summary>
        /// <returns>True if saving of template files to feature list was successful</returns>
        public async Task<bool> DeleteItemsAsync()
        {
            App.featureList.Clear();
            await ImportFeaturesAsync(AppConstants.GetTemplateFile());
            return SaveFeaturesToFile(App.featureList, AppConstants.FEATURES_FILE);
        }

        public async Task<bool> UpdateItemAsync(Feature item)
        {
            //bool parseResult = TryParseFeature(item); SHOULD be implemented infuture for final verification once JSON importing is split from general verification
            for (int i = 0; i < App.featureList.Count; i++)
            {
                if (App.featureList[i].Properties["id"] == item.Properties["id"])
                {
                    App.featureList[i] = item;
                }

            }
            return SaveFeaturesToFile(App.featureList, AppConstants.FEATURES_FILE);

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
                SaveFeaturesToFile(items, AppConstants.GetExportFile(fileName));
            }
            else if (items.Count == 1)
            {
                fileName = (string)items[0].Properties["name"];
                SaveFeatureToFile(items[0], AppConstants.GetExportFile(fileName));
            }

            return new ShareFileRequest
            {
                Title = "Features Export",
                File = new ShareFile(AppConstants.GetExportFile(fileName), "application/json"),
            };
        }

        private static bool ParseProperties(Feature feature)
        {
            if (feature.Properties == null)
            {
                feature.Properties = new Dictionary<string, object>();
            }

            if (!feature.Properties.ContainsKey("id"))
            {
                feature.Properties.Add("id", Guid.NewGuid().ToString());
            }
            else if ((string)feature.Properties["id"] == AppConstants.NEW_ENTRY_ID) //Until parsing is properly modular - brand new features are not checked again below
            {
                feature.Properties["id"] = Guid.NewGuid().ToString();
                return true;
            }


            // If author ID hasn't been set on the feature, default it to the user's ID.
            string author = Preferences.Get("UserID", "Groundsman");
            if (feature.Properties.ContainsKey("author"))
            {
                author = (string)feature.Properties["author"];
                if (author.Length > 30)
                {
                    author = author.Substring(0, 30);
                    feature.Properties["author"] = author;
                }
            }
            else
            {
                feature.Properties.Add("author", author);
            }

            // Add default name if empty
            string name = feature.Geometry.Type.ToString();
            if (feature.Properties.ContainsKey("name"))
            {
                name = (string)feature.Properties["name"];
                if (name.Length > 30)
                {
                    name = name.Substring(0, 30);
                    foreach (char c in Path.GetInvalidFileNameChars())
                    {
                        name = name.Replace(c, '-');
                    }
                    feature.Properties["name"] = name;
                }
            }
            else
            {
                feature.Properties.Add("name", name);
            }

            if (feature.Properties.ContainsKey("metadataStringValue"))
            {
                string metadataStringValue = (string)feature.Properties["metadataStringValue"];
                if (!string.IsNullOrEmpty(metadataStringValue) && metadataStringValue.Length > 30)
                {
                    metadataStringValue = name.Substring(0, 30);
                    foreach (char c in Path.GetInvalidFileNameChars())
                    {
                        metadataStringValue = metadataStringValue.Replace(c, '-');
                    }
                    feature.Properties["metadataStringValue"] = metadataStringValue;
                }
            }

            if (feature.Properties.ContainsKey("metadataIntegerValue"))
            {
                try
                {
                    int metadataIntegerValue = Convert.ToInt32(feature.Properties["metadataIntegerValue"]);
                    feature.Properties["metadataIntegerValue"] = metadataIntegerValue;
                }
                catch
                {
                    //Could not parse int value warning
                }
            }

            if (feature.Properties.ContainsKey("metadataFloatValue"))
            {
                try
                {
                    float metadataFloatValue = Convert.ToSingle(feature.Properties["metadataFloatValue"]);
                    feature.Properties["metadataFloatValue"] = metadataFloatValue;
                }
                catch
                {
                    //Could not parse float value warning
                }
            }

            DateTime date = DateTime.Now;
            if (feature.Properties.ContainsKey("date"))
            {
                // If the date field is missing or invalid, convert it into DateTime.Now.
                if (DateTime.TryParse((string)feature.Properties["date"], out date) == false)
                {
                    //warning date couldnt be parsed
                }
                feature.Properties["date"] = date.ToShortDateString();
            }
            else
            {
                feature.Properties.Add("date", date.ToShortDateString());
            }
            return true;
        }
    }
}
