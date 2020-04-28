using Xamarin.Forms;

namespace Groundsman
{
    public partial class MapView : ContentPage
    {
        public MapView()
        {
            InitializeComponent();
            NavigationPage.SetHasNavigationBar(this, false);
        }

        protected override void OnAppearing()
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