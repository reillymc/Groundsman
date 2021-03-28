using System;
using Xamarin.Essentials;

namespace Groundsman.Services
{
    public class ShakeService
    {
        App Current;
        // Set speed delay for monitoring changes.
        SensorSpeed speed = SensorSpeed.Game;

        public ShakeService(App app)
        {
            Current = app;
            // Register for reading changes, be sure to unsubscribe when finished
            Accelerometer.ShakeDetected += Accelerometer_ShakeDetected;
        }

        private void Accelerometer_ShakeDetected(object sender, EventArgs e)
        {
            Stop();
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
            if (Preferences.Get("EnableShakeToUndo", true))
            {
                try
                {
                    if (!Accelerometer.IsMonitoring)
                    {
                        Accelerometer.Start(speed);
                    }
                }
                catch (Exception ex)
                {
                    // Other error has occurred.
                }
            }
        }
    }
}
