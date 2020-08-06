using Groundsman.Models;
using Groundsman.Services;
using Plugin.FilePicker;
using Plugin.FilePicker.Abstractions;
using System;
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
                        await FeatureStore.ImportFeaturesAsync(contents, true);
                    }
                }
                else
                {
                    await NavigationService.ShowAlert("Permissions Error", "Storage permissions for Groundsman must be enabled to utilise this feature.", false);
                }
            }
            catch (Exception ex)
            {
                await NavigationService.ShowAlert("Import Error", $"File must contain valid GeoJSON and be accessible to Groundsman. {ex}", false);
            }
        }
    }
}
