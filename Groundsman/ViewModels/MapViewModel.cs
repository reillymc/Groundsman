using System.Threading.Tasks;
using Groundsman.Models;
using Groundsman.Services;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Maps;
using Point = Groundsman.Models.Point;
using Polygon = Groundsman.Models.Polygon;
using Position = Groundsman.Models.Position;
using XFMPolygon = Xamarin.Forms.Maps.Polygon;
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
                Point point = (Point)feature.Geometry;
                string address = double.IsNaN(point.Coordinates.Altitude)
                    ? $"{point.Coordinates.Longitude}, {point.Coordinates.Latitude}"
                    : $"{point.Coordinates.Longitude}, {point.Coordinates.Latitude}, {point.Coordinates.Altitude}";
                Pin pin = new Pin
                {
                    Label = (string)feature.Properties["name"],
                    Address = address,
                    Type = PinType.Place,
                    Position = new XFMPosition(point.Coordinates.Latitude, point.Coordinates.Longitude),
                };
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
                Polyline polyline = new Polyline
                {
                    StrokeColor = Color.OrangeRed,
                    StrokeWidth = 5,
                };
                LineString lineString = (LineString)feature.Geometry;
                lineString.Coordinates.ForEach((Position point) =>
                {
                    polyline.Geopath.Add(new XFMPosition(point.Latitude, point.Longitude));
                });
                Map.MapElements.Add(polyline);
            }
        }

        private void DrawPolygon(Feature feature)
        {
            if (Preferences.Get(Constants.MapDrawPolygonsKey, true))
            {
                XFMPolygon xfmpolygon = new XFMPolygon
                {
                    StrokeWidth = 4,
                    StrokeColor = Color.OrangeRed,
                    FillColor = Color.OrangeRed.AddLuminosity(.1).MultiplyAlpha(0.6),
                };

                Polygon polygon = (Polygon)feature.Geometry;
                foreach (LineString lineString in polygon.Coordinates)
                {
                    foreach (Position pos in lineString.Coordinates)
                    {
                        xfmpolygon.Geopath.Add(new XFMPosition(pos.Latitude, pos.Longitude));
                    }
                }
                Map.MapElements.Add(xfmpolygon);
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
            string result = await NavigationService.GetCurrentPage().DisplayActionSheet((string)feature.Properties[Constants.NameProperty], "Dismiss", "Delete", "View");

            switch (result)
            {
                case "Delete":
                    shakeService.Start();
                    _ = FeatureStore.DeleteItem(feature);
                    RefreshMap();
                    break;
                case "View":
                    if (feature.Properties.ContainsKey(Constants.LogDateTimeListProperty))
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
