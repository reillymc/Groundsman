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
            TapGestureRecognizer tapEvent = new TapGestureRecognizer();

            var pointTapRecogniser = new TapGestureRecognizer();
            pointTapRecogniser.Tapped += async (sender, e) =>
            {
                await Navigation.PopModalAsync();
                HomePage.Instance.ShowNewDetailFormPage("Point");
            };
            pointFrame.GestureRecognizers.Add(pointTapRecogniser);

            var lineTapRecogniser = new TapGestureRecognizer();
            lineTapRecogniser.Tapped += async (sender, e) =>
            {
                await Navigation.PopModalAsync();
                HomePage.Instance.ShowNewDetailFormPage("Line");
            };
            lineFrame.GestureRecognizers.Add(lineTapRecogniser);

            var polygonTapRecogniser = new TapGestureRecognizer();
            polygonTapRecogniser.Tapped += async (sender, e) =>
            {
                await Navigation.PopModalAsync();
                HomePage.Instance.ShowNewDetailFormPage("Polygon");
            };
            polygonFrame.GestureRecognizers.Add(polygonTapRecogniser);

            var importTapRecogniser = new TapGestureRecognizer();
            importTapRecogniser.Tapped += async (sender, e) =>
            {
                await Navigation.PopModalAsync();
                await App.FeatureStore.ImportFeaturesFromFile();
            };
            importFrame.GestureRecognizers.Add(importTapRecogniser);

            var pasteTapRecogniser = new TapGestureRecognizer();
            pasteTapRecogniser.Tapped += async (sender, e) =>
            {
                await Navigation.PopModalAsync();
                await App.FeatureStore.ImportFeaturesFromClipboard();
            };
            pasteFrame.GestureRecognizers.Add(pasteTapRecogniser);
        }

        async void OnDismissButtonClicked(object sender, EventArgs args)
        {
            await Navigation.PopModalAsync();
        }
    }
}
