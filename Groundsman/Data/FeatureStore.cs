using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Plugin.FilePicker;
using Plugin.FilePicker.Abstractions;
using Xamarin.Essentials;

namespace Groundsman.Data
{
    public class FeatureStore
    {
        /// <summary>
        /// GeoJSONObject that stores the current feature list during runtime
        /// </summary>
        public GeoJSONObject GeoJSONStore = new GeoJSONObject()
        {
            type = "FeatureCollection",
            features = new ObservableCollection<Feature>()
        };

        /// <summary>
        /// Get features list from file
        /// </summary>
        /// <returns></returns>
        public async Task FetchFeaturesFromFile()
        {
            string featuresFile = GetFeaturesFile();
            bool success = await ImportFeaturesAsync(featuresFile, false);
            if (!success)
            {
                var confirmation = await HomePage.Instance.DisplayAlert("Feature List Error", $"Groundsman has detected that your stored feature list is corrupt and cannot be opened. You may copy your data and modify it with an external editor to be GeoJSON compliant then import it again.", "Copy to Clipboard and Erase", "Erase");
                if (confirmation)
                {
                    await Clipboard.SetTextAsync(featuresFile);
                    
                }
                DeleteAllFeatures();
            }
        }

        /// <summary>
        /// Delete selected feature
        /// </summary>
        /// <param name="feature"></param>
        public void DeleteFeatureAsync(Feature feature)
        {
            bool deleteSuccessful = GeoJSONStore.features.Remove(feature);
            if (deleteSuccessful)
            {
                SaveCurrentFeaturesToEmbeddedFile();
            }
        }

        /// <summary>
        /// Add a feature to the feature list and call save features to file
        /// </summary>
        /// <param name="feature"></param>
        public void SaveFeatureAsync(Feature feature)
        {
            // If this is a newly added feature, generate an ID and add it immediately.
            if (feature.properties.id == AppConstants.NEW_ENTRY_ID)
            {
                EnsureUniqueID(feature);
                GeoJSONStore.features.Add(feature);
            }
            else
            {
                // Otherwise we are saving over an existing feature, so override its contents without changing ID.
                int indexToEdit = -1;
                for (int i = 0; i < GeoJSONStore.features.Count; i++)
                {
                    if (GeoJSONStore.features[i].properties.id == feature.properties.id)
                    {
                        indexToEdit = i;
                        break;
                    }
                }

                if (indexToEdit != -1)
                {
                    GeoJSONStore.features[indexToEdit] = feature;
                }
            }
            SaveCurrentFeaturesToEmbeddedFile();
        }

        /// <summary>
        /// clears all features and reloads the example featureset
        /// </summary>
        public void DeleteAllFeatures()
        {
            GeoJSONStore.features.Clear();
            SaveCurrentFeaturesToEmbeddedFile();

            _ = ImportFeaturesAsync(GetDefaultFile(), false);
        }

        /// <summary>
        /// Imports features from a specified filepath.
        /// </summary>
        /// <param name="path">path to file.</param>
        /// <returns></returns>
        public async Task<bool> ImportFeaturesFromFileURL(string path, string fileName)
        {
            var confirmation = await HomePage.Instance.DisplayAlert("Import File", $"Do you want to add the features in '{fileName}' to your features list?", "Yes", "No");
            if (confirmation)
            {
                try
                {
                    string text = File.ReadAllText(path);
                    bool resultStatus = await ImportFeaturesAsync(text, true);
                    return resultStatus;
                }
                catch (Exception)
                {
                    await HomePage.Instance.DisplayAlert("Import Error", "An unknown error occured when trying to process this file.", "OK");
                }
            }
            return false;
        }

