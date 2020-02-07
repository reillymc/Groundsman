using Plugin.Permissions;
using Plugin.Permissions.Abstractions;
using System;
using System.Windows.Input;
using Xamarin.Forms;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Plugin.Share;
using System.IO;

namespace Groundsman
{
    public class LoggerViewModel : ViewModelBase
    {
        private const string LOG_FILENAME = "log.csv";
        public ICommand ToggleButtonClickCommand { set; get; }
        public ICommand ClearButtonClickCommand { set; get; }
        public ICommand ExportButtonClickCommand { set; get; }
        private CancellationTokenSource cts;
        private double lat;
        private double lon;
        private double alt;
        public bool isLogging;

        private string _textEntry;
        public string TextEntry
        {
            get { return _textEntry; }
            set
            {
                _textEntry = value;
                OnPropertyChanged();
            }
        }

        private string _ToggleButtonLabel = "Start Logging";
        public string ToggleButtonLabel
        {
            get { return _ToggleButtonLabel; }
            set
            {
                _ToggleButtonLabel = value;
                OnPropertyChanged();
            }
        }

        private int _intervalEntry = 1;
        public int IntervalEntry
        {
            get { return _intervalEntry; }
            set
            {
                _intervalEntry = value;
                OnPropertyChanged();
            }
        }

        public LoggerViewModel()
        {
            ToggleButtonClickCommand = new Command(() =>
            {
                if (isLogging)
                {
                    StopUpdate();
                }
                else
                {
                    if (IntervalEntry < 1)
                    {
                        IntervalEntry = 1;
                    }
                    StartUpdate();
                }
            });

            ClearButtonClickCommand = new Command(() =>
            {
                TextEntry = "";
            });

            ExportButtonClickCommand = new Command(() =>
            {
                ExportLog();
            });
        }

        private void StartUpdate()
        {
            cts = new CancellationTokenSource();
            _ = UpdaterAsync(new TimeSpan(0, 0, IntervalEntry), cts.Token);
            isLogging = true;
            ToggleButtonLabel = "Stop Logging";
        }

        private void StopUpdate()
        {
            cts.Cancel();
            cts.Dispose();
            isLogging = false;
            ToggleButtonLabel = "Start Logging";
        }

        private async Task UpdaterAsync(TimeSpan interval, CancellationToken ct)
        {
            while (true)
            {
                await Task.Delay(interval, ct);
                _ = GetGeoLocation();
            }
        }

        private async void ExportLog()
        {
            if (!CrossShare.IsSupported)
                return;
            File.WriteAllText(Path.Combine(FileSystem.AppDataDirectory, LOG_FILENAME), TextEntry);
            ExperimentalFeatures.Enable("ShareFileRequest_Experimental");
            await Share.RequestAsync(new ShareFileRequest
            {
                Title = "Groundsman Logfile",
                File = new ShareFile(Path.Combine(FileSystem.AppDataDirectory, LOG_FILENAME), "text/csv")
            });
        }

        private async Task GetGeoLocation()
        {
            try
            {
                var status = await CrossPermissions.Current.CheckPermissionStatusAsync(Permission.Location);

                if (status == PermissionStatus.Granted)
                {
                    // Gets current location of device (MORE ACCURATE, but slower)
                    var request = new GeolocationRequest(GeolocationAccuracy.Medium);
                    var location = await Geolocation.GetLocationAsync(request);
                    if (location != null)
                    {
                        lat = location.Latitude;
                        lon = location.Longitude;
                        alt = location.Altitude ?? 0;
                    }
                    string newEntry = string.Format("{0}, {1}, {2}, {3}\n", DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"), lat, lon, alt);
                    TextEntry += newEntry;
                }
                else
                {
                    StopUpdate();
                    await HomePage.Instance.DisplayAlert("Permissions Error", "Location permissions for Groundsman must be enabled to utilise this feature.", "Ok");
                }
            }
            catch (Exception)
            {
                await HomePage.Instance.DisplayAlert("Geolocation Error", "Location services must be enabled to utilise this feature", "Ok");
                throw new Exception();
            }
        }
    }
}
