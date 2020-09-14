using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Point = Groundsman.Models.Point;

namespace Groundsman.Services
{
    public class LogStore
    {
        private CancellationTokenSource cts;
        public string LogString { get; set; }
        public List<Point> LogPoints { get; set; }
        private readonly string CSVHeader = "Time, Latitude, Longitude, Altitude\n";

        public LogStore()
        {
            LogPoints = new List<Point>();
            if (File.Exists(AppConstants.LOG_FILE))
            {
                LogString = File.ReadAllText(AppConstants.LOG_FILE);
                foreach (string pointString in File.ReadAllLines(AppConstants.LOG_FILE))
                {
                    string[] stringArray = pointString.Split(",");
                    try
                    {
                        Point point = new Point(double.Parse(stringArray[1]), double.Parse(stringArray[2]), double.Parse(stringArray[3]));
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
            File.WriteAllText(AppConstants.LOG_FILE, LogString);
            await Share.RequestAsync(new ShareFileRequest
            {
                Title = "Groundsman Logfile",
                File = new ShareFile(AppConstants.LOG_FILE, "text/csv"),
                PresentationSourceBounds = DeviceInfo.Platform == DevicePlatform.iOS && DeviceInfo.Idiom == DeviceIdiom.Tablet
                    ? new System.Drawing.Rectangle((int)(DeviceDisplay.MainDisplayInfo.Width * .474), 80, 0, 0)
                    : System.Drawing.Rectangle.Empty
            });
        }

        public void ClearLog()
        {
            LogString = CSVHeader;
            LogPoints = new List<Point>();
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
                Point location = await HelperServices.GetGeoLocation();
                if (location != null)
                {
                    string newEntry = string.Format("{0}, {1}, {2}, {3}\n", DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"), location.Latitude, location.Longitude, location.Altitude);
                    LogString += newEntry;
                    LogPoints.Add(new Point(location.Latitude, location.Longitude, location.Altitude));
                    File.WriteAllText(AppConstants.LOG_FILE, LogString);
                    MessagingCenter.Send(this, "LogUpdated");
                }
            }
        }
    }
}
