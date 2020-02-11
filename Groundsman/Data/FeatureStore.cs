using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Plugin.FilePicker;
using Plugin.FilePicker.Abstractions;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;
using Xamarin.Essentials;

namespace Groundsman.Data
{
    public class FeatureStore
    {
        public List<Feature> CurrentFeatures { get; set; } = new List<Feature>();

        public Task<List<Feature>> GetFeaturesAsync()
        {
            return Task.Run(async () =>
            {
                string featuresFile = GetFeaturesFile();
                var rootobject = JsonConvert.DeserializeObject<RootObject>(featuresFile);
                if (rootobject == null)
                {
                    //TODO: handle exception allowing to export corrupted file/erase and start over
                    throw new Exception();
                }

                rootobject.Type = "FeatureCollection";

                foreach (var feature in rootobject.Features)
                {
                    await TryParseFeature(feature);
                }

                return rootobject.Features;
            });
        }

        private async Task<bool> TryParseFeature(Feature feature)
        {
            // Ensure the feature has valid GeoJSON fields supplied.
            if (feature != null && feature.Type != null && feature.Geometry != null && feature.Geometry.Type != null && feature.Geometry.Coordinates != null)
            {
                // Immediately convert LineStrings to Line for use in the rest of the codebase. 
                // This will be converted back to LineString before serialization back to json.
                if (feature.Geometry.Type == "LineString")
                {
                    feature.Geometry.Type = "Line";
                }

                feature.Properties.Xamarincoordinates = new List<Point>();
                object[] trueCoords;

                // Determine if feature is supported and if so convert its points and add appropriate icon
                switch (feature.Geometry.Type)
                {
                    case "Point":
                        feature.Properties.TypeIconPath = "point_icon.png";
                        trueCoords = feature.Geometry.Coordinates.ToArray();
                        feature.Properties.Xamarincoordinates.Add(JsonCoordToXamarinPoint(trueCoords));
                        break;
                    case "Line":
                        feature.Properties.TypeIconPath = "line_icon.png";
                        // Iterates the root coordinates (List<object>),
                        // then casts each element in the list to a Jarray which contain the actual coordinates.
                        for (int i = 0; i < feature.Geometry.Coordinates.Count; i++)
                        {
                            trueCoords = ((JArray)feature.Geometry.Coordinates[i]).ToObject<object[]>();
                            feature.Properties.Xamarincoordinates.Add(JsonCoordToXamarinPoint(trueCoords));
                        }
                        break;
                    case "Polygon":
                        feature.Properties.TypeIconPath = "area_icon.png";
                        // Iterates the root coordinates (List<object>), and casts each element in the list to a Jarray, 
                        // then casts each Jarray's element to another Jarray which contain the actual coordinates.
                        for (int i = 0; i < feature.Geometry.Coordinates.Count; i++)
                        {
                            for (int j = 0; j < ((JArray)feature.Geometry.Coordinates[i]).Count; j++)
                            {
                                trueCoords = ((JArray)(((JArray)feature.Geometry.Coordinates[i])[j])).ToObject<object[]>();
                                feature.Properties.Xamarincoordinates.Add(JsonCoordToXamarinPoint(trueCoords));
                            }
                        }
                        break;
                    default:
                        await HomePage.Instance.DisplayAlert("Unsupported Feature", "Groundsman currently only supports feature types of Point, Line, and Polygon.", "OK");
                        return false;
                }

                // If author ID hasn't been set on the feature, default it to the user's ID.
                if (string.IsNullOrWhiteSpace(feature.Properties.AuthorId))
                {
                    feature.Properties.AuthorId = Preferences.Get("UserID", "Groundsman");
                }

                // Add default name if empty
                if (string.IsNullOrWhiteSpace(feature.Properties.Name))
                {
                    feature.Properties.Name = "Unnamed " + feature.Geometry.Type;
                }

                // If the date field is missing or invalid, convert it into DateTime.Now.
                if (feature.Properties.Date == null || DateTime.TryParse(feature.Properties.Date, out _) == false)
                {
                    feature.Properties.Date = DateTime.Now.ToShortDateString();
                }
                return true;
            }
            return false;
        }

        private Point JsonCoordToXamarinPoint(object[] coords)
        {
            double longitude = (double)coords[0];
            double latitude = (double)coords[1];
            double altitude = (coords.Length == 3) ? (double)coords[2] : 0.0;

            Point point = new Point(latitude, longitude, altitude);
            return point;
        }

        public void DeleteFeatureAsync(string featureID)
        {
            Feature featureToDelete = App.FeatureStore.CurrentFeatures.Find((feature) => (feature.Properties.Id == featureID));
            bool deleteSuccessful = App.FeatureStore.CurrentFeatures.Remove(featureToDelete);

            if (deleteSuccessful)
            {
                SaveCurrentFeaturesToEmbeddedFile();
            }
        }

