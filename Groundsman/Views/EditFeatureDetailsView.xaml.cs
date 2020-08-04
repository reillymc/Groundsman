using Xamarin.Forms;
using Groundsman.ViewModels;
using Groundsman.Models;

namespace Groundsman.Views
{
    public partial class EditFeatureDetailsView : ContentPage
    {
        /// <summary>
        /// Detail form constructor for when a new entry is being added.
        /// </summary>
        /// <param name="type">The geoJSON geometry type being added.</param>
        public EditFeatureDetailsView(FeatureType type)
        {
            InitializeComponent();
            BindingContext = new FeatureDetailsViewModel(type);
        }

        /// <summary>
        /// Detail form constructor for when an existing entry is being edited.
        /// </summary>
        /// <param name="data">The entry's data as represented by a feature object.</param>
        public EditFeatureDetailsView(Feature data)
        {
            InitializeComponent();
            BindingContext = new FeatureDetailsViewModel(data);
        }
    }
}
