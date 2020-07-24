using Groundsman.Services;
using Groundsman.ViewModels;
using System;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.PlatformConfiguration;
using Xamarin.Forms.PlatformConfiguration.iOSSpecific;

namespace Groundsman
{
    public partial class AddFeatureView : ContentPage
    {
        AddFeatureViewModel viewModel;
        bool modal;

        public AddFeatureView(bool modal)
        {
            InitializeComponent();
            BindingContext = viewModel = new AddFeatureViewModel(modal);
            this.modal = modal;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            if (DeviceInfo.Platform == DevicePlatform.iOS && DeviceInfo.Version < new Version(13, 0))
            {
                var safeInsets = On<iOS>().SafeAreaInsets();
                safeInsets.Bottom = 0;
                Padding = safeInsets;
            }

        }

        async void OnDismissButtonClicked(object sender, EventArgs args)
        {
            NavigationService navigationService = new NavigationService();
            await navigationService.NavigateBack(modal);
        }
    }
}
