using Groundsman.Models;
using Groundsman.Services;
using System.Collections.Generic;
using System.Threading.Tasks;
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
            MessagingCenter.Subscribe<LogStore>(this, "LogUpdated", (sender) => { DrawLogPath(); });
        }

        public Position defaultMapCentre = new Position(153.021, -27.47);

        // Only center map on user if location permissions are granted
        private async void CenterMapOnUser()
        {
            var status = await HelperServices.CheckAndRequestPermissionAsync(new Permissions.LocationWhenInUse());
            if (status != PermissionStatus.Granted)
            {
                Map.MoveToRegion(MapSpan.FromCenterAndRadius(new XFMPosition(defaultMapCentre.Latitude, defaultMapCentre.Longitude), Distance.FromMiles(1.0)));
                return;
            }
            else
            {
                Position location = await HelperServices.GetGeoLocation();
                Map.MoveToRegion(MapSpan.FromCenterAndRadius(new XFMPosition(location.Latitude, location.Longitude), Distance.FromMiles(1.0)));
            }
        }

        public void CleanFeatures()
        {
            Map.MapElements.Clear();
            Map.Pins.Clear();
        }

        public async void RefreshMap()
        {
            CleanFeatures();
            DrawFeatures();
            DrawLogPath();

            //SetShowingUser
            var status = await HelperServices.CheckAndRequestPermissionAsync(new Permissions.LocationWhenInUse());
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
            if (Preferences.Get("ShowPointsOnMap", true))
            {
                Point point = (Point)feature.Geometry;
                Pin pin = new Pin
                {
                    Label = (string)feature.Properties["name"],
                    Address = string.Format("{0}, {1}, {2}", point.Coordinates.Longitude, point.Coordinates.Latitude, point.Coordinates.Altitude),
                    Type = PinType.Place,
                    Position = new XFMPosition(point.Coordinates.Latitude, point.Coordinates.Longitude),
                };
                pin.MarkerClicked += async (sender, e) =>
                {
                    await DisplayFeatureActionMenuAsync(feature);
                };
                Map.Pins.Add(pin);
            }
        }

        private void DrawLineString(Feature feature)
        {
            if (Preferences.Get("ShowLinesOnMap", true))
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
            if (Preferences.Get("ShowPolygonsOnMap", true))
            {
                XFMPolygon xfmpolygon = new XFMPolygon
                {
                    StrokeWidth = 4,
                    StrokeColor = Color.OrangeRed,
                    FillColor = Color.FromHex("#85cb5748"),
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

        private void DrawLogPath()
        {
            if (Preferences.Get("ShowLogPathOnMap", true))
            {
                List<Position> logFile = LogStore.LogPoints;
                Polyline logPolyline = new Polyline
                {
                    StrokeColor = Color.DarkOrange,
                    StrokeWidth = 3,
                };
                logFile.ForEach((Position point) =>
                {
                    logPolyline.Geopath.Add(new XFMPosition(point.Latitude, point.Longitude));
                });
                Map.MapElements.Add(logPolyline);
            }
        }

        /// <summary>
        /// When map is clicked, iterates through all features in the feature list and shows the feature info menu if feature has been tapped
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void OnMapClicked(object sender, MapClickedEventArgs e)
        {
            FeatureList.ForEach(async (Feature feature) =>
            {
                bool ItemHit = false;

                if (feature.Geometry.Type == GeoJSONType.Polygon && Preferences.Get("ShowPolygonsOnMap", true))
                {
                    Polygon polygon = (Polygon)feature.Geometry;
                    ItemHit |= polygon.ContainsPosition(new Position(e.Position.Longitude, e.Position.Latitude));
                }
                else if (feature.Geometry.Type == GeoJSONType.LineString && Preferences.Get("ShowLinesOnMap", true))
                {
                    LineString lineString = (LineString)feature.Geometry;    
                    ItemHit |= lineString.ContainsPosition(new Position(e.Position.Longitude, e.Position.Latitude, 0));
                }

                if (ItemHit)
                {
                    await DisplayFeatureActionMenuAsync(feature);
                }
            });
            if (Preferences.Get("ShowLogPathOnMap", true))
            {
                LineString logLine = new LineString(LogStore.LogPoints);
                if (logLine.ContainsPosition(new Position(e.Position.Longitude, e.Position.Latitude)))
                {
                    await DisplayLogActionMenuAsync();
                }
            }
        }

        async Task DisplayFeatureActionMenuAsync(Feature feature)
        {
            string result = await NavigationService.GetCurrentPage().DisplayActionSheet((string)feature.Properties["name"], "Dismiss", "Delete", "View", "Edit");

            switch (result)
            {
                case "Delete":
                    bool yesResponse = await NavigationService.GetCurrentPage().DisplayAlert("Delete Feature", "Are you sure you want to delete this feature?", "Yes", "No");
                    if (yesResponse)
                    {
                        await FeatureStore.DeleteItemAsync(feature);
                        RefreshMap();
                    }
                    break;
                case "View":
                    await NavigationService.NavigateToDetailPage(feature);
                    break;
                case "Edit":
                    await NavigationService.NavigateToEditPage(feature);
                    break;
                default:
                    break;
            }
        }

        async Task DisplayLogActionMenuAsync()
        {
            string result = await NavigationService.GetCurrentPage().DisplayActionSheet("Logger Path", "Dismiss", "Clear");

            if (result == "Clear")
            {
                LogStore.ClearLog();
                RefreshMap();
            }
        }
    }
}
