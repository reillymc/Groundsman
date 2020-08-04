using Groundsman.ViewModels;
using Xamarin.Forms;

namespace Groundsman.Views
{
    public partial class MapView : ContentPage
    {
        MapViewModel viewModel;
        public MapView()
        {
            NavigationPage.SetHasNavigationBar(this, false);
            InitializeComponent();
            BindingContext = viewModel = new MapViewModel();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            viewModel.RefreshMap();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            viewModel.CleanupLog();
        }
    }
}