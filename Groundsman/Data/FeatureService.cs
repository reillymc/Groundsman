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
using System.Collections.Generic;

namespace Groundsman.Services
{
    public class FeatureService
    {

        ///// <summary>
        ///// clears all features and reloads the example featureset
        ///// </summary>
        //public static void DeleteAllFeatures()
        //{
        //    SaveCurrentFeaturesToEmbeddedFile();

        //    _ = ImportFeaturesAsync(GetDefaultFile(), false);
        //}

        /// <summary>
        /// Imports features from a specified filepath.
        /// </summary>
        /// <param name="path">path to file.</param>
        /// <returns></returns>
        public async Task<ObservableCollection<Feature>> ImportFeaturesFromFileURL(string path, string fileName)
        {
            var confirmation = await HomePage.Instance.DisplayAlert("Import File", $"Do you want to add the features in '{fileName}' to your features list?", "Yes", "No");
            if (confirmation)
            {
                try
                {
                    string text = File.ReadAllText(path);
                    //ObservableCollection<Feature> Imported = await ImportFeaturesAsync(text, true);
                    //return Imported;
                }
                catch (Exception)
                {
                    await HomePage.Instance.DisplayAlert("Import Error", "An unknown error occured when trying to process this file.", "OK");
                }
            }
            return null;
        }

        public async Task ImportFeaturesFromFile()
        {
            //TODO: exception handling - 
            try
            {
                var status = await HelperServices.CheckAndRequestPermissionAsync(new Permissions.StorageRead());

                // If permissions allowed, prompt the user to pick a file.
                if (status == PermissionStatus.Granted)
                {
                    FileData fileData = await CrossFilePicker.Current.PickFile();

                    // If the user didn't cancel, import the contents of the file they selected.
                    if (fileData != null)
                    {
                        string contents = System.Text.Encoding.UTF8.GetString(fileData.DataArray);
                        //await ImportFeaturesAsync(contents, true);
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
                //await ImportFeaturesAsync(contents, true);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static async Task ExportFeatures()
        {
            await Share.RequestAsync(new ShareFileRequest
            {
                Title = "Features Export",
                File = new ShareFile(AppConstants.FEATURES_FILE, "text/plain")
            });
        }

        public static async Task ExportFeature(Feature feature)
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

        public async Task CopyFeaturesToClipboard(ObservableCollection<Feature> features)
        {
            GeoJSONObject geoJSONObject = new GeoJSONObject
            {
                type = "FeatureCollection",
                features = features
            };
            string textFile = JsonConvert.SerializeObject(geoJSONObject, Formatting.Indented);
            await Clipboard.SetTextAsync(textFile);
            await HomePage.Instance.DisplayAlert("Copy Features", "Features successfully copied to clipboard.", "OK");
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="feature"></param>
        public static void EnsureUniqueID(Feature feature)
        {
            // Generate feature ID
            //TODO: avoid ID collisions
            feature.properties.id = Guid.NewGuid().ToString();
        }

    }
}