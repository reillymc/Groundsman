using System;
using System.Threading;
using System.Threading.Tasks;
using Groundsman.Misc;
using Groundsman.Services;
using UIKit;
using Xamarin.Forms;

namespace Groundsman.iOS.Services;

public class LocationService
{
    private nint _taskId;
    private CancellationTokenSource _cts;

    public async Task Start(int interval)
    {
        _cts = new CancellationTokenSource();
        _taskId = UIApplication.SharedApplication.BeginBackgroundTask("com.qutgeodev.groundsman", OnExpiration);
        try
        {
            LogService locShared = new LogService();
            await locShared.Run(_cts.Token, interval);
        }
        catch (OperationCanceledException) { }
        finally
        {
            if (_cts.IsCancellationRequested)
            {
                StopServiceMessage message = new StopServiceMessage();
                Device.BeginInvokeOnMainThread(
                    () => MessagingCenter.Send(message, "ServiceStopped")
                );
            }
        }

        double time = UIApplication.SharedApplication.BackgroundTimeRemaining;

        UIApplication.SharedApplication.EndBackgroundTask(_taskId);
    }

    public void Stop() => _cts.Cancel();

    private void OnExpiration() => UIApplication.SharedApplication.EndBackgroundTask(_taskId);
}
