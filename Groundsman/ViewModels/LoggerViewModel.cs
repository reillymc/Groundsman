using System;
using System.Windows.Input;
using Xamarin.Forms;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using Groundsman.Services;
using Point = Groundsman.Models.Point;
using System.Collections.Generic;

namespace Groundsman.ViewModels
{
    public class LoggerViewModel : BaseViewModel
    {
        public ICommand ToggleButtonClickCommand { set; get; }
        public ICommand ClearButtonClickCommand { set; get; }
        public ICommand ShareButtonClickCommand { set; get; }
        private CancellationTokenSource cts;
        public bool isLogging;

        private readonly string CSVHeader = "Time, Latitude, Longitude, Altitude\n";
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

        private List<string> _UnitItems = new List<string> { "Seconds", "Minutes", "Hours" };
        public List<string> UnitItems
        {
            get { return _UnitItems; }
            set
            {
                _UnitItems = value;
                OnPropertyChanged();
            }
        }

        private int _UnitEntry = 0;
        public int UnitEntry
        {
            get { return _UnitEntry; }
            set
            {
                _UnitEntry = value;
                OnPropertyChanged();
            }
        }

        private string _ToggleButtonLabel = "Start";
        public string ToggleButtonLabel
        {
            get { return _ToggleButtonLabel; }
            set
            {
                _ToggleButtonLabel = value;
                OnPropertyChanged();
            }
        }

        private int logInterval = 1;

        private int _intervalEntry = 1;
        public int IntervalEntry
        {
            get { return _intervalEntry; }
            set
            {
                int temp = UnitEntry;
                if (value == 1)
                {
                    UnitItems = new List<string> { "Second", "Minute", "Hour" };
                } else
                {
                    UnitItems = new List<string> { "Seconds", "Minutes", "Hours" };
                }
                UnitEntry = temp;
                _intervalEntry = value;
                OnPropertyChanged();
            }
        }

        public LoggerViewModel()
        {
            Title = "Logger";
            TextEntry = File.ReadAllText(AppConstants.LOG_FILE);
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
                    switch (UnitEntry)
                    {
                        case 0:
                            logInterval = IntervalEntry;
                            break;
                        case 1:
                            logInterval = IntervalEntry * 60;
                            break;
                        case 2:
                            logInterval = IntervalEntry * 3600;
                            break;
                    }
                    StartUpdate();
                }
            });

            ClearButtonClickCommand = new Command(() =>
            {
                TextEntry = CSVHeader;
                File.WriteAllText(AppConstants.LOG_FILE, TextEntry);
            });

            ShareButtonClickCommand = new Command(async () =>
            {
                File.WriteAllText(AppConstants.LOG_FILE, TextEntry);
                await App.LogStore.ExportLogFile();
            });
        }

        private void StartUpdate()
        {
            cts = new CancellationTokenSource();
            _ = UpdaterAsync(new TimeSpan(0, 0, logInterval), cts.Token);
            isLogging = true;
            ToggleButtonLabel = "Stop";
        }

        private void StopUpdate()
        {
            cts.Cancel();
            cts.Dispose();
            isLogging = false;
            ToggleButtonLabel = "Start";
        }

        private async Task UpdaterAsync(TimeSpan interval, CancellationToken ct)
        {
            while (true)
            {
                await Task.Delay(interval, ct);
                Point location = await HelperServices.GetGeoLocation();
                if (location != null)
                {
                    string newEntry = string.Format("{0}, {1}, {2}, {3}\n", DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"), location.Latitude, location.Longitude, location.Altitude);
                    TextEntry += newEntry;
                    File.WriteAllText(AppConstants.LOG_FILE, TextEntry);
                }
                else
                {
                    StopUpdate();
                }
            }
        }
    }
}
