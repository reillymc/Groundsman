using Xamarin.Forms;
using Groundsman.ViewModels;

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

            // Set title to just 'Line' instead of 'LineString'
            if (type == "LineString")
            {
                Title = $"New Line";
            }
            else
            {
                Title = $"New {type}";
            }
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

        }
    }
}
