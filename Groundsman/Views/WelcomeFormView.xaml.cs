using Groundsman.ViewModels;
using Xamarin.Forms;

namespace Groundsman.Views
{
    public partial class WelcomeFormView : ContentPage
    {
        private readonly WelcomeFormViewModel viewModel;

        public WelcomeFormView()
        {
            InitializeComponent();
            BindingContext = viewModel = new WelcomeFormViewModel();

            NavigationPage.SetHasBackButton(this, false);
        }

        //Stop the user from leaving the ID Entry page.
        protected override bool OnBackButtonPressed() => true;
    }
}