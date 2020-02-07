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
                Point location = await Services.GeolocationService.GetGeoLocation();
                if (location != null)
                {
                    string newEntry = string.Format("{0}, {1}, {2}, {3}\n", DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"), location.Latitude, location.Longitude, location.Altitude);
                    TextEntry += newEntry;
                }
                else
                {
                    StopUpdate();
                }
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
    }
}
