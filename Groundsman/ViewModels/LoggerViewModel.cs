using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Groundsman.Misc;
using Groundsman.Models;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Groundsman.ViewModels
{
    public class LoggerViewModel : BaseViewModel
    {
        public ICommand ToggleButtonClickCommand { set; get; }
        public ICommand ClearButtonClickCommand { set; get; }
        public ICommand ShareButtonClickCommand { set; get; }
        public ICommand OnCancelTappedCommand { get; set; }
        public ICommand OnDoneTappedCommand { get; set; }

        public Feature LogFeature;
        public Feature OldLogFeature;
        public ObservableCollection<DisplayPosition> LogPositions { get; set; }
        private readonly List<string> DateTimeList = new List<string>();

        public bool isLogging;

        private bool _ScrollEnabled = true;
        public bool ScrollEnabled
        {
            get => _ScrollEnabled;
            set
            {
                _ScrollEnabled = value;
                OnPropertyChanged();
            }
        }

        private List<string> _UnitItems = new List<string>() { "Seconds", "Minutes", "Hours" };
        public List<string> UnitItems
        {
            get => _UnitItems;
            set
            {
                _UnitItems = value;
                OnPropertyChanged();
            }
        }

        private int _UnitEntry = 0;
        public int UnitEntry
        {
            get => _UnitEntry;
            set
            {
                _UnitEntry = value;
                OnPropertyChanged();
            }
        }

        private string _ToggleButtonLabel = "Start";
        public string ToggleButtonLabel
        {
            get => _ToggleButtonLabel;
            set
            {
                _ToggleButtonLabel = value;
                OnPropertyChanged();
            }
        }
        private int _intervalEntry = 1;
        public int IntervalEntry
        {
            get => _intervalEntry;
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
            HandleMessages();
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

            OldLogFeature = new Feature(Log.Geometry, Log.Properties);
            InitCommands();
            HandleMessages();
        }

        private void InitCommands()
        {
            ToggleButtonClickCommand = new Command(() => { ToggleLogging(); });
            ClearButtonClickCommand = new Command(() => { LogPositions.Clear(); });
            ShareButtonClickCommand = new Command<View>(async (view) => { await ShareLog(view); });
            OnCancelTappedCommand = new Command(async () => await OnCancelActivated());
            OnDoneTappedCommand = new Command(async () => await OnSaveUpdateActivated());
        }

        private async Task OnCancelActivated()
        {
            await SaveLog(true);
            await OnDismiss(true);
        }

        public void ToggleLogging()
        {
            if (isLogging)
            {
                ToggleButtonLabel = "Start";
                StopServiceMessage message = new StopServiceMessage();
                MessagingCenter.Send(message, "ServiceStopped");
            }
            else
            {
                if (IntervalEntry < 1)
                {
                    IntervalEntry = 1;
                }
                StartServiceMessage message = new StartServiceMessage
                {
                    Interval = UnitEntry switch
                    {
                        1 => IntervalEntry * 1000 * 60, // Minutes
                        2 => IntervalEntry * 1000 * 3600, // Hours
                        _ => IntervalEntry * 1000, // Seconds
                    }
                };
                ToggleButtonLabel = "Stop";
                MessagingCenter.Send(message, "ServiceStarted");
            }
            isLogging = !isLogging;
            ScrollEnabled = !isLogging;
        }

        private async Task OnSaveUpdateActivated()
        {
            try
            {
                if (await SaveLog())
                {
                    await NavigationService.NavigateBack(true);
                }
            }
            catch (Exception e)
            {
                await Application.Current.MainPage.DisplayAlert("Unable To Save Log", $"{e.Message}.", "Ok");
            }
        }

        private async Task<bool> SaveLog(bool reset = false)
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
            if (reset)
            {
                if ((string)LogFeature.Properties[Constants.IdentifierProperty] == Constants.NewFeatureID)
                {
                    await FeatureStore.AddItem(LogFeature);
                }
                else
                {
                    await FeatureStore.UpdateItem(OldLogFeature);
                }
            }
            return (string)LogFeature.Properties[Constants.IdentifierProperty] == Constants.NewFeatureID ? await FeatureStore.AddItem(LogFeature) : await FeatureStore.UpdateItem(LogFeature);
        }

        private async Task ShareLog(View element)
        {
            if (IsBusy) return;

            IsBusy = true;

            System.Drawing.Rectangle bounds = element.GetAbsoluteBounds().ToSystemRectangle();

            if (Preferences.Get(Constants.ShareLogAsGeoJSONKey, false))
            {
                ShareFileRequest share = new ShareFileRequest
                {
                    Title = "Share Feature",
                    File = new ShareFile(await FeatureStore.ExportFeature(LogFeature), "application/json")
                };
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

        private void HandleMessages()
        {
            MessagingCenter.Subscribe<DisplayPosition>(this, "Location", message =>
            {
                LogPositions.Add(message);
                DateTimeList.Add(message.Index);
                try
                {
                    SaveLog();
                }
                catch { }
            });

            MessagingCenter.Subscribe<StopServiceMessage>(this, "ServiceStopped", message => { ToggleButtonLabel = "Start"; });

            MessagingCenter.Subscribe<LocationErrorMessage>(this, "LocationError", message =>
            {
                ToggleLogging();
                Device.BeginInvokeOnMainThread(async () =>
                {
                    await Application.Current.MainPage.DisplayAlert("Unable To Fetch Location", "Ensure Groundsman has access to your device's location.", "Ok");
                });
            });
        }

        public void Unsubscribe()
        {
            MessagingCenter.Unsubscribe<Position>(this, "Location");
            MessagingCenter.Unsubscribe<StopServiceMessage>(this, "ServiceStopped");
            MessagingCenter.Unsubscribe<StartServiceMessage>(this, "ServiceStarted");
            MessagingCenter.Unsubscribe<LocationErrorMessage>(this, "LocationError");
        }
    }
}
