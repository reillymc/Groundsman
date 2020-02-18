using System;
using Xamarin.Forms;

namespace Groundsman
{
    public partial class FeatureDetailsView : ContentPage
    {
        public FeatureDetailsView(Feature data)
        {
            InitializeComponent();
            BindingContext = new FeatureDetailsViewModel(data);

            Title = data.Properties.Name;
        }

        private void listView_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            geolocationListView.SelectedItem = null;
        }

        async void OnDismissButtonClicked(object sender, EventArgs args)
        {
            await Navigation.PopModalAsync();
        }

        // Android button spam fix: force all opened pages to go back to main page.
        protected override bool OnBackButtonPressed()
        {
            base.OnBackButtonPressed();
            HomePage.Instance.Navigation.PopToRootAsync();
            return true;
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