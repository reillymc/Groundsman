﻿using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Groundsman.ViewModels
{
    public class SettingsViewModel : BaseViewModel
    {
        public Command DeleteAllFeatures { get; set; }
        public Command InfoButtonTappedCommand { get; set; }

        public string IDEntry
        {
            get => Preferences.Get(Constants.UserIDKey, Constants.DefaultUserValue);
            set
            {
                if (!string.IsNullOrWhiteSpace(value))
                {
                    Preferences.Set(Constants.UserIDKey, value);
                }
                else
                {
                    Preferences.Set(Constants.UserIDKey, Constants.DefaultUserValue);
                }
            }
        }

        public int DecimalAccuracyEntry
        {
            get => Preferences.Get(Constants.DecimalAccuracyKey, Constants.DefaultDecimalAccuracyValue);
            set => Preferences.Set(Constants.DecimalAccuracyKey, value);
        }
        public int GPSPrecisionEntry
        {
            get => Preferences.Get(Constants.GPSPrecisionKey, Constants.DefaultGPSPrecisionValue);
            set => Preferences.Set(Constants.GPSPrecisionKey, value);
        }

        public int ListOrdering
        {
            get => Preferences.Get(Constants.ListOrderingKey, Constants.DefaultListOrderingValue);
            set {
                Preferences.Set(Constants.ListOrderingKey, value);
                _ = FeatureStore.GetItemsAsync();
                }
        }

        public bool EnableShakeToUndo
        {
            get => Preferences.Get(Constants.ShakeToUndoKey, true);
            set => Preferences.Set(Constants.ShakeToUndoKey, value);
        }

        public bool EnableMapPreview
        {
            get => Preferences.Get(Constants.MapPreviewKey, true);
            set => Preferences.Set(Constants.MapPreviewKey, value);
        }

        public int LoggerExportFormat
        {
            get => Preferences.Get(Constants.LoggerExportFormatKey, Constants.LoggerExportFormatDefaultValue);
            set => Preferences.Set(Constants.LoggerExportFormatKey, value);
        }

        public bool ShowPointsOnMap
        {
            get => Preferences.Get(Constants.MapDrawPointsKey, true);
            set => Preferences.Set(Constants.MapDrawPointsKey, value);
        }
        public bool ShowLinesOnMap
        {
            get => Preferences.Get(Constants.MapDrawLinesKey, true);
            set => Preferences.Set(Constants.MapDrawLinesKey, value);
        }
        public bool ShowPolygonsOnMap
        {
            get => Preferences.Get(Constants.MapDrawPolygonsKey, true);
            set => Preferences.Set(Constants.MapDrawPolygonsKey, value);
        }

        public SettingsViewModel()
        {
            Title = "Settings";
            DeleteAllFeatures = new Command(async () => await ExecuteDeleteAllFeaturesCommand());
            InfoButtonTappedCommand = new Command(async () => await NavigationService.ShowAlert("Credits", "Development:\nReilly MacKenzie-Cree\nGeorge Delosa\nAri Luangamath", false));
        }

        private async Task ExecuteDeleteAllFeaturesCommand()
        {
            bool yesResponse = await NavigationService.ShowAlert("Reset Feature List?", "This will permanently erase all saved features. Do you wish to continue?", true);
            if (yesResponse)
            {
                await FeatureStore.ClearItems();
                _ = await FeatureStore.GetItemsAsync();
            }
        }
    }
}
