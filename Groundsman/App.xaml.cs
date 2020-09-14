using Groundsman.Interfaces;
using Groundsman.Models;
using Groundsman.Services;
using Groundsman.Views;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Groundsman
{
    public partial class App : Application
    {
        public IDataStore<Feature> FeatureStore => DependencyService.Get<IDataStore<Feature>>();
        public INavigationService<Feature> NavigationService => DependencyService.Get<INavigationService<Feature>>();
        public enum Theme { Light, Dark }
        public static Theme AppTheme { get; set; }

        public App()
        {
            Device.SetFlags(new[] { "SwipeView_Experimental" });
            InitializeComponent();
            DependencyService.Register<FeatureStore>();
            DependencyService.Register<NavigationService>();
            FeatureStore.GetItemsAsync(true);
            MainPage = new NavigationPage(HomePage.Instance);

            // If the user ID hasn't been set yet, prompt the user to create one upon app launch.
            if (Preferences.Get("UserID", "Groundsman") == "Groundsman")
            {
                _ = NavigationService.PushWelcomePage();
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
