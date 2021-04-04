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

        protected override void OnAppearing()
        {
            base.OnAppearing();
        }
    }
}