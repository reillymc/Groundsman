using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using Groundsman.Misc;
using Groundsman.Models;
using Groundsman.Services;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Maps;
using Point = Groundsman.Models.Point;
using Polygon = Groundsman.Models.Polygon;
using Position = Groundsman.Models.Position;
using XFMPosition = Xamarin.Forms.Maps.Position;

namespace Groundsman.ViewModels;

public abstract class BaseEditFeatureViewModel : BaseViewModel
{
    public ICommand OnDoneTappedCommand { get; set; }
    public ICommand OnCancelTappedCommand { get; set; }
    public ICommand ShareButtonClickCommand { get; set; }
    public ICommand GetFeatureCommand { get; set; }
    public ICommand DeleteFeatureCommand { get; set; }
    public ICommand AddPropertyCommand { get; set; }
    public ICommand DeletePropertyCommand { get; set; }

    public GeoJSONType GeometryType { get; set; }

    public ObservableCollection<DisplayPosition> Positions { get; set; } = new();
    public ObservableCollection<DisplayProperty> Properties { get; set; } = new();

    public bool IsExistingFeature { get; set; } = false;
    public bool ShowLogEditor { get; set; } = false;
    public bool ShowMapPreview { get; set; } = false;

    public string Id;
    public string NameEntry { get; set; }
    public DateTime DateEntry { get; set; }

    public PreviewMap Map { get; private set; }

    public BaseEditFeatureViewModel()
    {
        OnDoneTappedCommand = new Command(async () => await SaveDismiss());
        OnCancelTappedCommand = new Command(async () => await CancelDismiss());
        DeleteFeatureCommand = new Command(async () => await DeleteDismiss());
        AddPropertyCommand = new Command(() => Properties.Add(new DisplayProperty("", "")));
        DeletePropertyCommand = new Command<DisplayProperty>((item) => Properties.Remove(item));

        Map = new PreviewMap
        {
            InputTransparent = true,
        };

        if (Preferences.Get(Constants.MapPreviewKey, false))
        {
            ShowMapPreview = true;
            _ = CenterMapOnUser();
        }

        Positions.CollectionChanged += Positions_CollectionChanged;
    }

    private void Positions_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems != null)
        {
            foreach (object item in e.NewItems)
            {
                ((INotifyPropertyChanged)item).PropertyChanged += ItemPropertyChanged;
            }
        }
        if (e.OldItems != null)
        {
            foreach (object item in e.OldItems)
            {
                ((INotifyPropertyChanged)item).PropertyChanged -= ItemPropertyChanged;
            }
        }
    }

    private void ItemPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != "Altitude")
        {
            UpdateMap();
        }
    }

    public void UpdateMap()
    {
        if (!Preferences.Get(Constants.MapPreviewKey, true)) return;

        if (Positions.Count > 0 && Positions.All<DisplayPosition>(position => !position.HasBlankCoordinate()))
        {
            Map.MapElements.Clear();
            Map.Pins.Clear();

            Position centerPosition;
            Position spanPosition;

            try
            {
                switch (GeometryType)
                {
                    case GeoJSONType.Point:
                        var pin = MapHelper.GeneratePin(new Feature(FeatureHelper.GetGeometry(Positions, GeoJSONType.Point)));
                        Map.Pins.Add(pin);
                        var point =  (Point)FeatureHelper.GetGeometry(Positions, GeoJSONType.Point);
                        centerPosition = new Position(point.Coordinates.Longitude, point.Coordinates.Latitude);
                        Map.MoveToRegion(MapSpan.FromCenterAndRadius(new XFMPosition(centerPosition.Latitude, centerPosition.Longitude), Distance.FromMiles(0.3)));
                        break;
                    case GeoJSONType.LineString:
                        Map.MapElements.Add(MapHelper.GenerateLine(new Feature(FeatureHelper.GetGeometry(Positions, GeoJSONType.LineString))));
                        var line = (LineString)FeatureHelper.GetGeometry(Positions, GeoJSONType.LineString);
                        centerPosition = line.GetCenterPosition();
                        spanPosition = line.GetSpan();
                        Map.MoveToRegion(new MapSpan(new XFMPosition(centerPosition.Latitude, centerPosition.Longitude), spanPosition.Latitude, spanPosition.Longitude));
                        break;
                    case GeoJSONType.Polygon:
                        Map.MapElements.Add(MapHelper.GeneratePolygon(new Feature(FeatureHelper.GetGeometry(Positions, GeoJSONType.Polygon))));
                        var polygon = (Polygon)FeatureHelper.GetGeometry(Positions, GeoJSONType.Polygon);
                        centerPosition = polygon.GetCenterPosition();
                        spanPosition = polygon.GetSpan();
                        Map.MoveToRegion(new MapSpan(new XFMPosition(centerPosition.Latitude, centerPosition.Longitude), spanPosition.Latitude, spanPosition.Longitude));
                        break;
                }
            }
            catch // Silently fail to render
            {
            }
        }
    }

    private async Task CenterMapOnUser()
    {
        var location = await HelperServices.GetGeoLocation();
        Map.MoveToRegion(MapSpan.FromCenterAndRadius(new XFMPosition(location.Latitude, location.Longitude), Distance.FromMiles(0.3)));

    }

    public Feature GetValidatedFeature()
    {
        return new Feature(FeatureHelper.GetGeometry(Positions, GeometryType), FeatureHelper.GetProperties(Properties))
        {
            Id = Id,
            Name = !string.IsNullOrEmpty(NameEntry) ? NameEntry : GeometryType.ToString(),
            Date = DateEntry,
            Author = Preferences.Get(Constants.UserIDKey, Constants.DefaultUserValue)
        };
    }

    public void AddDefaultProperties()
    {
        Properties.Add(new DisplayProperty("String Property", string.Empty, PropertyType.String));
        Properties.Add(new DisplayProperty("Integer Property", null, PropertyType.Integer));
        Properties.Add(new DisplayProperty("Float Property", null, PropertyType.Float));
    }

    public void InitialiseExisitingFeature(Feature feature)
    {
        Title = NameEntry = feature.Name;
        DateEntry = feature.Date;

        Id = feature.Id;
        GeometryType = feature.Geometry.Type;

        IsExistingFeature = true;


        foreach (KeyValuePair<string, object> property in feature.Properties)
        {
            if (!Constants.GroundsmanProperties.Contains(property.Key))
            {
                Properties.Add(DisplayProperty.FromObject(property.Key.ToString(), property.Value));
            }
        }
    }

    public async Task DeleteDismiss()
    {
        shakeService.Start();
        await NavigationService.NavigateBack(true);
        await FeatureStore.DeleteItem(Id);
        await FeatureStore.GetItemsAsync();
    }

    /// <summary>
    /// Save feature to database and dismiss page
    /// </summary>
    public abstract Task SaveDismiss();

    public abstract Task CancelDismiss();

    public virtual void OnAppear() { return; }
    public virtual void OnDisappear() { return; }
}
