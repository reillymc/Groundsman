using Xamarin.Forms;

namespace Groundsman
{
    public partial class EditFeatureDetailsView : ContentPage
    {
        /// <summary>
        /// Detail form constructor for when a new entry is being added.
        /// </summary>
        /// <param name="type">The geoJSON geometry type being added.</param>
        public EditFeatureDetailsView(string type)
        {
            InitializeComponent();
            BindingContext = new FeatureDetailsViewModel(type);

            Title = $"New {type}";

            DetermineAddPointBtnVisability(type);
        }

        /// <summary>
        /// Detail form constructor for when an existing entry is being edited.
        /// </summary>
        /// <param name="data">The entry's data as represented by a feature object.</param>
        public EditFeatureDetailsView(Feature data)
        {
            InitializeComponent();
            BindingContext = new FeatureDetailsViewModel(data);

            Title = data.properties.name;

            DetermineAddPointBtnVisability(data.geometry.type);
        }

        /// <summary>
        /// Determines whether or not the "add to {type}" button is visible.
        /// </summary>
        /// <param name="type">The type of the entry.</param>
        private void DetermineAddPointBtnVisability(string type)
        {
            if (type == "Polygon")
            {
                addPointBtn.ImageSource = "add_icon_color";
                addPointBtn.IsVisible = true;
                closePolyBtn.IsVisible = true;
            }
            else if (type == "Line" || type == "LineString")
            {
                addPointBtn.ImageSource = "add_icon_color";
                addPointBtn.IsVisible = true;
                closePolyBtn.IsVisible = false;
            } else
            {
                addPointBtn.IsVisible = false;
                closePolyBtn.IsVisible = false;
            }
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
