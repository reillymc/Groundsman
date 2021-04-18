using System;
using System.Threading;
using System.Threading.Tasks;
using Groundsman.Misc;
using Groundsman.Models;
using Xamarin.Forms;

namespace Groundsman.Services
{
    public class LogService
    {
        readonly bool stopping = false;

        public LogService() { }

        public async Task Run(CancellationToken token, int interval)
        {
            await Task.Run(async () =>
            {
                while (!stopping)
                {
                    token.ThrowIfCancellationRequested();
                    try
                    {
                        await Task.Delay(interval);
                        Position location = await HelperServices.GetGeoLocation();
                        DisplayPosition message = new DisplayPosition(DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"), location.Longitude.ToString(), location.Latitude.ToString(), location.Altitude.ToString());
                        MessagingCenter.Send<DisplayPosition>(message, "Location");
                    }
                    catch
                    {
                        MessagingCenter.Send<LocationErrorMessage>(new LocationErrorMessage(), "LocationError");
                    }
                }
            }, token);
        }
    }
}
