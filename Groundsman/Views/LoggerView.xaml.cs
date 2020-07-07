using Groundsman.ViewModels;
using Xamarin.Forms;

namespace Groundsman
{
    public partial class LoggerView : ContentPage
    {
        LoggerViewModel viewModel;
        private bool appeared = false;
        public LoggerView()
        {
            InitializeComponent();
            BindingContext = viewModel = new LoggerViewModel();
            appeared = true;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
        }

        private void LogLabel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (appeared)
            {
                ScrollBox.ScrollToAsync(logLabel, ScrollToPosition.End, true);
            }
        }
    }
}