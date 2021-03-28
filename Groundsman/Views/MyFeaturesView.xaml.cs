using Xamarin.Forms;
using Groundsman.ViewModels;

namespace Groundsman.Views
{
    public partial class MyFeaturesView : ContentPage
    {
        MyFeaturesViewModel viewModel;
        ViewCell lastCell;

        public MyFeaturesView()
        {
            InitializeComponent();
            BindingContext = viewModel = new MyFeaturesViewModel();
        }

        private void ViewCell_Tapped(object sender, System.EventArgs e)
        {
            if (Device.RuntimePlatform == Device.Android)
            {
                if (lastCell != null)
                    lastCell.View.BackgroundColor = Color.Default;
                ViewCell viewCell = (ViewCell)sender;
                if (viewCell.View != null)
                {
                    viewCell.View.BackgroundColor = App.AppTheme == App.Theme.Light ? Color.White : Color.FromHex("#111111");
                    lastCell = viewCell;
                }
            }
        }
    }
}
