using Groundsman.ViewModels;
using Xamarin.Forms;

namespace Groundsman.Views
{
    public partial class WelcomeView : ContentPage
    {
        private readonly WelcomeViewModel viewModel;

        public WelcomeView()
        {
            InitializeComponent();
            BindingContext = viewModel = new WelcomeViewModel();

            NavigationPage.SetHasBackButton(this, false);
        }

        //Stop the user from leaving the ID Entry page.
        protected override bool OnBackButtonPressed() => true;
    }
}