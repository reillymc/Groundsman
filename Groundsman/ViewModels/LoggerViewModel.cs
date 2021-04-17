using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Groundsman.Misc;
using Groundsman.Models;
using Groundsman.Services;
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
        public ICommand OnCancelTappedCommand { get; set; }
        public ICommand OnDoneTappedCommand { get; set; }

        public Feature LogFeature;
        public ObservableCollection<DisplayPosition> LogPositions { get; set; }
        private List<string> DateTimeList = new List<string>();

        public bool isLogging;


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

        private List<string> _UnitItems = new List<string>() { "Seconds", "Minutes", "Hours" };
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

        public string NameEntry { get; set; }

        public LoggerViewModel()
        {
            Title = "New Log Line";
            LogPositions = new ObservableCollection<DisplayPosition>();

            LogFeature = new Feature
            {
                Properties = new Dictionary<string, object>
                {
                    [Constants.IdentifierProperty] = Constants.NewFeatureID
                }
            };
            InitCommands();
        }

        private void InitCommands()
        {
            ToggleButtonClickCommand = new Command(() => { ToggleLogging(); });
            ClearButtonClickCommand = new Command(() => { LogPositions.Clear(); });
            ShareButtonClickCommand = new Command<View>(async (view) => { await ShareLog(view); });
            OnCancelTappedCommand = new Command(async () => await OnDismiss(true));
            OnDoneTappedCommand = new Command(async () => await OnSaveUpdateActivated());
        }

        public void ToggleLogging()
        {
            if (isLogging)
            {
                ToggleButtonLabel = "Start";
                cts.Cancel();
                cts.Dispose();
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
        }

        public LoggerViewModel(Feature Log)
        {
            Title = NameEntry = (string)Log.Properties[Constants.NameProperty];

            LogFeature = Log;
            LogPositions = new ObservableCollection<DisplayPosition>();
            object test = Log.Properties[Constants.LogDateTimeListProperty];
            string[] datetimes = ((IEnumerable)test).Cast<object>()
                             .Select(x => x.ToString())
                             .ToArray();
            //string[] datetimes = (string[])Log.Properties["DateTimes"];
            LineString line = (LineString)Log.Geometry;
            int index = 0;
            foreach (Position position in line.Coordinates)
            {
                DateTimeList.Add(datetimes[index]);
                LogPositions.Add(new DisplayPosition(datetimes[index], position));
                index++;
            }
            InitCommands();
        }


        private async Task OnSaveUpdateActivated()
        {
            try
            {
                if (SaveLog())
                {
                    await NavigationService.NavigateBack(true);
                }
            }
            catch (Exception e)
            {
                await Application.Current.MainPage.DisplayAlert("Unable To Save Log", $"{e.Message}.", "Ok");
            }
        }

        public void StartLogging(int Interval)
        {
            cts = new CancellationTokenSource();
            _ = UpdaterAsync(new TimeSpan(0, 0, Interval), cts.Token);
        }

        private async Task UpdaterAsync(TimeSpan interval, CancellationToken ct)
        {
            while (true)
            {
                await Task.Delay(interval, ct);
                try
                {
                    Position location = await HelperServices.GetGeoLocation();
                    DisplayPosition position = new DisplayPosition(DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"), location.Longitude.ToString(), location.Latitude.ToString(), location.Altitude.ToString());
                    LogPositions.Add(position);
                    DateTimeList.Add(position.Index);
                }
                catch
                {
                    await Application.Current.MainPage.DisplayAlert("Unable To Fetch Location", "Ensure Groundsman has access to your device's location.", "Ok");
                    ToggleLogging();
                }
                try
                {
                    SaveLog();
                }
                catch { } // Ignore if cant auto save
            }
        }

        private bool SaveLog()
        {
            List<Position> posList = new List<Position>();
            foreach (DisplayPosition displayPosition in LogPositions)
            {
                posList.Add(new Position(displayPosition));
            }

            LogFeature.Geometry = new LineString(posList);
            LogFeature.Properties[Constants.LogDateTimeListProperty] = DateTimeList.ToArray();
            LogFeature.Properties[Constants.NameProperty] = !string.IsNullOrEmpty(NameEntry) ? NameEntry : "Log LineString";
            LogFeature.Properties[Constants.DateProperty] = DateTime.Now.ToShortDateString();
            LogFeature.Properties[Constants.AuthorProperty] = Preferences.Get(Constants.UserIDKey, "Groundsman");
            return (string)LogFeature.Properties[Constants.IdentifierProperty] == Constants.NewFeatureID ? FeatureStore.AddItem(LogFeature) : FeatureStore.UpdateItem(LogFeature);
        }

        public async Task ShareLog(View element)
        {
            if (IsBusy) return;

            IsBusy = true;

            var bounds = element.GetAbsoluteBounds().ToSystemRectangle();

            if (Preferences.Get(Constants.ShareLogAsGeoJSONKey, false))
            {
                ShareFileRequest share = FeatureStore.ExportFeatures(new List<Feature>() { LogFeature });
                share.PresentationSourceBounds = DeviceInfo.Platform == DevicePlatform.iOS && DeviceInfo.Idiom == DeviceIdiom.Tablet ? bounds : System.Drawing.Rectangle.Empty;
                await Share.RequestAsync(share);
            }
            else
            {
                string LogString = "Time, Longitude, Latitude, Altitude\n";
                foreach (DisplayPosition position in LogPositions)
                {
                    LogString += $"{position}\n";
                }

                File.WriteAllText(Constants.EXPORT_LOG_FILE, LogString);
                await Share.RequestAsync(new ShareFileRequest
                {
                    Title = "Groundsman Log",
                    File = new ShareFile(Constants.EXPORT_LOG_FILE, "text/csv"),
                    PresentationSourceBounds = DeviceInfo.Platform == DevicePlatform.iOS && DeviceInfo.Idiom == DeviceIdiom.Tablet ? bounds : System.Drawing.Rectangle.Empty
                });
            }

            IsBusy = false;
        }
    }
}
