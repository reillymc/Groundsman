﻿using Groundsman.Models;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace Groundsman.Services
{
    public class LogStore
    {
        public List<Point> GetLogFileObject()
        {
            // Casts to doubles without error handeling currently
            if (File.Exists(AppConstants.LOG_FILE))
            {
                List<Point> logPoints = new List<Point>();
                string[] logList = File.ReadAllLines(AppConstants.LOG_FILE);
                foreach (string pointString in logList)
                {
                    string[] stringArray = pointString.Split(",");
                    Point point = new Point(double.Parse(stringArray[1]), double.Parse(stringArray[2]), double.Parse(stringArray[3]));
                    logPoints.Add(point);
                }
                return logPoints;
            }
            else
            {
                return null;
            }
        }

        public async Task ExportLogFile()
        {
            await Share.RequestAsync(new ShareFileRequest
            {
                Title = "Groundsman Logfile",
                File = new ShareFile(AppConstants.LOG_FILE, "text/csv"),
                PresentationSourceBounds = DeviceInfo.Platform == DevicePlatform.iOS && DeviceInfo.Idiom == DeviceIdiom.Tablet
                        ? new System.Drawing.Rectangle((int)(DeviceDisplay.MainDisplayInfo.Width * .474), 80, 0, 0)
                        : System.Drawing.Rectangle.Empty
            });
        }
    }
}