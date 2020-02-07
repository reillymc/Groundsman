using System;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Groundsman
{
    public class ProfileSettingsViewModel : ViewModelBase
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


        public ProfileSettingsViewModel()
        {
            if (Application.Current.Properties.ContainsKey("UserID") == true)
            {
                _IDEntry = Application.Current.Properties["UserID"] as string;
            }
            DecimalAccuracyEntry = Preferences.Get("DataDecimalAccuracy", 8);
            GPSPrecisionEntry = (Preferences.Get("GPSPrecision", 2));
        }

        private void HandleTextChanged()
        {
            if (string.IsNullOrWhiteSpace(IDEntry) == false)
            {
                if (IDEntry.Length <= 30)
                {
                    Application.Current.Properties["UserID"] = IDEntry;
                }
                else
                {
                    Application.Current.Properties["UserID"] = IDEntry.Substring(0, 30);
                }
            }
            else
            {
                Application.Current.Properties["UserID"] = "Default";
            }
        }

        private void HandlePrefsChanged()
        {
            //DecimalAccuracyValue = Preferences.Get("DataDecimalAccuracy", 8);
        }
    }
}
