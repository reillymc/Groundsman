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

        async void OnViewCellTapped(object sender, EventArgs e)
        {
            bool yesResponse = await HomePage.Instance.DisplayAlert("Reset User Data", "This will permanently erase all saved features. Do you wish to continue?", "Yes", "No");
            if (yesResponse)
            {
                //App.FeatureStore.DeleteAllFeatures();
                await HomePage.Instance.DisplayAlert("Reset User Data", "Your user data has been erased.", "Ok");
            }
        }


        protected override async void OnDisappearing()
        {
            base.OnDisappearing();

            if (Navigation.ModalStack.Count > 0)
            {
                await Navigation.PopModalAsync();
            }
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