﻿using Groundsman.Data;
using Groundsman.Interfaces;
using Groundsman.Services;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Groundsman
{
    public partial class App : Application
    {
        public IDataStore<Feature> FeatureStore => DependencyService.Get<IDataStore<Feature>>();
        public static LogStore LogStore { get; private set; }
        public enum Theme { Light, Dark }
        public static Theme AppTheme { get; set; }

        public App()
        {
            InitializeComponent();
            DependencyService.Register<FeatureStore>();
            FeatureStore.GetItemsAsync(true);
            LogStore = new LogStore();
            MainPage = new NavigationPage(HomePage.Instance);

            // If the user ID hasn't been set yet, prompt the user to create one upon app launch.
            if (Preferences.Get("UserID", "Groundsman") == "Groundsman")
            {
                HomePage.Instance.Navigation.PushModalAsync(new WelcomeFormView(true));
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