        public void SaveFeatureAsync(Feature feature)
        {
            // If this is a newly added feature, generate an ID and add it immediately.
            if (feature.Properties.Id == AppConstants.NEW_ENTRY_ID)
            {
                feature.Properties.Id = Guid.NewGuid().ToString(); //TODO use existing method
                App.FeatureStore.CurrentFeatures.Add(feature);
            }
            else
            {
                // Otherwise we are saving over an existing feature, so override its contents without changing ID.
                int indexToEdit = -1;
                for (int i = 0; i < App.FeatureStore.CurrentFeatures.Count; i++)
                {
                    if (App.FeatureStore.CurrentFeatures[i].Properties.Id == feature.Properties.Id)
                    {
                        indexToEdit = i;
                        break;
                    }
                }

                if (indexToEdit != -1)
                {
                    App.FeatureStore.CurrentFeatures[indexToEdit] = feature;
                }
            }

            SaveCurrentFeaturesToEmbeddedFile();
        }

        public void SaveAllCurrentFeaturesAsync()
        {
            SaveCurrentFeaturesToEmbeddedFile();
        }

        /// <summary>
        /// Formats the list of current features into valid geojson, then writes it to the embedded file.
        /// </summary>
        /// <returns></returns>
        private void SaveCurrentFeaturesToEmbeddedFile()
        {
            var objToSave = FormatCurrentFeaturesIntoGeoJSON();

            // Save the rootobject to file.
            var json = JsonConvert.SerializeObject(objToSave);
            File.WriteAllText(AppConstants.FEATURES_FILE, json);

            // Mark the features list as dirty so it can refresh.
            MyFeaturesViewModel.isDirty = true;
        }

        /// <summary>
        /// Takes the current list of features and prepare the contents into a valid geoJSON serializable structure.
        /// </summary>
        /// <returns></returns>
        private RootObject FormatCurrentFeaturesIntoGeoJSON()
        {
            var rootobject = new RootObject();
            rootobject.Type = "FeatureCollection";
            rootobject.Features = App.FeatureStore.CurrentFeatures;

            foreach (var feature in rootobject.Features)
            {
                // Convert Lines back into LineStrings for valid geojson.
                if (feature.Geometry.Type == "Line")
                {
                    feature.Geometry.Type = "LineString";
                }
            }

            // Mark the features list as being modified, since the feature types had to be converted to LineStrings for export.
            // The dirty flag will make sure the line features in the list are refreshed back to "Line" types next time that page is viewed.
            MyFeaturesViewModel.isDirty = true;

            return rootobject;
        }

        public void DeleteAllFeatures()
        {
            App.FeatureStore.CurrentFeatures.Clear();
            SaveCurrentFeaturesToEmbeddedFile();
        }

        /// <summary>
        /// Imports features from the contents of a file.
        /// </summary>
        /// <param name="fileContents">The string of geojson to import from.</param>
        /// <returns></returns>
        public async Task<bool> ImportFeaturesAsync(string importContents)
        {
            // Ensure file contents are structured in a valid GeoJSON format.
            RootObject importedFeaturesData = null;
            RootObject validatedFeatures = new RootObject
            {
                Features = new List<Feature>()
            };
            int successfulImport = 0;
            int failedImport = 0;

            try
            {
                importedFeaturesData = JsonConvert.DeserializeObject<RootObject>(importContents);
            }
            catch (Exception ex)
            {
                await HomePage.Instance.DisplayAlert("Import Failed", string.Format("Groundsman can only import valid GeoJSON and encountered an error:\n{0}", ex.Message), "OK");
            }

            if (importedFeaturesData != null)
            {
                // Loop through all imported features and make sure they are valid and ensuring there are no ID clashes.
                foreach (var importedFeature in importedFeaturesData.Features)
                {
                    bool parseResult = await TryParseFeature(importedFeature);
                    if (parseResult)
                    {
                        EnsureUniqueID(importedFeature);

                        validatedFeatures.Features.Add(importedFeature);
                        successfulImport++;
                    }
                    else
                    {
                        failedImport++;
                    }

                }

                // Finally, add all the imported features to the current features list.
                App.FeatureStore.CurrentFeatures.AddRange(validatedFeatures.Features);

                SaveCurrentFeaturesToEmbeddedFile();
                await HomePage.Instance.DisplayAlert("Import Success", string.Format("{0} new features have been added to your features list. {1} features failed to import.", successfulImport, failedImport), "OK");
                MyFeaturesViewModel.isDirty = true;
                return true;
            }

            return false;


        }

