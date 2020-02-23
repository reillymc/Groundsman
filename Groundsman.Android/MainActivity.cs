using Android.App;
using Android.Content.PM;
using Android.Views;
using Android.OS;
using Plugin.CurrentActivity;
using Android.Content.Res;
using Groundsman.Styles;

namespace Groundsman.Droid
{
    [Activity(Label = "Groundsman",
        //Icon = "@mipmap/ic_launcher",
        Theme = "@style/MainTheme",
        MainLauncher = true,
        ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation,
        LaunchMode = LaunchMode.SingleTask,
        ScreenOrientation = ScreenOrientation.Portrait)]
    public class MainActivity : Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {

        protected override void OnCreate(Bundle savedInstanceState)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(savedInstanceState);

            Xamarin.Forms.Forms.Init(this, savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);

            CrossCurrentActivity.Current.Init(this, savedInstanceState);
            Xamarin.FormsMaps.Init(this, savedInstanceState);

            Xamarin.Forms.Forms.ViewInitialized += (object sender, Xamarin.Forms.ViewInitializedEventArgs e) =>
            {
                if (!string.IsNullOrWhiteSpace(e.View.AutomationId))
                {
                    e.NativeView.ContentDescription = e.View.AutomationId;
                }
            };

            LoadApplication(new App());
            SetAppTheme();
        }

        protected override void OnResume()
        {
            base.OnResume();
        }

        /// <summary>
        /// Handle runtime permissions
        /// https://docs.microsoft.com/en-us/xamarin/essentials/get-started?context=xamarin%2Fxamarin-forms&tabs=windows%2Candroid
        /// </summary>
        /// <param name="requestCode"></param>
        /// <param name="permissions"></param>
        /// <param name="grantResults"></param>
        
        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            // Check if the selected toolbar button's id equals the back button id.
            if (item.ItemId == Android.Resource.Id.Home)
            {
                // If so, override it so it always takes the user straight back to the main page.
                HomePage.Instance.Navigation.PopToRootAsync();
                return false;
            }
            return base.OnOptionsItemSelected(item);
        }

        void SetAppTheme()
        {
            if (Resources.Configuration.UiMode.HasFlag(UiMode.NightYes))
                SetTheme(App.Theme.Dark);
            else
                SetTheme(App.Theme.Light);
        }

        void SetTheme(App.Theme mode)
        {
            if (mode == App.Theme.Dark)
            {
                if (App.AppTheme == App.Theme.Dark)
                    return;
                App.Current.Resources = new DarkTheme();
            }
            else
            {
                if (App.AppTheme != App.Theme.Dark)
                    return;
                App.Current.Resources = new LightTheme();
            }
            App.AppTheme = mode;
        }
    }
}

