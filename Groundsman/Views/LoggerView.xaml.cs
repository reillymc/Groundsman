using Groundsman.Models;
using Groundsman.ViewModels;
using Xamarin.Forms;

namespace Groundsman.Views
{
    public partial class LoggerView : ContentPage
    {
        LoggerViewModel viewModel;
        public LoggerView()
        {
            InitializeComponent();
            BindingContext = viewModel = new LoggerViewModel();
        }

        public LoggerView(Feature feature)
        {
            InitializeComponent();
            BindingContext = viewModel = new LoggerViewModel(feature);
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            
            if (viewModel.isLogging)
            {
                viewModel.ToggleLogging();
            }
            viewModel.Unsubscribe();
        }
    }
}