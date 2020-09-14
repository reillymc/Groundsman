using Groundsman.Models;
using Groundsman.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Maps;
using Point = Groundsman.Models.Point;

namespace Groundsman.ViewModels
{
    /// <summary>
    /// ViewModel for maps page
    /// </summary>
    public class MapViewModel : BaseViewModel
    {
        private CancellationTokenSource cts;
        public MapViewModel()
        {
            Map = new CustomMap();
            CenterMapOnUser();
            Map.MapClicked += OnMapClicked;
        }

        public CustomMap Map { get; private set; }

        // Only center map on user if location permissions are granted
        private async void CenterMapOnUser()
        {
            var status = await HelperServices.CheckAndRequestPermissionAsync(new Permissions.LocationWhenInUse());
            if (status != PermissionStatus.Granted)
            {
                Map.MoveToRegion(MapSpan.FromCenterAndRadius(new Position(-27.47004901089882, 153.021072), Distance.FromMiles(1.0)));
                return;
            }
            else
            {
                Point location = await HelperServices.GetGeoLocation();
                Map.MoveToRegion(MapSpan.FromCenterAndRadius(new Position(location.Latitude, location.Longitude), Distance.FromMiles(1.0)));
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
                var points = feature.properties.xamarincoordinates;
                if (feature.geometry.type == FeatureType.Point && Preferences.Get("ShowPointsOnMap", true))
                {
                    Pin pin = new Pin
                    {
                        Label = feature.properties.name,
                        Address = string.Format("{0}, {1}, {2}", points[0].Latitude, points[0].Longitude, points[0].Altitude),
                        Type = PinType.Place,
                        Position = new Position(points[0].Latitude, points[0].Longitude),
                    };
                    pin.MarkerClicked += async (sender, e) =>
                    {
                        await DisplayFeatureActionMenuAsync(feature);
                    };
                    Map.Pins.Add(pin);
                }
                else if (feature.geometry.type == FeatureType.LineString && Preferences.Get("ShowLinesOnMap", true))
                {
                    Polyline polyline = new Polyline
                    {
                        StrokeColor = Color.OrangeRed,
                        StrokeWidth = 5,
                    };
                    points.ForEach((Point point) =>
                    {
                        polyline.Geopath.Add(new Position(point.Latitude, point.Longitude));
                    });
                    Map.MapElements.Add(polyline);
                }
                else if (feature.geometry.type == FeatureType.Polygon && Preferences.Get("ShowPolygonsOnMap", true))
                {
                    Polygon polygon = new Polygon
                    {
                        StrokeWidth = 4,
                        StrokeColor = Color.OrangeRed,
                        FillColor = Color.FromHex("#85cb5748"),
                    };
                    points.ForEach((Point point) =>
                    {
                        polygon.Geopath.Add(new Position(point.Latitude, point.Longitude));
                    });
                    Map.MapElements.Add(polygon);
                }
            });
        }

        public async void RefreshMap()
        {
            GetFeatures();
            CleanFeatures();
            DrawFeatures();

            if (Preferences.Get("ShowLogPathOnMap", true))
            {
                //Setup log
                cts = new CancellationTokenSource();
                _ = MapLogUpdaterAsync(new TimeSpan(0, 0, 1), cts.Token);
            }
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

        private async Task MapLogUpdaterAsync(TimeSpan interval, CancellationToken ct)
        {
            while (true)
            {
                List<Point> logFile = App.LogStore.GetLogFileObject();
                Polyline logPolyline = new Polyline
                {
                    StrokeColor = Color.DarkOrange,
                    StrokeWidth = 3,
                };
                logFile.ForEach((Point point) =>
                {
                    logPolyline.Geopath.Add(new Position(point.Latitude, point.Longitude));
                });
                Map.MapElements.Add(logPolyline);
                await Task.Delay(interval, ct);
            }
        }

        public void CleanupLog()
        {
            if (cts != null)
            {
                cts.Cancel();
            }
        }

        void OnMapClicked(object sender, MapClickedEventArgs e)
        {
            FeatureList.ForEach(async (Feature feature) =>
            {
                bool ItemHit = false;
                Point[] points = feature.properties.xamarincoordinates.ToArray();
                if (feature.geometry.type == FeatureType.Polygon)
                {
                    ItemHit |= IsPointInPolygon(new Point(e.Position.Latitude, e.Position.Longitude, 0), points);
                }
                else if (feature.geometry.type == FeatureType.LineString)
                {
                    ItemHit |= IsPointOnLine(new Point(e.Position.Latitude, e.Position.Longitude, 0), points);
                }

                if (ItemHit)
                {
                    await DisplayFeatureActionMenuAsync(feature);
                }
            });
        }

        async Task DisplayFeatureActionMenuAsync(Feature feature)
        {
            string result = await NavigationService.GetCurrentPage().DisplayActionSheet(feature.properties.name, "Dismiss", "Delete", "View", "Edit");

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

        public bool IsPointInPolygon(Point p, Point[] polygon)
        {
            double minX = polygon[0].Longitude;
            double maxX = polygon[0].Longitude;
            double minY = polygon[0].Latitude;
            double maxY = polygon[0].Latitude;
            for (int i = 1; i < polygon.Length; i++)
            {
                Point q = polygon[i];
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
            for (int i = 0, j = polygon.Length - 1; i < polygon.Length; j = i++)
            {
                if ((polygon[i].Latitude > p.Latitude) != (polygon[j].Latitude > p.Latitude) &&
                     p.Longitude < (polygon[j].Longitude - polygon[i].Longitude) * (p.Latitude - polygon[i].Latitude) / (polygon[j].Latitude - polygon[i].Latitude) + polygon[i].Longitude)
                {
                    inside = !inside;
                }
            }
            return inside;
        }

        //currently only works on line vertices
        public bool IsPointOnLine(Point LocationTapped, Point[] polyline)
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
            double delta = 0.0001; // delta determines line tap accuracy

            for (int i = 1; i < polyline.Length; i++)
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
