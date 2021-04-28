using Groundsman.ViewModels;
using Xamarin.Forms;

namespace Groundsman.Views
{
    public partial class ProfileSettingsView : ContentPage
    {
        private readonly ProfileSettingsViewModel viewModel;

        public ProfileSettingsView()
        {
            InitializeComponent();
            BindingContext = viewModel = new ProfileSettingsViewModel();
        }
    }
}