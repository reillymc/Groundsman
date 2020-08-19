using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Groundsman.ViewModels
{
    public class ProfileSettingsViewModel : BaseViewModel
    {
        public Command DeleteAllFeatures { get; set; }
        public Command InfoButtonTappedCommand { get; set; }

        private string _IDEntry;
        public string IDEntry
        {
            get { return _IDEntry; }
            set
            {
                _IDEntry = value;
                HandleTextChanged();
            }
        }

        public int DecimalAccuracyEntry { get; set; }
        public int GPSPrecisionEntry { get; set; }
        public bool ShowPointsOnMap { get; set; }
        public bool ShowLinesOnMap { get; set; }
        public bool ShowPolygonsOnMap { get; set; }
        public bool ShowLogPathOnMap { get; set; }

        public ProfileSettingsViewModel()
        {
            Title = "Settings";
            DeleteAllFeatures = new Command(async () => await ExecuteDeleteAllFeaturesCommand());
            InfoButtonTappedCommand = new Command(async () => await NavigationService.ShowAlert("Credits", "Development:\nReilly MacKenzie-Cree\nGeorge Delosa\nAri Luangamath\n\nIcons by Icons8\nhttps://icons8.com", false));
            UpdatePreferences();
        }

        private async Task ExecuteDeleteAllFeaturesCommand()
        {
            bool yesResponse = await NavigationService.ShowAlert("Reset User Data", "This will permanently erase all saved features. Do you wish to continue?", true);
            if (yesResponse)
            {
                await FeatureStore.DeleteItemsAsync();
                await NavigationService.ShowAlert("Reset User Data", "Your user data has been erased.", false);
            }
        }

        private void HandleTextChanged()
        {
            if (string.IsNullOrWhiteSpace(IDEntry) == false)
            {
                Preferences.Set("UserID", IDEntry);
            }
            else
            {
                Preferences.Set("UserID", "Groundsman");
            }
        }

        public void UpdatePreferences()
        {
            IDEntry = Preferences.Get("UserID", "Groundsman");
            DecimalAccuracyEntry = Preferences.Get("DataDecimalAccuracy", 6);
            GPSPrecisionEntry = Preferences.Get("GPSPrecision", 2);
            ShowPointsOnMap = Preferences.Get("ShowPointsOnMap", true);
            ShowLinesOnMap = Preferences.Get("ShowLinesOnMap", true);
            ShowPolygonsOnMap = Preferences.Get("ShowPolygonsOnMap", true);
            ShowLogPathOnMap = Preferences.Get("ShowLogPathOnMap", false);
        }
    }
}
