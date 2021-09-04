using System;
using System.IO;
using System.Threading.Tasks;
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
        private IDataService<Feature> FeatureStore => DependencyService.Get<IDataService<Feature>>();
        public INavigationService<Feature> NavigationService => DependencyService.Get<INavigationService<Feature>>();
        public enum Theme { Light, Dark }
        public static Theme AppTheme { get; set; }

        public static ShakeService shakeService;

        private static Database database;
        public static Database Database
        {
            get
            {
                if (database == null)
                {
                    database = new Database(Path.Combine(FileSystem.AppDataDirectory, "groundsman.db"));
                }
                return database;
            }
        }

        public App()
        {
            InitializeComponent();
            shakeService = new ShakeService(this);
            DependencyService.Register<FeatureService>();
            DependencyService.Register<NavigationService>();
            MainPage = new NavigationPage(HomePage.Instance);

            // If the user ID hasn't been set yet, prompt the user to create one upon app launch.
            if (Constants.FirstRun)
            {
                _ = NavigationService.PushWelcomePage();
            }

            _ = ImportLegacyFeatureList();
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

        public async Task ImportLegacyFeatureList()
        {
            var legacyFeatureList = Constants.FeaturesFileContents;
            if (legacyFeatureList == null) return;
            try
            {
                _ = await FeatureStore.ImportRawContents(legacyFeatureList);
                _ = await FeatureStore.GetItemsAsync();
                File.Delete(Constants.FEATURES_FILE);
            }
            catch (Exception ex)
            {
                bool result = await NavigationService.ShowAlert("Feature List Error", "Groundsman was unable to load your saved features. Would you like to export the corrupted features?", true);
                if (result)
                {
                    await Share.RequestAsync(new ShareFileRequest
                    {
                        Title = "Groundsman Feature List (Corrupted)",
                        File = new ShareFile(Constants.FEATURES_FILE, "application/json"),
                    });
                }
            }

        }

        public async Task ImportRawGeoJSON(string contents)
        {
            try
            {
                int successfulImports = await FeatureStore.ImportRawContents(contents);
                _ = await FeatureStore.GetItemsAsync();
                await NavigationService.ShowImportAlert(successfulImports);
            }
            catch (Exception ex)
            {
                await NavigationService.ShowAlert("Import Error", ex.Message, false);
            }
        }

        public async void UndoDelete()
        {
            bool result = await NavigationService.GetCurrentPage().DisplayAlert("Undo Delete", "", "Undo", "Cancel");
            if (result)
            {
                string contents = File.ReadAllText(Constants.DELETED_FEATURE_FILE);
                try
                {
                    _ = await FeatureStore.ImportRawContents(contents);
                    _ = await FeatureStore.GetItemsAsync();
                }
                catch (Exception ex)
                {
                    await NavigationService.ShowAlert("Recovery Error", ex.Message, false);
                }
            }
        }
    }
}
