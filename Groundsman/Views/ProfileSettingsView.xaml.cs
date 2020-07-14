using Groundsman.ViewModels;
using System;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Groundsman
{
    public partial class ProfileSettingsView : ContentPage
    {
        ProfileSettingsViewModel viewModel;
        public ProfileSettingsView()
        {
            InitializeComponent();
            BindingContext = viewModel = new ProfileSettingsViewModel();
        }

        async void OnDismissButtonClicked(object sender, EventArgs args)
        {
            await Navigation.PopModalAsync();
        }

        void OnStepperValueChanged(object sender, ValueChangedEventArgs e)
        {
            int value = (int)e.NewValue;
            Preferences.Set("DataDecimalAccuracy", value);
        }

        void OnPickerSelectedIndexChanged(object sender, EventArgs e)
        {
            var picker = (Picker)sender;
            int selectedIndex = picker.SelectedIndex;
            if (selectedIndex != -1)
            {
                Preferences.Set("GPSPrecision", selectedIndex);
            }
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            viewModel.UpdatePreferences();
        }

        private void ShowPointsOnMapChanged(object sender, ToggledEventArgs e)
        {
            Preferences.Set("ShowPointsOnMap", e.Value);
        }

        private void ShowLinesOnMapChanged(object sender, ToggledEventArgs e)
        {
            Preferences.Set("ShowLinesOnMap", e.Value);
        }

        private void ShowPolygonsOnMapChanged(object sender, ToggledEventArgs e)
        {
            Preferences.Set("ShowPolygonsOnMap", e.Value);
        }

        private void ShowLogPathOnMapChanged(object sender, ToggledEventArgs e)
        {
            Preferences.Set("ShowLogPathOnMap", e.Value);
        }
    }
}