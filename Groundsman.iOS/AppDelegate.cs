using System;
using System.IO;
using CoreLocation;
using Foundation;
using Groundsman.iOS.Services;
using Groundsman.Misc;
using UIKit;
using Xamarin.Forms;

namespace Groundsman.iOS
{
    // The UIApplicationDelegate for the application. This class is responsible for launching the 
    // User Interface of the application, as well as listening (and optionally responding) to 
    // application events from iOS.
    [Register("AppDelegate")]
    public class AppDelegate : global::Xamarin.Forms.Platform.iOS.FormsApplicationDelegate
    {
        //
        // This method is invoked when the application has loaded and is ready to run. In this 
        // method you should instantiate the window, load the UI into it and then make the window
        // visible.
        //
        // You have 17 seconds to return from this method, or iOS will terminate your application.
        //
        private App mainForms;
        private LocationService locationService;
        private readonly CLLocationManager locMgr = new CLLocationManager();
        public override bool FinishedLaunching(UIApplication uiApplication, NSDictionary launchOptions)
        {
            Xamarin.Forms.Forms.Init();
            Xamarin.FormsMaps.Init();

            mainForms = new App();

            locationService = new LocationService();
            UIApplication.SharedApplication.SetMinimumBackgroundFetchInterval(UIApplication.BackgroundFetchIntervalMinimum);

            //Background Location Permissions
            locMgr.RequestAlwaysAuthorization();
            locMgr.PausesLocationUpdatesAutomatically = false;
            locMgr.AllowsBackgroundLocationUpdates = true;
            SetServiceMethods();

            // Get possible shortcut item
            if (launchOptions != null)
            {
                LaunchedShortcutItem = launchOptions[UIApplication.LaunchOptionsShortcutItemKey] as UIApplicationShortcutItem;
            }

            // Set app-wide tint colour and visual effects
            UIColor GroundsmanGreen = new UIColor(red: 0.30f, green: 0.75f, blue: 0.30f, alpha: 1.00f);
            UIView.Appearance.TintColor = GroundsmanGreen;
            UINavigationBar.Appearance.Translucent = true;

            LoadApplication(mainForms);

            return base.FinishedLaunching(uiApplication, launchOptions);
        }

        public override bool OpenUrl(UIApplication app, NSUrl url, NSDictionary options)
        {
            {
                using (StreamReader reader = new StreamReader(url.Path))
                {
                    string filecontent = reader.ReadToEnd();
                    _ = mainForms.ImportRawGeoJSON(filecontent);
                }
            }
            return true;
        }

        public UIApplicationShortcutItem LaunchedShortcutItem { get; set; }

        public bool HandleShortcutItem(UIApplicationShortcutItem shortcutItem)
        {
            bool handled = false;

            // Anything to process?
            if (shortcutItem == null) return false;
            // Take action based on the shortcut type
            switch (shortcutItem.Type)
            {
                case "com.geoapplads.Groundsman.000":
                    _ = mainForms.NavigationService.PushAddFeaturePage();
                    handled = true;
                    break;
            }
            // Return results
            return handled;
        }

        public override void OnActivated(UIApplication uiApplication)
        {
            // Handle any shortcut item being selected
            HandleShortcutItem(LaunchedShortcutItem);

            // Clear shortcut after it's been handled
            LaunchedShortcutItem = null;
        }

        public override void PerformActionForShortcutItem(UIApplication application, UIApplicationShortcutItem shortcutItem, UIOperationHandler completionHandler) => completionHandler(HandleShortcutItem(shortcutItem));

        private void SetServiceMethods()
        {
            MessagingCenter.Subscribe<StartServiceMessage>(this, "ServiceStarted", async message =>
            {
                await locationService.Start(message.Interval);
            });

            MessagingCenter.Subscribe<StopServiceMessage>(this, "ServiceStopped", message =>
            {
                locationService.Stop();
            });
        }

        public override void PerformFetch(UIApplication application, Action<UIBackgroundFetchResult> completionHandler)
        {
            try
            {
                completionHandler(UIBackgroundFetchResult.NewData);
            }
            catch (Exception)
            {
                completionHandler(UIBackgroundFetchResult.NoData);
            }
        }
    }
}