        public async Task ImportFeaturesFromFile()
        {
            //TODO: exception handling - 
            try
            {
                var status = await Services.CheckAndRequestPermissionAsync(new Permissions.StorageRead());

                // If permissions allowed, prompt the user to pick a file.
                if (status == PermissionStatus.Granted)
                {
                    FileData fileData = await CrossFilePicker.Current.PickFile();

                    // If the user didn't cancel, import the contents of the file they selected.
                    if (fileData != null)
                    {
                        string contents = System.Text.Encoding.UTF8.GetString(fileData.DataArray);
                        await App.FeatureStore.ImportFeaturesAsync(contents, true);
                    }
                }
                else
                {
                    await HomePage.Instance.DisplayAlert("Permissions Error", "Storage permissions for Groundsman must be enabled to utilise this feature.", "Ok", "OK");
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task ImportFeaturesFromClipboard()
        {
            try
            {
                string contents = await Clipboard.GetTextAsync();
                await ImportFeaturesAsync(contents, true);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task ExportFeatures()
        {
            await Share.RequestAsync(new ShareFileRequest
            {
                Title = "Features Export",
                File = new ShareFile(AppConstants.FEATURES_FILE, "text/plain")
            });
        }

        public async Task ExportFeature(Feature feature)
        {
            ObservableCollection<Feature> featureList = new ObservableCollection<Feature>
            {
                feature
            };
            var rootobject = new GeoJSONObject
            {
                type = "FeatureCollection",
                features = featureList
            };

            await Share.RequestAsync(new ShareTextRequest
            {
                Title = featureList[0].properties.name,
                Text = JsonConvert.SerializeObject(rootobject, Formatting.Indented)
            });
        }

        public async Task CopyFeaturesToClipboard()
        {
            string textFile = JsonConvert.SerializeObject(GeoJSONStore, Formatting.Indented);
            await Clipboard.SetTextAsync(textFile);
            await HomePage.Instance.DisplayAlert("Copy Features", "Features successfully copied to clipboard.", "OK");
        }

        /// <summary>
        /// Imports features from the contents of a file.
        /// </summary>
        /// <param name="fileContents">The string of geojson to import from.</param>
        /// <returns></returns>
        private async Task<bool> ImportFeaturesAsync(string importContents, bool notify)
        {
            // Ensure file contents are structured in a valid GeoJSON format.
            GeoJSONObject importedFeaturesData = null;
            int successfulImport = 0;
            int failedImport = 0;

            try
            {
                importedFeaturesData = JsonConvert.DeserializeObject<GeoJSONObject>(importContents);
            }
            catch (Exception ex)
            {
                await HomePage.Instance.DisplayAlert("Import Failed", string.Format("Groundsman can only import valid GeoJSON and encountered an error:\n{0}", ex.Message), "OK");
            }

            if (importedFeaturesData != null)
            {
                // Loop through all imported features and make sure they are valid and ensuring there are no ID clashes.
                foreach (var importedFeature in importedFeaturesData.features)
                {
                    bool parseResult = await TryParseFeature(importedFeature);
                    if (parseResult)
                    {
                        EnsureUniqueID(importedFeature);
                        GeoJSONStore.features.Add(importedFeature);
                        successfulImport++;
                    }
                    else
                    {
                        failedImport++;
                    }
                }

                SaveCurrentFeaturesToEmbeddedFile();
                if (notify)
                {
                    await HomePage.Instance.DisplayAlert("Import Success", string.Format("{0} new features have been added to your features list. {1} features failed to import.", successfulImport, failedImport), "OK");
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// Formats the list of current features into valid geojson, then writes it to the embedded file.
        /// </summary>
        /// <returns></returns>
        private void SaveCurrentFeaturesToEmbeddedFile()
        {
            // Save the rootobject to file.
            var json = JsonConvert.SerializeObject(GeoJSONStore);
            File.WriteAllText(AppConstants.FEATURES_FILE, json);
        }

        /// <summary>
        /// Ensure given feature meets formatting requreiments
        /// </summary>
        /// <param name="feature"></param>
        /// <returns></returns>
        private async Task<bool> TryParseFeature(Feature feature)
        {
            // Ensure the feature has valid GeoJSON fields supplied.
            if (feature != null && feature.type != null && feature.geometry != null && feature.geometry.type != null && feature.geometry.coordinates != null)
            {
                feature.properties.xamarincoordinates = new ObservableCollection<Point>();
                object[] trueCoords;

                // Determine if feature is supported and if so convert its points and add appropriate icon
                switch (feature.geometry.type)
                {
                    case "Point":
                        feature.properties.typeIconPath = "point_icon.png";
                        trueCoords = feature.geometry.coordinates.ToArray();
                        feature.properties.xamarincoordinates.Add(JsonCoordToXamarinPoint(trueCoords));
                        break;
                    case "LineString":
                        feature.properties.typeIconPath = "line_icon.png";
                        // Iterates the root coordinates (List<object>),
                        // then casts each element in the list to a Jarray which contain the actual coordinates.
                        for (int i = 0; i < feature.geometry.coordinates.Count; i++)
                        {
                            trueCoords = ((JArray)feature.geometry.coordinates[i]).ToObject<object[]>();
                            feature.properties.xamarincoordinates.Add(JsonCoordToXamarinPoint(trueCoords));
                        }
                        break;
                    case "Polygon":
                        feature.properties.typeIconPath = "area_icon.png";
                        // Iterates the root coordinates (List<object>), and casts each element in the list to a Jarray, 
                        // then casts each Jarray's element to another Jarray which contain the actual coordinates.
                        for (int i = 0; i < feature.geometry.coordinates.Count; i++)
                        {
                            for (int j = 0; j < ((JArray)feature.geometry.coordinates[i]).Count; j++)
                            {
                                trueCoords = ((JArray)(((JArray)feature.geometry.coordinates[i])[j])).ToObject<object[]>();
                                feature.properties.xamarincoordinates.Add(JsonCoordToXamarinPoint(trueCoords));
                            }
                        }
                        break;
                    default:
                        await HomePage.Instance.DisplayAlert("Unsupported Feature", "Groundsman currently only supports feature types of Point, Line, and Polygon.", "OK");
                        return false;
                }

                // If author ID hasn't been set on the feature, default it to the user's ID.
                if (string.IsNullOrWhiteSpace(feature.properties.author))
                {
                    feature.properties.author = Preferences.Get("UserID", "Groundsman");
                }

                // Add default name if empty
                if (string.IsNullOrWhiteSpace(feature.properties.name))
                {
                    feature.properties.name = "Unnamed " + feature.geometry.type;
                }

                // If the date field is missing or invalid, convert it into DateTime.Now.
                if (feature.properties.date == null || DateTime.TryParse(feature.properties.date, out _) == false)
                {
                    feature.properties.date = DateTime.Now.ToShortDateString();
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="coords"></param>
        /// <returns></returns>
        private Point JsonCoordToXamarinPoint(object[] coords)
        {
            double longitude = (double)coords[0];
            double latitude = (double)coords[1];
            double altitude = (coords.Length == 3) ? (double)coords[2] : 0.0;

            Point point = new Point(latitude, longitude, altitude);
            return point;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="feature"></param>
        private static void EnsureUniqueID(Feature feature)
        {
            // Generate feature ID
            //TODO: avoid ID collisions
            feature.properties.id = Guid.NewGuid().ToString();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private string GetFeaturesFile()
        {
            if (File.Exists(AppConstants.FEATURES_FILE))
            {
                return File.ReadAllText(AppConstants.FEATURES_FILE);
            }
            else
            {
                return GetDefaultFile();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private string GetDefaultFile()
        {
            var assembly = IntrospectionExtensions.GetTypeInfo(GetType()).Assembly;
            Stream stream = assembly.GetManifestResourceStream("Groundsman.locationsAutoGenerated.json");
            string text = "";
            using (var reader = new StreamReader(stream))
            {
                text = reader.ReadToEnd();
            }
            return text;
        }
    }
}