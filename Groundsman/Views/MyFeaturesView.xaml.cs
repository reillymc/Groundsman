using Xamarin.Forms;
using Groundsman.ViewModels;

namespace Groundsman.Views
{
    public partial class MyFeaturesView : ContentPage
    {
        MyFeaturesViewModel viewModel;
        public MyFeaturesView()
        {
            InitializeComponent();
            BindingContext = viewModel = new MyFeaturesViewModel();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            viewModel.GetFeatures();
        }
    }
}