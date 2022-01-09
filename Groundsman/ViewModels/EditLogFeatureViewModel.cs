using System;
using System.Collections;
using System.Collections.Generic;
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
    public class EditLogFeatureViewModel : BaseEditFeatureViewModel
    {
        public ICommand ToggleButtonClickCommand { set; get; }
        public ICommand ClearButtonClickCommand { set; get; }

        public Feature OldLogFeature;
        private readonly List<string> DateTimeList = new List<string>();
        private List<DisplayPosition> OrderedPositions { get; set; } = new List<DisplayPosition>();

        public bool isLogging;

        public bool isLogLine { get; set; } = true;
        public bool isRegularFeature { get; set; } = false;
        public bool ShowAddButton { get; set; } = false;
        public bool ShowClosePolygon { get; set; } = false;

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



        public EditLogFeatureViewModel()
        {
            Title = "New Log Line";

            Feature.Properties = new Dictionary<string, object>
            {
                [Constants.IdentifierProperty] = Constants.NewFeatureID
            };
            DateEntry = DateTime.Now.ToShortDateString();

            GeometryType = GeoJSONType.LineString;

            InitCommands();
            HandleMessages();
        }

        public EditLogFeatureViewModel(Feature Log)
        {
            Feature.Geometry = Log.Geometry;
            Feature.Properties = Log.Properties;
            Title = NameEntry = Log.Name;
            DateEntry = Log.Date;
            Feature.Id = Log.Id;

            GeometryType = GeoJSONType.LineString;
            IsExistingFeature = true;


            object test = Log.Properties[Constants.LogTimestampsProperty];
            string[] timestamps = ((IEnumerable)test).Cast<object>()
                             .Select(x => x.ToString())
                             .ToArray();
            LineString line = (LineString)Log.Geometry;
            int index = 0;
            foreach (Position position in line.Coordinates)
            {
                DateTimeList.Add(timestamps[index]);
                Positions.Insert(0, new DisplayPosition(timestamps[index], position));
                OrderedPositions.Add(new DisplayPosition(timestamps[index], position));
                index++;
            }

            OldLogFeature = new Feature(Log.Geometry, Log.Properties);
            InitCommands();
            HandleMessages();
        }

        private void InitCommands()
        {
            ToggleButtonClickCommand = new Command(() => { ToggleLogging(); });
            ClearButtonClickCommand = new Command(() => { Positions.Clear(); OrderedPositions.Clear(); });
        }

        public override async Task CancelDismiss()
        {
            try
            {
                Unsubscribe();
                if (isLogging)
                {
                    ToggleLogging();
                }
                await SaveLog(true);
            }
            catch { }

            await OnDismiss(true);
        }

        public override void AnyDismiss()
        {
            Unsubscribe();
            if (isLogging)
            {
                ToggleLogging();
            }
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

        public override async Task SaveDismiss()
        {
            try
            {
                Unsubscribe();
                if (isLogging)
                {
                    ToggleLogging();
                }
                if (await SaveLog() > 0)
                {
                    // Run in parallel?
                    _ = await FeatureStore.GetItemsAsync();
                    await OnDismiss(true);
                }
            }
            catch (Exception e)
            {
                await Application.Current.MainPage.DisplayAlert("Unable To Save Log", $"{e.Message}.", "Ok");
            }
        }

        public override async Task DeleteDismiss()
        {
            shakeService.Start();
            await NavigationService.NavigateBack(true);
            await FeatureStore.DeleteItem(Feature);
            await FeatureStore.GetItemsAsync();
        }

        private async Task<int> SaveLog(bool reset = false)
        {
            if (reset)
            {
                return OldLogFeature != null ? await FeatureStore.SaveItem(OldLogFeature) : await FeatureStore.DeleteItem(Feature);
            }

            List<Position> posList = new List<Position>();
            foreach (DisplayPosition displayPosition in OrderedPositions)
            {
                posList.Add(new Position(displayPosition));
            }

            Feature.Geometry = new LineString(posList);
            Feature.Name = NameEntry ?? "Log LineString";
            Feature.Date = DateTime.Parse(DateEntry).ToShortDateString();
            Feature.Author = Preferences.Get(Constants.UserIDKey, Constants.DefaultUserValue);

            if (Feature.Id == null)
            {
                Feature.Id = Guid.NewGuid().ToString();
            }
            Feature.Properties[Constants.LogTimestampsProperty] = DateTimeList.ToArray();

            return await FeatureStore.SaveItem(Feature);
        }

        public override async Task ShareFeature(View element)
        {
            if (IsBusy) return;

            IsBusy = true;

            try
            {
                if (await SaveLog() > 0)
                {
                    System.Drawing.Rectangle bounds = element.GetAbsoluteBounds().ToSystemRectangle();

                    if (Preferences.Get(Constants.LoggerExportFormatKey, 0) == 1)
                    {
                        ShareFileRequest share = new ShareFileRequest
                        {
                            Title = "Share Feature",
                            File = new ShareFile(await FeatureHelper.ExportFeatures(Feature), "application/json")
                        };
                        share.PresentationSourceBounds = DeviceInfo.Platform == DevicePlatform.iOS && DeviceInfo.Idiom == DeviceIdiom.Tablet ? bounds : System.Drawing.Rectangle.Empty;
                        await Share.RequestAsync(share);
                    }
                    else
                    {
                        string LogString = "Timestamp, Longitude, Latitude, Altitude\n";
                        foreach (DisplayPosition position in OrderedPositions)
                        {
                            LogString += $"{position}\n";
                        }

                        File.WriteAllText(Constants.EXPORT_LOG_FILE, LogString);
                        await Share.RequestAsync(new ShareFileRequest
                        {
                            Title = "Share Log",
                            File = new ShareFile(Constants.EXPORT_LOG_FILE, "text/csv"),
                            PresentationSourceBounds = DeviceInfo.Platform == DevicePlatform.iOS && DeviceInfo.Idiom == DeviceIdiom.Tablet ? bounds : System.Drawing.Rectangle.Empty
                        });
                    }

                }
            }
            catch (Exception e)
            {
                await Application.Current.MainPage.DisplayAlert("Unable To Share Log", $"{e.Message}.", "Ok");
            }
            IsBusy = false;
        }

        private void HandleMessages()
        {
            MessagingCenter.Subscribe<DisplayPosition>(this, "Location", message =>
            {
                Positions.Insert(0, message);
                OrderedPositions.Add(message);
                DateTimeList.Add(message.Index);
                try
                {
                    _ = SaveLog();
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
