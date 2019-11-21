using Xamarin.Forms;

namespace Groundsman
{
    public partial class WelcomeFormView : ContentPage
    {
        public WelcomeFormView()
        {
            InitializeComponent();
            NavigationPage.SetHasNavigationBar(this, false);
            NavigationPage.SetHasBackButton(this, false);
        }

        // Stop the user from leaving the ID Entry page.
        protected override bool OnBackButtonPressed()
        {
            return true;
        }
    }
}