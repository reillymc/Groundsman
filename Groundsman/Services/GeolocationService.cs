using System;
using System.Threading.Tasks;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;
using Xamarin.Essentials;

namespace Groundsman.Services
{
    public class GeolocationService
    {
        private static GeolocationAccuracy geolocationAccuracy;
        private static Point point;

        /// <summary>
        /// Queries the current device's location coordinates
        /// </summary>
        public static async Task<Point> GetGeoLocation()
        {
            geolocationAccuracy = Preferences.Get("GPSPrecision", 2) switch
            {
                0 => GeolocationAccuracy.Best,
                1 => GeolocationAccuracy.High,
                3 => GeolocationAccuracy.Low,
                _ => GeolocationAccuracy.Medium,
            };
            try
            {
                var status = await CrossPermissions.Current.CheckPermissionStatusAsync(Permission.Location);
                if (status == PermissionStatus.Granted)
                {
                    // Gets current location of device (MORE ACCURATE, but slower)
                    var request = new GeolocationRequest(geolocationAccuracy);
                    var location = await Geolocation.GetLocationAsync(request);
                    if (location != null)
                    {
                        point = new Point(location.Latitude, location.Longitude, location.Altitude ?? 0.0);
                        return point;
                    }
                }
                else
                {
                    await HomePage.Instance.DisplayAlert("Permissions Error", "Location permissions for Groundsman must be enabled to fetch location.", "Ok");
                    return null;
                }
            }
            catch (Exception)
            {
                await HomePage.Instance.DisplayAlert("Geolocation Error", "Location permissions for Groundsman must be enabled to fetch location", "Ok");
                throw new Exception();
            }
            return null;
        }
    }
}
