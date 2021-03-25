using Groundsman.Models;
using Groundsman.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Maps;
using Position = Groundsman.Models.Position;
using XFMPosition = Xamarin.Forms.Maps.Position;
using Polygon = Groundsman.Models.Polygon;
using XFMPolygon = Xamarin.Forms.Maps.Polygon;
using Point = Groundsman.Models.Point;

namespace Groundsman.ViewModels
{
    /// <summary>
    /// ViewModel for maps page
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

        // Only center map on user if location permissions are granted
        private async void CenterMapOnUser()
        {
            var status = await HelperServices.CheckAndRequestPermissionAsync(new Permissions.LocationWhenInUse());
            if (status != PermissionStatus.Granted)
            {
                Map.MoveToRegion(MapSpan.FromCenterAndRadius(new Xamarin.Forms.Maps.Position(-27.47004901089882, 153.021072), Distance.FromMiles(1.0)));
                return;
            }
            else
            {
                Position location = await HelperServices.GetGeoLocation();
                Map.MoveToRegion(MapSpan.FromCenterAndRadius(new Xamarin.Forms.Maps.Position(location.Latitude, location.Longitude), Distance.FromMiles(1.0)));
            }
        }

        public void CleanFeatures()
        {
            Map.MapElements.Clear();
            Map.Pins.Clear();
        }

        public void DrawFeatures()
        {
            // Using CurrentFeature to draw the geodata on the map
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

        private void DrawPolygon(Feature feature)
        {
            if (!Preferences.Get("ShowPolygonsOnMap", true))
            {
                return;
            }
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

        private void DrawLineString(Feature feature)
        {
            if (!Preferences.Get("ShowLinesOnMap", true))
            {
                return;
            }
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

        private void DrawPoint(Feature feature)
        {
            if (!Preferences.Get("ShowPointsOnMap", true))
            {
                return;
            }
            Point point = (Point)feature.Geometry;
            Pin pin = new Pin
            {
                Label = (string)feature.Properties["name"],
                Address = string.Format("{0}, {1}, {2}", point.Coordinates.Latitude, point.Coordinates.Longitude, point.Coordinates.Altitude),
                Type = PinType.Place,
                Position = new XFMPosition(point.Coordinates.Latitude, point.Coordinates.Longitude),
            };
            pin.MarkerClicked += async (sender, e) =>
            {
                await DisplayFeatureActionMenuAsync(feature);
            };
            Map.Pins.Add(pin);
        }

        public async void RefreshMap()
        {
            GetFeatures();
            CleanFeatures();
            DrawFeatures();
            DrawLogPath();

            //SetShowingUser
            var status = await HelperServices.CheckAndRequestPermissionAsync(new Permissions.LocationWhenInUse());
            if (status == PermissionStatus.Granted)
            {
                Map.IsShowingUser = true;
            }
            else
            {
                Map.IsShowingUser = false;
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
                    logPolyline.Geopath.Add(new Xamarin.Forms.Maps.Position(point.Latitude, point.Longitude));
                });
                Map.MapElements.Add(logPolyline);
            }
        }

        async void OnMapClicked(object sender, MapClickedEventArgs e)
        {
            FeatureList.ForEach(async (Feature feature) =>
            {
                bool ItemHit = false;

                if (feature.Geometry.Type == GeoJSONType.Polygon && Preferences.Get("ShowPolygonsOnMap", true))
                {
                    Polygon polygon = (Polygon)feature.Geometry;
                    List<Position> posList = new List<Position>();
                    foreach (LineString lineString in polygon.Coordinates)
                    {
                        foreach (Position pos in lineString.Coordinates)
                        {
                            posList.Add(pos);
                        }
                    }
                    ItemHit |= IsPointInPolygon(new Position(e.Position.Longitude, e.Position.Latitude, 0), posList);
                }
                else if (feature.Geometry.Type == GeoJSONType.LineString && Preferences.Get("ShowLinesOnMap", true))
                {
                    LineString lineString = (LineString)feature.Geometry;
                    List<Position> positions = new List<Position>(lineString.Coordinates);
    
                    ItemHit |= IsPointOnLine(new Position(e.Position.Longitude, e.Position.Latitude, 0), positions);
                }

                if (ItemHit)
                {
                    await DisplayFeatureActionMenuAsync(feature);
                }
            });
            if (Preferences.Get("ShowLogPathOnMap", true))
            {
                if (IsPointOnLine(new Position(e.Position.Longitude, e.Position.Latitude, 0), LogStore.LogPoints))
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

        public bool IsPointInPolygon(Position p, List<Position> polygon)
        {
            double minX = polygon[0].Longitude;
            double maxX = polygon[0].Longitude;
            double minY = polygon[0].Latitude;
            double maxY = polygon[0].Latitude;
            for (int i = 1; i < polygon.Count; i++)
            {
                Position q = polygon[i];
                minX = Math.Min(q.Longitude, minX);
                maxX = Math.Max(q.Longitude, maxX);
                minY = Math.Min(q.Latitude, minY);
                maxY = Math.Max(q.Latitude, maxY);
            }

            if (p.Longitude < minX || p.Longitude > maxX || p.Latitude < minY || p.Latitude > maxY)
            {
                return false;
            }

            // http://www.ecse.rpi.edu/Homepages/wrf/Research/Short_Notes/pnpoly.html
            bool inside = false;
            for (int i = 0, j = polygon.Count - 1; i < polygon.Count; j = i++)
            {
                if ((polygon[i].Latitude > p.Latitude) != (polygon[j].Latitude > p.Latitude) &&
                     p.Longitude < (polygon[j].Longitude - polygon[i].Longitude) * (p.Latitude - polygon[i].Latitude) / (polygon[j].Latitude - polygon[i].Latitude) + polygon[i].Longitude)
                {
                    inside = !inside;
                }
            }
            return inside;
        }

        public bool IsPointOnLine(Position LocationTapped, List<Position> polyline)
        {
            double Lat1;
            double Lat2;
            double Lon1;
            double Lon2;
            double PointLat;
            double PointLon;
            double AB;
            double AP;
            double PB;
            double delta = 0.000075; // delta determines line tap accuracy

            for (int i = 1; i < polyline.Count; i++)
            {
                Lat1 = polyline[i - 1].Latitude;
                Lat2 = polyline[i].Latitude;

                Lon1 = polyline[i - 1].Longitude;
                Lon2 = polyline[i].Longitude;

                PointLat = LocationTapped.Latitude;
                PointLon = LocationTapped.Longitude;

                AB = Math.Sqrt((Lat2 - Lat1) * (Lat2 - Lat1) + (Lon2 - Lon1) * (Lon2 - Lon1));
                AP = Math.Sqrt((PointLat - Lat1) * (PointLat - Lat1) + (PointLon - Lon1) * (PointLon - Lon1));
                PB = Math.Sqrt((Lat2 - PointLat) * (Lat2 - PointLat) + (Lon2 - PointLon) * (Lon2 - PointLon));

                // Check if position is between two points of a line within distance of delta from the line
                if (Math.Abs(AB - (AP + PB)) < delta)
                    return true;
            }
            return false;
        }

        private async void GetFeatures()
        {
            ObservableCollection<Feature> updates = await FeatureStore.GetItemsAsync();
            FeatureList.ReplaceRange(updates);
        }
    }
}