using System;
using Groundsman.ViewModels;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.PlatformConfiguration;
using Xamarin.Forms.PlatformConfiguration.iOSSpecific;

namespace Groundsman.Views
{
    public partial class EditFeatureDetailsView : ContentPage
    {
        private readonly BaseFeatureDetailsViewModel viewModel;
        /// <summary>
        /// Detail form constructor for when a new entry is being added.
        /// </summary>
        /// <param name="type">The geoJSON geometry type being added.</param>
        public EditFeatureDetailsView(FeatureDetailsViewModel featureDetailsViewModel)
        {
            InitializeComponent();
            BindingContext = viewModel = featureDetailsViewModel;
        }

        public EditFeatureDetailsView(LoggerViewModel loggerViewModel)
        {
            InitializeComponent();
            BindingContext = viewModel = loggerViewModel;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            if (DeviceInfo.Platform == DevicePlatform.iOS && DeviceInfo.Version < new Version(13, 0))
            {
                Thickness safeInsets = On<iOS>().SafeAreaInsets();
                safeInsets.Bottom = 0;
                Padding = safeInsets;
            }
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();

            viewModel.DiscardDismiss();
        }
    }
}
