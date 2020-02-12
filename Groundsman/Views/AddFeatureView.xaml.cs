using System;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Groundsman
{
    public partial class AddFeatureView : ContentPage
    {
        public AddFeatureView()
        {
            InitializeComponent();
        }

        async void OnDismissButtonClicked(object sender, EventArgs args)
        {
            await Navigation.PopModalAsync();
        }

        async void Point_Button_Clicked(object sender, EventArgs e)
        {
            await Navigation.PopModalAsync();
            HomePage.Instance.ShowNewDetailFormPage("Point");
        }

        async void Line_Button_Clicked(object sender, EventArgs e)
        {
            await Navigation.PopModalAsync();
            HomePage.Instance.ShowNewDetailFormPage("Line");
        }

        async void Polygon_Button_Clicked(object sender, EventArgs e)
        {
            await Navigation.PopModalAsync();
            HomePage.Instance.ShowNewDetailFormPage("Point");
        }

        async void Import_Button_Clicked(object sender, EventArgs e)
        {
            await Navigation.PopModalAsync();
            await App.FeatureStore.ImportFeaturesFromFile();
        }

        async void Paste_Button_Clicked(object sender, EventArgs e)
        {
            await Navigation.PopModalAsync();
            await App.FeatureStore.ImportFeaturesFromClipboard();
        }

        protected override async void OnDisappearing()
        {
            base.OnDisappearing();

            if (Navigation.ModalStack.Count > 0)
            {
                await Navigation.PopModalAsync();
            }
        }
    }
}
