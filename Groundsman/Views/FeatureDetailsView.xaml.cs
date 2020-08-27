using Groundsman.Models;
using Groundsman.ViewModels;
using Xamarin.Forms;

namespace Groundsman.Views
{
    public partial class FeatureDetailsView : ContentPage
    {
        public FeatureDetailsView(Feature data)
        {
            InitializeComponent();
            BindingContext = new FeatureDetailsViewModel(data);
        }
    }
}