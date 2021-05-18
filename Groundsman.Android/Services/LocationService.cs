using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Groundsman.Droid.Helpers;
using Groundsman.Misc;
using Groundsman.Services;
using Xamarin.Forms;

namespace Groundsman.Droid.Services
{
    [Service]
    public class AndroidLocationService : Service
    {
        private CancellationTokenSource _cts;
        public const int SERVICE_RUNNING_NOTIFICATION_ID = 10000;

        public override IBinder OnBind(Intent intent) => null;

        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {
            _cts = new CancellationTokenSource();

            Notification notif = DependencyService.Get<INotification>().ReturnNotif();
            StartForeground(SERVICE_RUNNING_NOTIFICATION_ID, notif);

            Task.Run(() =>
            {
                try
                {
                    LogService locShared = new LogService();
                    locShared.Run(_cts.Token, intent.Extras.GetInt("Interval")).Wait();
                }
                catch (OperationCanceledException)
                {
                }
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
            }, _cts.Token);

            return StartCommandResult.Sticky;
        }

        public override void OnDestroy()
        {
            if (_cts != null)
            {
                _cts.Token.ThrowIfCancellationRequested();
                _cts.Cancel();
            }

            base.OnDestroy();
        }
    }
}