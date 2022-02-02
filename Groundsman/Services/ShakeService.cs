using Xamarin.Essentials;

namespace Groundsman.Services;

public class ShakeService
{
    private readonly App Current;

    // Set speed delay for monitoring changes.
    private readonly SensorSpeed speed = SensorSpeed.Game;

    public ShakeService(App app)
    {
        Current = app;
        // Register for reading changes, be sure to unsubscribe when finished
        Accelerometer.ShakeDetected += Accelerometer_ShakeDetected;
    }

    private void Accelerometer_ShakeDetected(object sender, EventArgs e)
    {
        Stop();
        try
        {
            // Use default vibration length
            Vibration.Vibrate();
        }
        catch
        {
            // Other error has occurred.
        }
        Current.UndoDelete();
    }

    public void Stop()
    {
        Accelerometer.ReadingChanged -= Accelerometer_ShakeDetected;
        if (Accelerometer.IsMonitoring)
        {
            Accelerometer.Stop();
        }
    }

    public void Start()
    {
        if (Preferences.Get(Constants.ShakeToUndoKey, true))
        {
            try
            {
                if (!Accelerometer.IsMonitoring)
                {
                    Accelerometer.Start(speed);
                }
            }
            catch
            {
                // Other error has occurred.
            }
        }
    }
}
