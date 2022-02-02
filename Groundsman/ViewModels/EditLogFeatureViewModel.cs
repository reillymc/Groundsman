using System.Collections;
using System.Linq;
using System.Windows.Input;
using Groundsman.Misc;
using Groundsman.Models;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Groundsman.ViewModels;

public class EditLogFeatureViewModel : BaseEditFeatureViewModel
{
    public ICommand ToggleButtonClickCommand { set; get; }
    public ICommand ClearButtonClickCommand { set; get; }

    public Feature OldLogFeature;
    private readonly List<string> DateTimeList = new List<string>();

    public bool isLogging;

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

    /// <summary>
    /// ViewModel constructor for creating a new log feature.
    /// </summary>
    public EditLogFeatureViewModel()
    {
        Title = "New Log Line";

        Id = Guid.NewGuid().ToString();

        AddDefaultProperties();

        DateEntry = DateTime.Now;

        GeometryType = GeoJSONType.LineString;

        ShowLogEditor = true;

        InitCommandBindings();
        UpdateMap();
    }

    /// <summary>
    /// ViewModel constructor for editing an existing log feature.
    /// </summary>
    public EditLogFeatureViewModel(Feature feature)
    {
        InitialiseExisitingFeature(feature);

        int index;
        LineString lineString = (LineString)feature.Geometry;
        try
        {
            index = 0;
            object propertyValue = feature.Properties[Constants.LogTimestampsProperty];
            string[] timestamps = ((IEnumerable)propertyValue).Cast<object>()
                             .Select(x => x.ToString())
                             .ToArray();
            foreach (Position position in lineString.Coordinates)
            {
                DateTimeList.Add(timestamps[index]);
                Positions.Insert(0, new DisplayPosition(timestamps[index], position));
                index++;
            }
        }
        catch
        {
            index = 1;
            foreach (Position pos in lineString.Coordinates)
            {
                Positions.Add(new DisplayPosition(index.ToString(), pos));
                index++;
            }
        }

        OldLogFeature = new Feature(feature.Geometry, feature.Properties);

        ShowLogEditor = true;

        InitCommandBindings();
        UpdateMap();
    }

    /// <summary>
    /// Initialise page-specific command bindings.
    /// </summary>
    private void InitCommandBindings()
    {
        ToggleButtonClickCommand = new Command(() => ToggleLogging());
        ClearButtonClickCommand = new Command(() => ClearLog());
        ShareButtonClickCommand = new Command<View>(async (view) => await ShareFeature(view));
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

    public void ClearLog()
    {
        Positions.Clear();
        UpdateMap();
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

    public async Task ShareFeature(View element)
    {
        if (IsBusy) return;
        IsBusy = true;

        try
        {
            if (await SaveLog() > 0)
            {
                Feature saveFeature = GetValidatedFeature();
                saveFeature.Geometry = new LineString(Positions.Reverse().Select(pointValue => new Position(pointValue)).ToList());

                if (Preferences.Get(Constants.LoggerExportFormatKey, Constants.LoggerExportFormatDefaultValue) == 1)
                {
                    await ShareFeature(saveFeature, element);
                }
                else
                {
                    System.Drawing.Rectangle bounds = System.Drawing.Rectangle.Empty;
                    if (DeviceInfo.Platform == DevicePlatform.iOS && DeviceInfo.Idiom == DeviceIdiom.Tablet)
                    {
                        if (element != null) bounds = element.GetAbsoluteBounds().ToSystemRectangle();
                    }
                    var share = new ShareFileRequest
                    {
                        Title = "Share Log",
                        File = new ShareFile(await FeatureHelper.ExportLog(saveFeature), "text/csv"),
                        PresentationSourceBounds =  bounds
                    };
                    await Share.RequestAsync(share);
                }
            }
        }
        catch (Exception ex)
        {
            await NavigationService.ShowAlert("Invalid Feature Data", $"{ex.Message}", false);
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
