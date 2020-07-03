using Foundation;
using Groundsman.Services;
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

        NavigationService navigationService;

        public override bool FinishedLaunching(UIApplication uiApplication, NSDictionary launchOptions)
        {
            Xamarin.Forms.Forms.Init();
            Xamarin.FormsMaps.Init();

            // Get possible shortcut item
            if (launchOptions != null)
            {
                LaunchedShortcutItem = launchOptions[UIApplication.LaunchOptionsShortcutItemKey] as UIApplicationShortcutItem;
            }
            UIColor tintColor = UIColor.FromRGB(76, 175, 80);
            UINavigationBar.Appearance.TintColor = tintColor;
            UINavigationBar.Appearance.Translucent = true;

            LoadApplication(new App());

            return base.FinishedLaunching(uiApplication, launchOptions);
        }

        public override bool OpenUrl(UIApplication app, NSUrl url, NSDictionary options)
        {
            {
                url.StartAccessingSecurityScopedResource();
                //_ = App.FeatureStore.ImportFeaturesFromFileURL(url.StandardizedUrl.Path, url.LastPathComponent);
            }
            return true;
        }

        public UIApplicationShortcutItem LaunchedShortcutItem { get; set; }

        public bool HandleShortcutItem(UIApplicationShortcutItem shortcutItem)
        {
            var handled = false;

            // Anything to process?
            if (shortcutItem == null) return false;
            navigationService = new NavigationService();
            // Take action based on the shortcut type
            switch (shortcutItem.Type)
            {
                case ShortcutIdentifier.First:
                    navigationService.NavigateToNewEditPage("Point");
                    handled = true;
                    break;
                case ShortcutIdentifier.Second:
                    navigationService.NavigateToNewEditPage("LineString");
                    handled = true;
                    break;
                case ShortcutIdentifier.Third:
                    navigationService.NavigateToNewEditPage("Polygon");
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