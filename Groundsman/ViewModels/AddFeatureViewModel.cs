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

        private bool modal;

        public AddFeatureViewModel(bool modal)
        {
            this.modal = modal;
            AddFeatureCommand = new Command<string>(async (id) => await AddFeatureAsync(id));
        }

        private async Task AddFeatureAsync(string id)
        {
            switch (id)
            {
                case "Point":
                    await navigationService.NavigateBack(modal);
                    await navigationService.NavigateToNewEditPage("Point");
                    break;

                case "LineString":
                    await navigationService.NavigateBack(modal);
                    await navigationService.NavigateToNewEditPage("LineString");
                    break;

                case "Polygon":
                    await navigationService.NavigateBack(modal);
                    await navigationService.NavigateToNewEditPage("Polygon");
                    break;

                case "Clipboard":
                    string contents = await Clipboard.GetTextAsync();
                    await featureStore.ImportFeaturesAsync(contents, true);
                    await navigationService.NavigateBack(modal);
                    break;

                case "File":
                    await ImportFeaturesFromFile();
                    await navigationService.NavigateBack(modal);
                    break;
                default:
                    await navigationService.NavigateBack(modal);
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
                        await featureStore.ImportFeaturesAsync(contents, true);
                    }
                }
                else
                {
                    await HomePage.Instance.DisplayAlert("Permissions Error", "Storage permissions for Groundsman must be enabled to utilise this feature.", "OK");
                }
            }
            catch (Exception ex)
            {
                await HomePage.Instance.DisplayAlert("Import Error", $"File must contain valid GeoJSON and be accessible to Groundsman. {ex}", "OK");
            }
        }
    }
}
