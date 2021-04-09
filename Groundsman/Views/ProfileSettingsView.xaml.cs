using System;
using Groundsman.ViewModels;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Groundsman.Views
{
    public partial class ProfileSettingsView : ContentPage
    {
        ProfileSettingsViewModel viewModel;

        public ProfileSettingsView()
        {
            InitializeComponent();
            BindingContext = viewModel = new ProfileSettingsViewModel();
        }
    }
}