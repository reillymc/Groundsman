using Groundsman.ViewModels;
using Xamarin.Forms;

namespace Groundsman
{
    public partial class WelcomeFormView : ContentPage
    {
        WelcomeFormViewModel viewModel;

        public WelcomeFormView(bool modal)
        {
            InitializeComponent();
            BindingContext = viewModel = new WelcomeFormViewModel(modal);

            NavigationPage.SetHasBackButton(this, false);
        }

        //Stop the user from leaving the ID Entry page.
        protected override bool OnBackButtonPressed()
        {
            return true;
        }
    }
}