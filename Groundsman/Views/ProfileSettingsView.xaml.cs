using System;
using System.Diagnostics;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Groundsman
{
    public partial class ProfileSettingsView : ContentPage
    {
        public ProfileSettingsView()
        {
            InitializeComponent();
        }

        async void OnDismissButtonClicked(object sender, EventArgs args)
        {
            await Navigation.PopModalAsync();
        }

        async void OnViewCellTapped(object sender, EventArgs e)
        {
            bool yesResponse = await HomePage.Instance.DisplayAlert("Reset User Data", "This will permanently erase all saved features. Do you wish to continue?", "Yes", "No");
            if (yesResponse)
            {
                App.FeatureStore.DeleteAllFeatures();
                await HomePage.Instance.DisplayAlert("Reset User Data", "Your user data has been erased.", "Ok");
            }
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
            Debug.WriteLine(selectedIndex);
            if (selectedIndex != -1)
            {
                Preferences.Set("GPSPrecision", selectedIndex);
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
    }
}