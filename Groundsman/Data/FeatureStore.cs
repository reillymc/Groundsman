using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xamarin.Essentials;

namespace Groundsman.Data
{
    public class FeatureStore
    {
        public List<Feature> CurrentFeatures { get; set; } = new List<Feature>();
        private readonly string DATA_PATH = FileSystem.AppDataDirectory + "/";
        private const string EMBEDDED_FILENAME = "locations.json";
        private const string LOG_FILENAME = "log.csv";

        public Task<List<Feature>> GetFeaturesAsync()
        {
            return Task.Run(async () =>
            {
                string featuresFile = GetFeaturesFile();
                var rootobject = JsonConvert.DeserializeObject<RootObject>(featuresFile);
                if (rootobject == null)
                {
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
            if (feature == null || feature.Type == null || feature.Geometry == null || feature.Geometry.Type == null || feature.Geometry.Coordinates == null)
            {
                await HomePage.Instance.DisplayAlert("Invalid File", "Ensure your file only contains data in valid GeoJSON format.", "OK");
                return false;
            }

            // Immediately convert LineStrings to Line for use in the rest of the codebase. 
            // This will be converted back to LineString before serialization back to json.
            if (feature.Geometry.Type == "LineString")
            {
                feature.Geometry.Type = "Line";
            }

            if (string.IsNullOrWhiteSpace(feature.Properties.Name))
            {
                feature.Properties.Name = "Unnamed " + feature.Geometry.Type;
            }

            // If author ID hasn't been set on the feature, default it to the user's ID.
            if (string.IsNullOrWhiteSpace(feature.Properties.AuthorId))
            {
                feature.Properties.AuthorId = Preferences.Get("UserID", "Groundsman");
            }

            // If the date field is missing or invalid, convert it into DateTime.Now.
            DateTime dummy;
            if (feature.Properties.Date == null || DateTime.TryParse(feature.Properties.Date, out dummy) == false)
            {
                feature.Properties.Date = DateTime.Now.ToShortDateString();
            }

            // Determine the icon used for each feature based on it's geometry type.
            if (feature.Geometry.Type == "Point")
            {
                feature.Properties.TypeIconPath = "point_icon.png";
            }
            else if (feature.Geometry.Type == "Line")
            {
                feature.Properties.TypeIconPath = "line_icon.png";
            }
            else if (feature.Geometry.Type == "Polygon")
            {
                feature.Properties.TypeIconPath = "area_icon.png";
            }
            else
            {
                await HomePage.Instance.DisplayAlert("Import Error", "Groundsman currently only supports feature types of Point, Line, and Polygon.", "OK");
                return false;
            }

            // Initialise xamarin coordinates list.
            feature.Properties.Xamarincoordinates = new List<Point>();

            // Properly deserialize the list of coordinates into an app-use-specific list of Points (XamarinCoordinates).
            {
                object[] trueCoords;

                if (feature.Geometry.Type == "Point")
                {
                    trueCoords = feature.Geometry.Coordinates.ToArray();
                    feature.Properties.Xamarincoordinates.Add(JsonCoordToXamarinPoint(trueCoords));

                }
                else if (feature.Geometry.Type == "Line")
                {
                    // Iterates the root coordinates (List<object>),
                    // then casts each element in the list to a Jarray which contain the actual coordinates.
                    for (int i = 0; i < feature.Geometry.Coordinates.Count; i++)
                    {
                        trueCoords = ((JArray)feature.Geometry.Coordinates[i]).ToObject<object[]>();
                        feature.Properties.Xamarincoordinates.Add(JsonCoordToXamarinPoint(trueCoords));
                    }
                }
                else if (feature.Geometry.Type == "Polygon")
                {
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
                }
                else
                {
                    return false;
                }
            }

            return true;
        }

        private Point JsonCoordToXamarinPoint(object[] coords)
        {
            double longitude = (double)coords[0];
            double latitude = (double)coords[1];
            double altitude = (coords.Length == 3) ? (double)coords[2] : 0.0;

            Point point = new Point(latitude, longitude, altitude);
            return point;
        }

        public void DeleteFeatureAsync(int featureID)
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
                TryGetUniqueFeatureID(feature);
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
            File.WriteAllText(Path.Combine(FileSystem.AppDataDirectory, EMBEDDED_FILENAME), json);

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
        public async Task<bool> ImportFeaturesAsync(string fileContents)
        {
            try
            {
                // Ensure file contents are structured in a valid GeoJSON format.
                var importedRootObject = JsonConvert.DeserializeObject<RootObject>(fileContents);
                if (importedRootObject == null)
                {
                    await HomePage.Instance.DisplayAlert("Invalid File Contents", "Ensure your file only contains data in valid GeoJSON format.", "OK");
                    return false;
                }

                // Loop through all imported features and make sure they are valid.
                foreach (var importedFeature in importedRootObject.Features)
                {
                    bool parseResult = await TryParseFeature(importedFeature);
                    //TODO: importedFeature.properties.authorId etc - clense data on import
                    if (parseResult == false)
                    {
                        return false;
                    }
                }

                // Loop through all imported features one by one, ensuring there are no ID clashes.
                foreach (var importedFeature in importedRootObject.Features)
                {
                    TryGetUniqueFeatureID(importedFeature);
                }

                // Finally, add all the imported features to the current features list.
                App.FeatureStore.CurrentFeatures.AddRange(importedRootObject.Features);

                SaveCurrentFeaturesToEmbeddedFile();
                await HomePage.Instance.DisplayAlert("Import Success", "New features have been added to your features list.", "OK");
                MyFeaturesViewModel.isDirty = true;
                return true;
            }
            catch (Exception)
            {
                await HomePage.Instance.DisplayAlert("Invalid File Contents", "Ensure your file only contains data in valid GeoJSON format.", "OK");
                return false;
            }
        }

        public string GetLogFile()
        {
            // Attempt to open the embedded file on the device. 
            // If it exists return it, else create a new embedded file from a json source file.
            if (File.Exists(DATA_PATH + LOG_FILENAME))
            {
                return File.ReadAllText(DATA_PATH + LOG_FILENAME);
            }
            else
            {
                return "";
            }
        }

        public List<Point> GetLogFileObject()
        {
            // Attempt to open the embedded file on the device. 
            // If it exists return it, else create a new embedded file from a json source file.
            if (File.Exists(DATA_PATH + LOG_FILENAME))
            {
                List<Point> logList = File.ReadAllLines(DATA_PATH + LOG_FILENAME).Select(x => new Point
                (
                    x[1],
                    x[2],
                    x[3]
                )).ToList();
                return logList;
            }
            else
            {
                return null;
            }
        }

        public string GetEmbeddedFile()
        {
            if (File.Exists(DATA_PATH + EMBEDDED_FILENAME))
            {
                return File.ReadAllText(DATA_PATH + EMBEDDED_FILENAME);
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
            if (File.Exists(Path.Combine(FileSystem.AppDataDirectory, EMBEDDED_FILENAME)))
            {
                return File.ReadAllText(Path.Combine(FileSystem.AppDataDirectory, EMBEDDED_FILENAME));
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

        /// <summary>
        /// If necessary, creates a new ID that is unique to all current features stored.
        /// </summary>
        /// <returns>The original ID if no clashes were found, else a new unique ID.</returns>
        public static void TryGetUniqueFeatureID(Feature featureToCheck)
        {
            bool validID = false;

            while (validID == false)
            {
                validID = true;

                if (featureToCheck.Properties.Id == AppConstants.NEW_ENTRY_ID)
                {
                    validID = false;
                }
                else
                {
                    foreach (var feature in App.FeatureStore.CurrentFeatures)
                    {
                        if (featureToCheck.Properties.Id == feature.Properties.Id && featureToCheck != feature)
                        {
                            validID = false;
                            break;
                        }
                    }
                }

                if (validID == false)
                {
                    featureToCheck.Properties.Id = DateTime.Now.GetHashCode();
                }
            }
        }
    }
}
