using Xamarin.Forms;

namespace Groundsman
{
    public partial class MyFeaturesView : ContentPage
    {

        public MyFeaturesView()
        {
            InitializeComponent();
        }

        protected override void OnAppearing() // make data refresh on android - maybe with willappear
        {
            base.OnAppearing();

            if (((MyFeaturesViewModel)BindingContext).RefreshListCommand.CanExecute(null))
            {
                ((MyFeaturesViewModel)BindingContext).RefreshListCommand.Execute(null);
            }
        }

        protected override void OnDisappearing()
        {
            //loadingList.IsRunning = false;
            //loadingList.IsVisible = false;
            base.OnDisappearing();
        }
    }
}