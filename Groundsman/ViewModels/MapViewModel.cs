using Groundsman.Misc;
using Groundsman.Models;
using Groundsman.Services;
using Xamarin.Essentials;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Maps;
using Polygon = Groundsman.Models.Polygon;
using Position = Groundsman.Models.Position;
using XFMPosition = Xamarin.Forms.Maps.Position;

namespace Groundsman.ViewModels;

/// <summary>
/// ViewModel for maps page
/// Notes Xamarin Forms Maps Position Long/Lat is inverted to Lat/Long
/// </summary>
public class MapViewModel : BaseViewModel
{
    public CustomMap Map { get; private set; }

    public MapViewModel()
    {
        Map = new CustomMap(Constants.DefaultLocation);
        CenterMapOnUser();
        Map.MapClicked += OnMapClicked;
    }

    // Center map on user if location permissions are granted;
    private async void CenterMapOnUser()
    {
        try
        {
            Position location = await HelperServices.GetGeoLocation();
            Map.MoveToRegion(MapSpan.FromCenterAndRadius(new XFMPosition(location.Latitude, location.Longitude), Distance.FromMiles(1.0)));
        }
        catch { } // Silenty fail
    }

    public async void RefreshMap()
    {
        // Clear Features
        Map.MapElements.Clear();
        Map.Pins.Clear();

        await FeatureStore.GetItemsAsync();
        DrawFeatures();

        //SetShowingUser
        PermissionStatus status = await HelperServices.CheckAndRequestPermissionAsync(new Permissions.LocationWhenInUse());
        Map.IsShowingUser = status == PermissionStatus.Granted;
    }

    /// <summary>
    /// Iterates through all features in the feature list and calls the apropriate draw method baysed on feature type
    /// </summary>
    public void DrawFeatures()
    {
        FeatureList.ForEach((Feature feature) =>
        {
            switch (feature.Geometry.Type)
            {
                case GeoJSONType.Point:
                    DrawPoint(feature);
                    break;
                case GeoJSONType.LineString:
                    DrawLineString(feature);
                    break;
                case GeoJSONType.Polygon:
                    DrawPolygon(feature);
                    break;
            }
        });
    }

    private void DrawPoint(Feature feature)
    {
        if (Preferences.Get(Constants.MapDrawPointsKey, true))
        {
            var pin = MapHelper.GeneratePin(feature);
            pin.InfoWindowClicked += async (sender, e) =>
            {
                await DisplayFeatureActionMenuAsync(feature);
                e.HideInfoWindow = true;
            };
            Map.Pins.Add(pin);
        }
    }

    private void DrawLineString(Feature feature)
    {
        if (Preferences.Get(Constants.MapDrawLinesKey, true))
        {
            Map.MapElements.Add(MapHelper.GenerateLine(feature));
        }
    }

    private void DrawPolygon(Feature feature)
    {
        if (Preferences.Get(Constants.MapDrawPolygonsKey, true))
        {
            Map.MapElements.Add(MapHelper.GeneratePolygon(feature));
        }
    }

    /// <summary>
    /// When map is clicked, iterates through all features in the feature list and shows the feature info menu if feature has been tapped
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnMapClicked(object sender, MapClickedEventArgs e)
    {
        FeatureList.ForEach((Feature feature) =>
        {
            bool ItemHit = false;

            if (feature.Geometry.Type == GeoJSONType.Polygon && Preferences.Get(Constants.MapDrawPolygonsKey, true))
            {
                Polygon polygon = (Polygon)feature.Geometry;
                ItemHit |= polygon.ContainsPosition(new Position(e.Position.Longitude, e.Position.Latitude));
            }
            else if (feature.Geometry.Type == GeoJSONType.LineString && Preferences.Get(Constants.MapDrawLinesKey, true))
            {
                LineString lineString = (LineString)feature.Geometry;
                ItemHit |= lineString.ContainsPosition(new Position(e.Position.Longitude, e.Position.Latitude, 0));
            }

            if (ItemHit)
            {
                _ = DisplayFeatureActionMenuAsync(feature);
            }
        });
    }

    private async Task DisplayFeatureActionMenuAsync(Feature feature)
    {
        string result = await NavigationService.GetCurrentPage().DisplayActionSheet(feature.Name, "Dismiss", "Delete", "View", "Share");

        switch (result)
        {
            case "Delete":
                shakeService.Start();
                _ = await FeatureStore.DeleteItem(feature.Id);
                _ = await FeatureStore.GetItemsAsync();
                RefreshMap();
                break;
            case "View":
                if (feature.Properties.ContainsKey(Constants.LogTimestampsProperty))
                {
                    await NavigationService.NavigateToLoggerPage(feature);
                }
                else
                {
                    await NavigationService.NavigateToEditPage(feature);
                }
                break;
            case "Share":
                await ShareFeature(feature);
                break;
            default:
                break;
        }
    }


}
