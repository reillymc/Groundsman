using Groundsman.ViewModels;
using System;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.PlatformConfiguration;
using Xamarin.Forms.PlatformConfiguration.iOSSpecific;

namespace Groundsman.Views
{
    public partial class AddFeatureView : ContentPage
    {
        AddFeatureViewModel viewModel;

        public AddFeatureView()
        {
            InitializeComponent();
            BindingContext = viewModel = new AddFeatureViewModel();
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
    }
}
