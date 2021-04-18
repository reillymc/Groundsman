using System;
using System.Threading;
using System.Threading.Tasks;
using Groundsman.Misc;
using Groundsman.Services;
using UIKit;
using Xamarin.Forms;

namespace Groundsman.iOS.Services
{
    public class LocationService
    {
        nint _taskId;
        CancellationTokenSource _cts;

        public async Task Start(int interval)
        {
            _cts = new CancellationTokenSource();
            _taskId = UIApplication.SharedApplication.BeginBackgroundTask("com.qutgeodev.groundsman", OnExpiration);
            try
            {
                var locShared = new LogService();
                await locShared.Run(_cts.Token, interval);
            }
            catch (OperationCanceledException) { }
            finally
            {
                if (_cts.IsCancellationRequested)
                {
                    var message = new StopServiceMessage();
                    Device.BeginInvokeOnMainThread(
                        () => MessagingCenter.Send(message, "ServiceStopped")
                    );
                }
            }

            var time = UIApplication.SharedApplication.BackgroundTimeRemaining;

            UIApplication.SharedApplication.EndBackgroundTask(_taskId);
        }

        public void Stop()
        {
            _cts.Cancel();
        }

        void OnExpiration()
        {
            UIApplication.SharedApplication.EndBackgroundTask(_taskId);
        }
    }
}