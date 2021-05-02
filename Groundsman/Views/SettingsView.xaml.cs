using Groundsman.ViewModels;
using Xamarin.Forms;

namespace Groundsman.Views
{
    public partial class SettingsView : ContentPage
    {
        private readonly SettingsViewModel viewModel;

        public SettingsView()
        {
            InitializeComponent();
            BindingContext = viewModel = new SettingsViewModel();
        }
    }
}