using Groundsman.Models;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Groundsman.ViewModels;

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
                await NavigationService.NavigateToNewEditPage(GeoJSONType.Point);
                break;
            case "LineString":
                await NavigationService.NavigateToNewEditPage(GeoJSONType.LineString);
                break;
            case "Polygon":
                await NavigationService.NavigateToNewEditPage(GeoJSONType.Polygon);
                break;
            case "Clipboard":
                await ImportRawGeoJSON(await Clipboard.GetTextAsync());
                break;
            case "File":
                await ImportFeaturesFromFile();
                break;
            case "Log":
                await NavigationService.NavigateToNewLoggerPage();
                break;
        }
    }


    public async Task ImportFeaturesFromFile()
    {
        try
        {
            var customFileType = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
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
                var reader = new StreamReader(fileStream);
                string fileContents = reader.ReadToEnd();
                await ImportRawGeoJSON(fileContents);
            }
        }
        catch
        {
            await NavigationService.ShowAlert("Import Error", $"Please allow Groundsman to access device storage.", false);
        }
    }

    public async Task ImportRawGeoJSON(string contents)
    {
        try
        {
            int successfulImports = await FeatureStore.ImportRawContents(contents);
            _ = await FeatureStore.GetItemsAsync();
            await NavigationService.ShowImportAlert(successfulImports);
        }
        catch (Exception ex)
        {
            await NavigationService.ShowAlert("Import Error", ex.Message, false);
        }
    }

}
