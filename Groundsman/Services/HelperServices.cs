using System;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Position = Groundsman.Models.Position;

namespace Groundsman.Services
{
    public class HelperServices
    {
        private static GeolocationAccuracy geolocationAccuracy;
        private static Position point;
        private static int decimalAccuracy;

        /// <summary>
        /// Queries the current location of the device
        /// </summary>
        /// <returns>A point object containing the device's current location</returns>
        public static async Task<Position> GetGeoLocation()
        {
            geolocationAccuracy = Preferences.Get("GPSPrecision", 2) switch
            {
                0 => GeolocationAccuracy.Best,
                1 => GeolocationAccuracy.High,
                3 => GeolocationAccuracy.Low,
                _ => GeolocationAccuracy.Medium,
            };
            decimalAccuracy = Preferences.Get("DataDecimalAccuracy", 8);
            try
            {
                // Gets current location of device (MORE ACCURATE, but slower)
                var request = new GeolocationRequest(geolocationAccuracy);
                var location = await Geolocation.GetLocationAsync(request);
                if (location != null)
                {
                    point = new Position(Math.Round(location.Longitude, decimalAccuracy), Math.Round(location.Latitude, decimalAccuracy), Math.Round(location.Altitude ?? 0.0, decimalAccuracy));
                    return point;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception)
            {
                await Application.Current.MainPage.DisplayAlert("Geolocation Error", "Location permissions for Groundsman must be enabled to fetch location", "Ok");
                return null;
            }
        }

        /// <summary>
        /// Method to check persmission status of given permission
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="permission">Permission type</param>
        /// <returns>Permission status</returns>
        public static async Task<PermissionStatus> CheckAndRequestPermissionAsync<T>(T permission)
            where T : Permissions.BasePermission
        {
            var status = await permission.CheckStatusAsync();
            if (status != PermissionStatus.Granted)
            {
                status = await permission.RequestAsync();
            }

            return status;
        }
    }
}
