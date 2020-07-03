using Xamarin.Forms;
using Groundsman.ViewModels;

namespace Groundsman
{
    public partial class MyFeaturesView : ContentPage
    {
        MyFeaturesViewModel viewModel;
        public MyFeaturesView()
        {
            InitializeComponent();
            BindingContext = viewModel = new MyFeaturesViewModel();
        }
    }
}