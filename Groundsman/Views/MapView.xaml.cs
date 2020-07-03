using Xamarin.Forms;
using System.Threading;
using Groundsman.ViewModels;

namespace Groundsman
{
    public partial class MapView : ContentPage
    {
        private CancellationTokenSource cts;
        MapViewModel viewModel;
        public MapView()
        {
            NavigationPage.SetHasNavigationBar(this, false);
            InitializeComponent();
            BindingContext = viewModel = new MapViewModel();
        }


        protected override async void OnAppearing()
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