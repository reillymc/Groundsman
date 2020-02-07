using Groundsman.Data;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Groundsman
{
    public partial class App : Application
    {
        public static FeatureStore FeatureStore { get; private set; }
        public static string AppTheme { get; set; }

        public App()
        {
            InitializeComponent();
            FeatureStore = new FeatureStore();
            MainPage = new NavigationPage(HomePage.Instance);
            // If the user ID hasn't been set yet, prompt the user to create one upon app launch.
            if (!Preferences.ContainsKey("UserID"))
            {
                MainPage.Navigation.PushModalAsync(new WelcomeFormView());
            }
        }

        protected override void OnStart()
        {
            // Handle when your app starts
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }
}
