using Groundsman.Data;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Groundsman
{
    public partial class App : Application
    {
        public static FeatureStore FeatureStore { get; private set; }
        public static LogStore LogStore { get; private set; }
        public static Theme AppTheme { get; set; }
        public enum Theme
        {
            Light,
            Dark
        }

        public App()
        {
            InitializeComponent();
            FeatureStore = new FeatureStore();
            LogStore = new LogStore();
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