        public async Task ImportFeaturesFromFile()
        {
            //TODO: exception handling - 
            try
            {
                var status = await CrossPermissions.Current.CheckPermissionStatusAsync(Permission.Storage);

                // If permissions allowed, prompt the user to pick a file.
                if (status == PermissionStatus.Granted)
                {
                    FileData fileData = await CrossFilePicker.Current.PickFile();

                    // If the user didn't cancel, import the contents of the file they selected.
                    if (fileData != null)
                    {
                        string contents = System.Text.Encoding.UTF8.GetString(fileData.DataArray);
                        await App.FeatureStore.ImportFeaturesAsync(contents);
                    }
                }
                else
                {

                    // Display storage permission popup if permission is not be established, display alert if the user declines 
                    if (await CrossPermissions.Current.ShouldShowRequestPermissionRationaleAsync(Permission.Storage))
                    {
                        await HomePage.Instance.DisplayAlert("Permissions Error", "Storage permissions for Groundsman must be enabled to utilise this feature.", "Ok", "OK");
                    }

                    // If the user accepts the permission get the resulting value and check the if the key exists
                    var results = await CrossPermissions.Current.RequestPermissionsAsync(Permission.Storage);
                    if (results.ContainsKey(Permission.Storage))
                    {
                        status = results[Permission.Storage];
                    }
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
                await App.FeatureStore.ImportFeaturesAsync(contents);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private static void EnsureUniqueID(Feature feature)
        {
            // Generate feature ID
            //TODO: avoid ID collisions
            if (feature.Properties.Id == null)
            {
                feature.Properties.Id = Guid.NewGuid().ToString();
            }
            else
            {
                foreach (var existingFeature in App.FeatureStore.CurrentFeatures)
                {
                    if (feature.Properties.Id == existingFeature.Properties.Id && feature != existingFeature)
                    {
                        feature.Properties.Id = Guid.NewGuid().ToString();
                    }
                }
            }
        }

        public string GetEmbeddedFile()
        {
            if (File.Exists(AppConstants.FEATURES_FILE))
            {
                return File.ReadAllText(AppConstants.FEATURES_FILE);
            }
            else
            {
                var assembly = IntrospectionExtensions.GetTypeInfo(this.GetType()).Assembly;
                Stream stream = assembly.GetManifestResourceStream("Groundsman.locationsAutoGenerated.json");
                string text = "";
                using (var reader = new System.IO.StreamReader(stream))
                {
                    text = reader.ReadToEnd();
                }
                return text;
            }
        }

        public string GetFeaturesFile()
        {
            if (File.Exists(AppConstants.FEATURES_FILE))
            {
                return File.ReadAllText(AppConstants.FEATURES_FILE);
            }
            else
            {
                var assembly = IntrospectionExtensions.GetTypeInfo(this.GetType()).Assembly;
                Stream stream = assembly.GetManifestResourceStream("Groundsman.locationsAutoGenerated.json");
                string text = "";
                using (var reader = new System.IO.StreamReader(stream))
                {
                    text = reader.ReadToEnd();
                }
                return text;
            }
        }

        /// <summary>
        /// Exports the current list of features by serializing to geojson.
        /// </summary>
        /// <returns></returns>
        public string ExportFeaturesToJson()
        {
            try
            {
                var rootobject = FormatCurrentFeaturesIntoGeoJSON();
                var json = JsonConvert.SerializeObject(rootobject, Formatting.Indented);

                // String cleaning
                if (json.StartsWith("[", StringComparison.Ordinal)) json = json.Substring(1);
                if (json.EndsWith("]", StringComparison.Ordinal)) json = json.TrimEnd(']');
                return json;

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

        public async Task CopyFeaturesToClipboard()
        {
            string textFile = GetEmbeddedFile();
            await Clipboard.SetTextAsync(textFile);
            await HomePage.Instance.DisplayAlert("Copy Features", "Features successfully copied to clipboard.", "OK");
        }

        /// <summary>
        /// Imports features from a specified filepath.
        /// </summary>
        /// <param name="path">path to file.</param>
        /// <returns></returns>
        public async Task<bool> ImportFeaturesFromFile(string path, string fileName)
        {
            var confirmation = await HomePage.Instance.DisplayAlert("Import File", $"Do you want to add the features in '{fileName}' to your features list?", "Yes", "No");
            if (confirmation)
            {
                try
                {
                    string text = File.ReadAllText(path);
                    bool resultStatus = await ImportFeaturesAsync(text);
                    MyFeaturesViewModel.isDirty = true;
                    return resultStatus;
                }
                catch (Exception)
                {
                    await HomePage.Instance.DisplayAlert("Import Error", "An unknown error occured when trying to process this file.", "OK");
                }
            }
            return false;
        }


    }
}
