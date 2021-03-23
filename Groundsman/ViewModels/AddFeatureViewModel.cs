using Groundsman.Models;
using Groundsman.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Groundsman.ViewModels
{
    public class AddFeatureViewModel : BaseViewModel
    {
        public Command AddFeatureCommand { set; get; }
        public Command OnCancelTappedCommand { get; set; }

        public AddFeatureViewModel()
        {
            AddFeatureCommand = new Command<string>(async (id) => await AddFeatureAsync(id));
            OnCancelTappedCommand = new Command(async () => await OnDismiss(true));
            Title = "Add Features";
        }

        private async Task AddFeatureAsync(string id)
        {
            await NavigationService.NavigateBack(true);
            switch (id)
            {
                case "Point":
                    await NavigationService.NavigateToNewEditPage(FeatureType.Point);
                    break;
                case "LineString":
                    await NavigationService.NavigateToNewEditPage(FeatureType.LineString);
                    break;
                case "Polygon":
                    await NavigationService.NavigateToNewEditPage(FeatureType.Polygon);
                    break;
                case "Clipboard":
                    string contents = await Clipboard.GetTextAsync();
                    await FeatureStore.ImportFeaturesAsync(contents, true);
                    break;
                case "File":
                    await ImportFeaturesFromFile();
                    break;
            }
        }

        public async Task ImportFeaturesFromFile()
        {
            try
            {
                var customFileType =
                    new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
                    {
                            { DevicePlatform.iOS, new[] { "public.json", "com.apple.dt.document.geojson" } }, // or general UTType values
                            { DevicePlatform.Android, new[] { "application/json", "application/geo+json" } },
                            { DevicePlatform.UWP, new[] { ".json", ".geojson" } },
                            { DevicePlatform.macOS, new[] { "json", "geojson" } }, // or general UTType values
                    });

                var options = new PickOptions
                {
                    PickerTitle = "Please select a CheckSafe template file",
                    FileTypes = customFileType,
                };
                var fileData = await FilePicker.PickAsync();

                // If the user didn't cancel, import the contents of the file they selected.
                if (fileData != null)
                {
                    var fileStream = await fileData.OpenReadAsync();

                    StreamReader reader = new StreamReader(fileStream);
                    string fileContents = reader.ReadToEnd();

                    int success = await FeatureStore.ImportFeaturesAsync(fileContents, true);
                }
            }
            catch
            {
                await NavigationService.ShowAlert("Import Error", $"Please allow Groundsman to access device storage.", false);
            }
        }
    }
}
