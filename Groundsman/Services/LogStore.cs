using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Position = Groundsman.Models.Position;

namespace Groundsman.Services
{
    public class LogStore
    {
        private CancellationTokenSource cts;
        public string LogString { get; set; }
        public List<Position> LogPoints { get; set; }
        private readonly string CSVHeader = "Time, Latitude, Longitude, Altitude\n";

        public LogStore()
        {
            LogPoints = new List<Position>();
            if (File.Exists(AppConstants.LOG_FILE))
            {
                LogString = File.ReadAllText(AppConstants.LOG_FILE);
                foreach (string pointString in File.ReadAllLines(AppConstants.LOG_FILE))
                {
                    string[] stringArray = pointString.Split(",");
                    try
                    {
                        Position point = new Position(double.Parse(stringArray[1]), double.Parse(stringArray[2]), double.Parse(stringArray[3]));
                        LogPoints.Add(point);
                    }
                    catch { /*TODO*/ }
                }
            }
            else
            {
                LogString = CSVHeader;
            }
        }

        public async Task ExportLogFile()
        {
            File.WriteAllText(AppConstants.EXPORT_LOG_FILE, LogString);
            await Share.RequestAsync(new ShareFileRequest
            {
                Title = "Groundsman Log",
                File = new ShareFile(AppConstants.EXPORT_LOG_FILE, "text/csv"),
                PresentationSourceBounds = DeviceInfo.Platform == DevicePlatform.iOS && DeviceInfo.Idiom == DeviceIdiom.Tablet
                    ? new System.Drawing.Rectangle((int)(DeviceDisplay.MainDisplayInfo.Width * .474), 80, 0, 0)
                    : System.Drawing.Rectangle.Empty
            });
        }

        public void ClearLog()
        {
            LogString = CSVHeader;
            LogPoints = new List<Position>();
            File.WriteAllText(AppConstants.LOG_FILE, LogString);
            MessagingCenter.Send(this, "LogUpdated");
        }

        public void StartLogging(int Interval)
        {
            cts = new CancellationTokenSource();
            _ = UpdaterAsync(new TimeSpan(0, 0, Interval), cts.Token);
        }

        public void StopLogging()
        {
            cts.Cancel();
            cts.Dispose();
        }

        private async Task UpdaterAsync(TimeSpan interval, CancellationToken ct)
        {
            while (true)
            {
                await Task.Delay(interval, ct);
                Position location = await HelperServices.GetGeoLocation();
                if (location != null)
                {
                    string newEntry = string.Format("{0}, {1}\n", DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"), location.ToString());
                    LogString += newEntry;
                    LogPoints.Add(location);
                    File.WriteAllText(AppConstants.LOG_FILE, LogString);
                    MessagingCenter.Send(this, "LogUpdated");
                }
            }
        }
    }
}
