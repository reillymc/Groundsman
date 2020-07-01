using Groundsman.Services;
using System;
using Xamarin.Forms;

namespace Groundsman
{
    public partial class AddFeatureView : ContentPage
    {
        NavigationService navigationService;
        bool modal;
        public AddFeatureView(bool isModal)
        {
            modal = isModal;
            InitializeComponent();
            TapGestureRecognizer tapEvent = new TapGestureRecognizer();
            navigationService = new NavigationService();

            var pointTapRecogniser = new TapGestureRecognizer();
            pointTapRecogniser.Tapped += async (sender, e) =>
            {
                navigationService.NavigateBack(modal);
                navigationService.NavigateToNewEditPage("Point");
            };
            pointFrame.GestureRecognizers.Add(pointTapRecogniser);

            var lineTapRecogniser = new TapGestureRecognizer();
            lineTapRecogniser.Tapped += async (sender, e) =>
            {
                navigationService.NavigateBack(modal);
                navigationService.NavigateToNewEditPage("LineString");
            };
            lineFrame.GestureRecognizers.Add(lineTapRecogniser);

            var polygonTapRecogniser = new TapGestureRecognizer();
            polygonTapRecogniser.Tapped += async (sender, e) =>
            {
                navigationService.NavigateBack(modal);
                navigationService.NavigateToNewEditPage("Polygon");
            };
            polygonFrame.GestureRecognizers.Add(polygonTapRecogniser);

            var importTapRecogniser = new TapGestureRecognizer();
            importTapRecogniser.Tapped += async (sender, e) =>
            {
                navigationService.NavigateBack(modal);
                await App.FeatureStore.ImportFeaturesFromFile();
            };
            importFrame.GestureRecognizers.Add(importTapRecogniser);

            var pasteTapRecogniser = new TapGestureRecognizer();
            pasteTapRecogniser.Tapped += async (sender, e) =>
            {
                navigationService.NavigateBack(modal);
                await App.FeatureStore.ImportFeaturesFromClipboard();
            };
            pasteFrame.GestureRecognizers.Add(pasteTapRecogniser);
        }

        void OnDismissButtonClicked(object sender, EventArgs args)
        {
            navigationService.NavigateBack(modal);
        }
    }
}
