using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace Groundsman.Data
{
    public class LogStore
    {

        public string GetLogFile()
        {
            // Attempt to open the embedded file on the device. 
            // If it exists return it, else create a new embedded file from a json source file.
            if (File.Exists(AppConstants.LOG_FILE))
            {
                return File.ReadAllText(AppConstants.LOG_FILE);
            }
            else
            {
                return "";
            }
        }

        public List<Point> GetLogFileObject()
        {
            // Attempt to open the embedded file on the device. 
            // If it exists return it, else create a new embedded file from a json source file.
            if (File.Exists(AppConstants.LOG_FILE))
            {
                List<Point> logList = File.ReadAllLines(AppConstants.LOG_FILE).Select(x => new Point
                (
                    x[1],
                    x[2],
                    x[3]
                )).ToList();
                return logList;
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
                File = new ShareFile(AppConstants.LOG_FILE, "text/csv")
            });
        }
    }
}
