using Xamarin.Forms;
using System.Threading;


namespace Groundsman
{
    public partial class MapView : ContentPage
    {
        private CancellationTokenSource cts;
        public MapView()
        {
            NavigationPage.SetHasNavigationBar(this, false);
            InitializeComponent();
        }


        protected override async void OnAppearing()
        {
            base.OnAppearing();
            ((MapViewModel)BindingContext).RefreshMap();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            ((MapViewModel)BindingContext).CleanupLog();
        }
    }
}