using Groundsman.Models;
using Groundsman.ViewModels;
using System;
using Xamarin.Forms;

namespace Groundsman.Views
{
    public partial class FeatureDetailsView : ContentPage
    {
        public FeatureDetailsView(Feature data)
        {
            InitializeComponent();
            BindingContext = new FeatureDetailsViewModel(data);

            Title = data.properties.name;
        }

        private void listView_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            geolocationListView.SelectedItem = null;
        }

        async void OnDismissButtonClicked(object sender, EventArgs args)
        {
            await Navigation.PopModalAsync();
        }
    }
}