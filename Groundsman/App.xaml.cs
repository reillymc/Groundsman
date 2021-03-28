using Groundsman.Interfaces;
using Groundsman.Models;
using Groundsman.Services;
using Groundsman.Views;
using System;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Groundsman
{
    public partial class App : Application
    {
        public IDataService<Feature> FeatureStore => DependencyService.Get<IDataService<Feature>>();
        public INavigationService<Feature> NavigationService => DependencyService.Get<INavigationService<Feature>>();
        public enum Theme { Light, Dark }
        public static Theme AppTheme { get; set; }

        public static ObservableRangeCollection<Feature> featureList = new ObservableRangeCollection<Feature>();

        public App()
        {
            InitializeComponent();
            DependencyService.Register<FeatureService>();
            DependencyService.Register<NavigationService>();
            FeatureStore.ImportFeaturesAsync(AppConstants.GetFeaturesFile());
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

        public async void ImportFileAsync(string fileContent)
        {
            try
            {
                int successfulImports = await FeatureStore.ImportFeaturesAsync(fileContent);
                await NavigationService.ShowImportAlert(successfulImports);
            }
            catch (Exception ex)
            {
                await NavigationService.ShowAlert("Import Error", ex.Message, false);
            }
        }
    }
}
