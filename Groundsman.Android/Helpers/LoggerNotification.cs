using Android.App;
using Android.Content;
using Android.OS;
using AndroidX.Core.App;
using Groundsman.Droid;
using Groundsman.Droid.Helpers;
using XamarinForms.LocationService.Droid.Helpers;

[assembly: Xamarin.Forms.Dependency(typeof(LoggerNotification))]
namespace XamarinForms.LocationService.Droid.Helpers;

internal class LoggerNotification : INotification
{
    private static readonly string foregroundChannelId = "9001";
    private static readonly Context context = global::Android.App.Application.Context;


    public Notification ReturnNotif()
    {
        Intent intent = new Intent(context, typeof(MainActivity));
        intent.AddFlags(ActivityFlags.SingleTop);
        intent.PutExtra("Groundsman Logger", "Now recording a log of your positions.");

        PendingIntent pendingIntent = PendingIntent.GetActivity(context, 0, intent, PendingIntentFlags.UpdateCurrent);

        NotificationCompat.Builder notifBuilder = new NotificationCompat.Builder(context, foregroundChannelId)
            .SetContentTitle("Groundsman Logger")
            .SetContentText("Now recording a log of your positions.")
            .SetSmallIcon(Resource.Mipmap.ic_launcher_foreground)
            .SetOngoing(true)
            .SetContentIntent(pendingIntent);

        if (global::Android.OS.Build.VERSION.SdkInt >= BuildVersionCodes.O)
        {
            NotificationChannel notificationChannel = new NotificationChannel(foregroundChannelId, "Groundsman", NotificationImportance.Low)
            {
                Importance = NotificationImportance.High
            };
            notificationChannel.EnableLights(true);
            notificationChannel.EnableVibration(true);
            notificationChannel.SetShowBadge(true);
            notificationChannel.SetVibrationPattern(new long[] { 100, 200, 300 });

            NotificationManager notifManager = context.GetSystemService(Context.NotificationService) as NotificationManager;
            if (notifManager != null)
            {
                notifBuilder.SetChannelId(foregroundChannelId);
                notifManager.CreateNotificationChannel(notificationChannel);
            }
        }

        return notifBuilder.Build();
    }
}
