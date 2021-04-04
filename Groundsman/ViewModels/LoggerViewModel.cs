using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Groundsman.Models;
using Groundsman.Services;
using Newtonsoft.Json.Linq;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Groundsman.ViewModels
{
    public class LoggerViewModel : BaseViewModel
    {
        private CancellationTokenSource cts;

        public ICommand ToggleButtonClickCommand { set; get; }
        public ICommand ClearButtonClickCommand { set; get; }
        public ICommand ShareButtonClickCommand { set; get; }

        public ObservableCollection<DisplayPosition> LogPositions { get; set; }
        private List<string> DateTimeList = new List<string>();

        private bool isLogging;


        private bool _ScrollEnabled = true;
        public bool ScrollEnabled
        {
            get { return _ScrollEnabled; }
            set
            {
                _ScrollEnabled = value;
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
        private int _intervalEntry = 1;
        public int IntervalEntry
        {
            get { return _intervalEntry; }
            set
            {
                int temp = UnitEntry;
                UnitItems = value == 1 ? new List<string> { "Second", "Minute", "Hour" } : new List<string> { "Seconds", "Minutes", "Hours" };
                UnitEntry = temp;
                _intervalEntry = value;
                OnPropertyChanged();
            }
        }

        public LoggerViewModel()
        {
            Title = "Logger";
            LogPositions = new ObservableCollection<DisplayPosition>();

            try
            {
                Feature LogLine = Feature.ImportGeoJSON(AppConstants.GetLogFile());
                JArray dt = (JArray)LogLine.Properties["DateTimes"];
                string[] datetimes = dt.ToObject<string[]>();
                LineString line = (LineString)LogLine.Geometry;
                int index = 0;
                foreach (Position position in line.Coordinates)
                {
                    DateTimeList.Add(datetimes[index]);
                    LogPositions.Add(new DisplayPosition(datetimes[index], position));
                    index++;
                }
            }
            catch (Exception ex)
            { /*TODO*/
                NavigationService.ShowAlert("", ex.Message, false);
                
            }


            ToggleButtonClickCommand = new Command(() =>
            {
                if (isLogging)
                {
                    ToggleButtonLabel = "Start";
                    StopLogging();
                }
                else
                {
                    ToggleButtonLabel = "Stop";
                    if (IntervalEntry < 1)
                    {
                        IntervalEntry = 1;
                    }
                    switch (UnitEntry)
                    {
                        case 0:
                            StartLogging(IntervalEntry);
                            break;
                        case 1:
                            StartLogging(IntervalEntry * 60);
                            break;
                        case 2:
                            StartLogging(IntervalEntry * 3600);
                            break;
                    }
                }
                isLogging = !isLogging;
                ScrollEnabled = !isLogging;
            });

            ClearButtonClickCommand = new Command(() => { ClearLog(); });

            ShareButtonClickCommand = new Command(async () => { await ExportLogFile(); });
        }

        public void StartLogging(int Interval)
        {
            cts = new CancellationTokenSource();
            _ = UpdaterAsync(new TimeSpan(0, 0, Interval), cts.Token);
        }

        public void StopLogging()
        {
            cts.Cancel();
            cts.Dispose();
        }

        private async Task UpdaterAsync(TimeSpan interval, CancellationToken ct)
        {
            while (true)
            {
                await Task.Delay(interval, ct);
                Position location = await HelperServices.GetGeoLocation();
                if (location != null)
                {
                    DisplayPosition position = new DisplayPosition(DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"), location.Longitude.ToString(), location.Latitude.ToString(), location.Altitude.ToString());
                    LogPositions.Add(position);
                    DateTimeList.Add(position.Index);
                    SaveLog();
                }
            }
        }

        private void SaveLog()
        {
            if (LogPositions.Count < 2)
            {
                return;
            }
            List<Position> posList = new List<Position>();
            foreach (DisplayPosition displayPosition in LogPositions)
            {
                posList.Add(new Position(displayPosition));
            }

            Feature logLine = new Feature
            {
                Properties = new Dictionary<string, object>(),
                Geometry = new LineString(posList)
            };

            logLine.Properties.Add("DateTimes", DateTimeList.ToArray());
            File.WriteAllText(AppConstants.LOG_FILE, logLine.ExportGeoJSON());
        }

        private void ClearLog()
        {
            LogPositions.Clear();
            File.WriteAllText(AppConstants.LOG_FILE, "");
        }

        public async Task ExportLogFile()
        {
            string LogString = "Time, Longitude, Latitude, Altitude\n";

            foreach (DisplayPosition position in LogPositions)
            {
                LogString += $"{position}\n";
            }

            File.WriteAllText(AppConstants.EXPORT_LOG_FILE, LogString);
            await Share.RequestAsync(new ShareFileRequest
            {
                Title = "Groundsman Log",
                File = new ShareFile(AppConstants.EXPORT_LOG_FILE, "text/csv"),
                PresentationSourceBounds = DeviceInfo.Platform == DevicePlatform.iOS && DeviceInfo.Idiom == DeviceIdiom.Tablet
                    ? new System.Drawing.Rectangle((int)(DeviceDisplay.MainDisplayInfo.Width * .474), 80, 0, 0)
                    : System.Drawing.Rectangle.Empty
            });
        }
    }
}
