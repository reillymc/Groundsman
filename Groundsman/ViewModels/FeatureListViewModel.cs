using Groundsman.Misc;
using Groundsman.Models;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Groundsman.ViewModels;

/// <summary>
/// View-model for the page that shows the list of data entries.
/// </summary>
public class FeatureListViewModel : BaseViewModel
{
    public Command AddButtonTappedCommand { set; get; }
    public Command ShareButtonTappedCommand { set; get; }
    public Command ItemTappedCommand { set; get; }
    public Command DeleteEntryCommand { get; set; }
    public Command ShareEntryCommand { get; set; }

    /// <summary>
    /// View-model constructor.
    /// </summary>
    public FeatureListViewModel()
    {
        AddButtonTappedCommand = new Command(async () => await AddButtonTapped());
        ShareButtonTappedCommand = new Command(async () => await ShowShareSheet());
        ItemTappedCommand = new Command<Feature>(async (feature) => await ShowFeatureDetailsPage(feature));
        DeleteEntryCommand = new Command<Feature>(async (feature) => await DeleteFeature(feature));
        ShareEntryCommand = new Command<Feature>(async (feature) => await ShareFeature(feature));

        Title = "My Features";
    }

    public async Task ShareFeature(Feature feature)
    {
        ShareFileRequest share = new ShareFileRequest
        {
            Title = "Share Feature",
            File = new ShareFile(await FeatureHelper.ExportFeatures(feature), "application/json")
        };
        share.PresentationSourceBounds = DeviceInfo.Platform == DevicePlatform.iOS && DeviceInfo.Idiom == DeviceIdiom.Tablet ? new System.Drawing.Rectangle(0, 20, 0, 0) : System.Drawing.Rectangle.Empty;
        await Share.RequestAsync(share);
    }

    /// <summary>
    /// Shows native share sheet that exports all features.
    /// </summary>
    /// <returns></returns>
    private async Task ShowShareSheet()
    {
        if (IsBusy) return;
        IsBusy = true;

        ShareFileRequest share = new ShareFileRequest
        {
            Title = "Share Features",
            File = new ShareFile(await FeatureHelper.ExportFeatures(FeatureList), "application/json"),
        };
        share.PresentationSourceBounds = DeviceInfo.Platform == DevicePlatform.iOS && DeviceInfo.Idiom == DeviceIdiom.Tablet
                ? new System.Drawing.Rectangle((int)(DeviceDisplay.MainDisplayInfo.Width * .474), 80, 0, 0)
                : System.Drawing.Rectangle.Empty;
        await Share.RequestAsync(share);

        IsBusy = false;
    }

    /// <summary>
    /// Opens up the dialog box where the user can select between Point, Line, and Polygon feature types to add.
    /// </summary>
    /// <returns></returns>
    private async Task AddButtonTapped()
    {
        if (IsBusy) return;
        IsBusy = true;

        await NavigationService.PushAddFeaturePage();

        IsBusy = false;
    }

    /// <summary>
    /// Displays the edit page for the selected feature.
    /// </summary>
    /// <param name="feature">Feature to edit.</param>
    private async Task ShowFeatureDetailsPage(Feature feature)
    {
        if (IsBusy) return;
        IsBusy = true;
        if (feature.Properties.ContainsKey(Constants.LogTimestampsProperty))
        {
            await NavigationService.NavigateToLoggerPage(feature);
        }
        else
        {
            await NavigationService.NavigateToEditPage(feature);
        }
        IsBusy = false;
    }

    /// <summary>
    /// Call the Feature Store to delete seleted feature.
    /// </summary>
    /// <param name="feature">Feature to delete.</param>
    /// <returns></returns>
    private async Task DeleteFeature(Feature feature)
    {
        if (IsBusy) return;
        IsBusy = true;

        shakeService.Start();
        await FeatureStore.DeleteItem(feature.Id);
        await FeatureStore.GetItemsAsync();

        IsBusy = false;
    }
}
