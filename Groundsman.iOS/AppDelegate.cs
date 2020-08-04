using Foundation;
using Groundsman.Models;
using System.IO;
using UIKit;

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

        App mainForms;
        public override bool FinishedLaunching(UIApplication uiApplication, NSDictionary launchOptions)
        {
            Xamarin.Forms.Forms.Init();
            Xamarin.FormsMaps.Init();

            mainForms = new App();

            // Get possible shortcut item
            if (launchOptions != null)
            {
                LaunchedShortcutItem = launchOptions[UIApplication.LaunchOptionsShortcutItemKey] as UIApplicationShortcutItem;
            }
            UIColor tintColor = UIColor.FromRGB(76, 175, 80);
            UINavigationBar.Appearance.TintColor = tintColor;
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
                    mainForms.FeatureStore.ImportFeaturesAsync(filecontent, true);
                }
            }
            return true;
        }

        public UIApplicationShortcutItem LaunchedShortcutItem { get; set; }

        public bool HandleShortcutItem(UIApplicationShortcutItem shortcutItem)
        {
            var handled = false;

            // Anything to process?
            if (shortcutItem == null) return false;
            // Take action based on the shortcut type
            switch (shortcutItem.Type)
            {
                case ShortcutIdentifier.First:
                    _ = mainForms.navigationService.NavigateToNewEditPage(FeatureType.Point);
                    handled = true;
                    break;
                case ShortcutIdentifier.Second:
                    _ = mainForms.navigationService.NavigateToNewEditPage(FeatureType.LineString);
                    handled = true;
                    break;
                case ShortcutIdentifier.Third:
                    _ = mainForms.navigationService.NavigateToNewEditPage(FeatureType.Polygon);
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

        public override void PerformActionForShortcutItem(UIApplication application, UIApplicationShortcutItem shortcutItem, UIOperationHandler completionHandler)
        {
            // Perform action
            completionHandler(HandleShortcutItem(shortcutItem));
        }
    }
}