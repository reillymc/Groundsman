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

        // Android button spam fix: force all opened pages to go back to main page.
        protected override bool OnBackButtonPressed()
        {
            HomePage.Instance.Navigation.PopToRootAsync();
            return true;
        }
    }
}