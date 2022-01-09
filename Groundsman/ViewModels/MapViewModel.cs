using System.Threading.Tasks;
using Groundsman.Misc;
using Groundsman.Models;
using Groundsman.Services;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Maps;
using Polygon = Groundsman.Models.Polygon;
using Position = Groundsman.Models.Position;
using XFMPosition = Xamarin.Forms.Maps.Position;

namespace Groundsman.ViewModels
{
    /// <summary>
    /// ViewModel for maps page
    /// Notes Xamarin Forms Maps Position Long/Lat is inverted to Lat/Long
    /// </summary>
    public class MapViewModel : BaseViewModel
    {
        public CustomMap Map { get; private set; }

        public MapViewModel()
        {
            Map = new CustomMap();
            CenterMapOnUser();
            Map.MapClicked += OnMapClicked;
        }

        // Only center map on user if location permissions are granted otherwise center on Brisbane
        private async void CenterMapOnUser()
        {
            try
            {
                Position location = await HelperServices.GetGeoLocation();
                Map.MoveToRegion(MapSpan.FromCenterAndRadius(new XFMPosition(location.Latitude, location.Longitude), Distance.FromMiles(1.0)));
            }
            catch
            {
                Map.MoveToRegion(MapSpan.FromCenterAndRadius(new XFMPosition(-27.47, 153.021), Distance.FromMiles(1.0)));
            }
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
            string result = await NavigationService.GetCurrentPage().DisplayActionSheet(feature.Name, "Dismiss", "Delete", "View");

            switch (result)
            {
                case "Delete":
                    shakeService.Start();
                    await FeatureStore.DeleteItem(feature);
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
                default:
                    break;
            }
        }
    }
}
