using Xamarin.Essentials;

namespace Groundsman.ViewModels
{
    public class ProfileSettingsViewModel : BaseViewModel
    {
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
            IDEntry = Preferences.Get("UserID", "Groundsman");
            DecimalAccuracyEntry = Preferences.Get("DataDecimalAccuracy", 6);
            GPSPrecisionEntry = Preferences.Get("GPSPrecision", 2);
            ShowPointsOnMap = Preferences.Get("ShowPointsOnMap", true);
            ShowLinesOnMap = Preferences.Get("ShowLinesOnMap", true);
            ShowPolygonsOnMap = Preferences.Get("ShowPolygonsOnMap", true);
            ShowLogPathOnMap = Preferences.Get("ShowLogPathOnMap", false);
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
    }
}
