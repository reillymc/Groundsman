using System.Text;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Content.Res;
using Android.OS;
using Groundsman.Droid.Services;
using Groundsman.Misc;
using Groundsman.Styles;
using Plugin.CurrentActivity;
using Xamarin.Forms;

namespace Groundsman.Droid
{
    [Activity(Label = "Groundsman",
        //Icon = "@mipmap/ic_launcher",
        Theme = "@style/MainTheme",
        MainLauncher = true,
        ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation,
        LaunchMode = LaunchMode.SingleTask,
        ScreenOrientation = ScreenOrientation.Portrait)]
    [IntentFilter(new[] { Intent.ActionSend }, Categories = new[] { Intent.CategoryDefault }, DataMimeType = @"application/json")]
    public class MainActivity : Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        private Intent serviceIntent;
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

            App mainForms = new App();
            LoadApplication(mainForms);

            serviceIntent = new Intent(this, typeof(AndroidLocationService));
            SetServiceMethods();

            SetAppTheme();

            if (Intent.Action == Intent.ActionSend)
            {
                // Get the info from ClipData 
                ClipData.Item file = Intent.ClipData.GetItemAt(0);

                // Open a stream from the URI 
                System.IO.Stream fileStream = ContentResolver.OpenInputStream(file.Uri);

                // Save it over 
                System.IO.MemoryStream memOfFile = new System.IO.MemoryStream();

                fileStream.CopyTo(memOfFile);
                string decoded = Encoding.UTF8.GetString(memOfFile.ToArray());

                _ = mainForms.ImportRawGeoJSON(decoded);
            }
        }

        protected override void OnResume() => base.OnResume();

        /// <summary>
        /// Handle runtime permissions
        /// https://docs.microsoft.com/en-us/xamarin/essentials/get-started?context=xamarin%2Fxamarin-forms&tabs=windows%2Candroid
        /// </summary>
        /// <param name="requestCode"></param>
        /// <param name="permissions"></param>
        /// <param name="grantResults"></param>

        private void SetServiceMethods()
        {
            MessagingCenter.Subscribe<StartServiceMessage>(this, "ServiceStarted", message =>
            {
                serviceIntent.PutExtra("Interval", message.Interval);
                if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.O)
                {
                    StartForegroundService(serviceIntent);
                }
                else
                {
                    StartService(serviceIntent);
                }
            });

            MessagingCenter.Subscribe<StopServiceMessage>(this, "ServiceStopped", message =>
            {
                StopService(serviceIntent);
            });
        }

        private void SetAppTheme()
        {
            if (Resources.Configuration.UiMode.HasFlag(UiMode.NightYes))
                SetTheme(App.Theme.Dark);
            else
                SetTheme(App.Theme.Light);
        }

        private void SetTheme(App.Theme mode)
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

