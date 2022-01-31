using System;
using System.Collections;
using System.Collections.Generic;
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



        public EditLogFeatureViewModel()
        {
            Title = "New Log Line";

            Id = Guid.NewGuid().ToString();

            Properties.Add(new Property("String Property", string.Empty, 0));
            Properties.Add(new Property("Integer Property", null, 1));
            Properties.Add(new Property("Float Property", null, 2));

            DateEntry = DateTime.Now;

            GeometryType = GeoJSONType.LineString;

            ShowLogEditor = true;

            InitCommands();
            UpdateMap();
        }

        public EditLogFeatureViewModel(Feature Log)
        {
            Title = NameEntry = Log.Name;
            DateEntry = Log.Date;

            Id = Log.Id;
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
                index++;
            }

            foreach (KeyValuePair<string, object> property in Log.Properties)
            {
                if (!Constants.GroundsmanProperties.Contains(property.Key))
                {
                    Properties.Add(Property.FromObject(property.Key.ToString(), property.Value));
                }
            }

            OldLogFeature = new Feature(Log.Geometry, Log.Properties);

            ShowLogEditor = true;

            InitCommands();
            UpdateMap();
        }

        private void InitCommands()
        {
            ToggleButtonClickCommand = new Command(() => ToggleLogging());
            ClearButtonClickCommand = new Command(() => ClearLog());
        }

        public void ClearLog()
        {
            Positions.Clear();
            UpdateMap();
        }

        public override async Task CancelDismiss()
        {
            try
            {
                if (isLogging)
                {
                    ToggleLogging();
                }
                await SaveLog(true);
            }
            catch { }

            await OnDismiss(true);
        }

        public override void OnDisappear()
        {
            Unsubscribe();
            if (isLogging)
            {
                ToggleLogging();
            }
        }

        public override void OnAppear()
        {
            Subscribe();
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
            await FeatureStore.DeleteItem(Id);
            await FeatureStore.GetItemsAsync();
        }

        private async Task<int> SaveLog(bool reset = false)
        {
            if (reset)
            {
                return OldLogFeature != null ? await FeatureStore.SaveItem(OldLogFeature) : await FeatureStore.DeleteItem(Id);
            }

            try
            {
                Feature saveFeature = GetValidatedFeature();
                saveFeature.Properties[Constants.LogTimestampsProperty] = DateTimeList.ToArray();

                return await FeatureStore.SaveItem(saveFeature);
            }
            catch
            {
                // Can safely ignore log not saving
                return 0;
            }
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

                    Feature saveFeature = GetValidatedFeature();
                    LineString line = new LineString(Positions.Reverse().Select(pointValue => new Position(pointValue)).ToList());
                    saveFeature.Geometry = line;

                    ShareFileRequest share;
                    if (Preferences.Get(Constants.LoggerExportFormatKey, 0) == 1)
                    {
                        share = new ShareFileRequest
                        {
                            Title = "Share Feature",
                            File = new ShareFile(await FeatureHelper.ExportFeatures(saveFeature), "application/json")
                        };
                    }
                    else
                    {
                        share = new ShareFileRequest
                        {
                            Title = "Share Log",
                            File = new ShareFile(await FeatureHelper.ExportLog(saveFeature), "text/csv"),
                            PresentationSourceBounds = DeviceInfo.Platform == DevicePlatform.iOS && DeviceInfo.Idiom == DeviceIdiom.Tablet ? bounds : System.Drawing.Rectangle.Empty
                        };
                    }
                    share.PresentationSourceBounds = DeviceInfo.Platform == DevicePlatform.iOS && DeviceInfo.Idiom == DeviceIdiom.Tablet ? bounds : System.Drawing.Rectangle.Empty;
                    await Share.RequestAsync(share);
                }
            }
            catch (Exception e)
            {
                await Application.Current.MainPage.DisplayAlert("Unable To Share Log", $"{e.Message}.", "Ok");
            }
            IsBusy = false;
        }

        private void Subscribe()
        {
            MessagingCenter.Subscribe<DisplayPosition>(this, "Location", message =>
            {
                Positions.Insert(0, message);
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
            MessagingCenter.Unsubscribe<DisplayPosition>(this, "Location");
            MessagingCenter.Unsubscribe<StopServiceMessage>(this, "ServiceStopped");
            MessagingCenter.Unsubscribe<StartServiceMessage>(this, "ServiceStarted");
            MessagingCenter.Unsubscribe<LocationErrorMessage>(this, "LocationError");
        }
    }
}
