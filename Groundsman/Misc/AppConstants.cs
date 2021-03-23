using System.IO;
using Xamarin.Essentials;

namespace Groundsman
{
    public static class AppConstants
    {
        private static readonly string DATA_PATH = FileSystem.AppDataDirectory;
        private static readonly string CACHE_PATH = FileSystem.CacheDirectory;

        private static readonly string FEATURES_FILENAME = "locations.json";
        private static readonly string LOG_FILENAME = "log.csv";

        public static readonly string NEW_ENTRY_ID = "-1";
        public static readonly string FEATURES_FILE = Path.Combine(DATA_PATH, FEATURES_FILENAME);
        public static readonly string LOG_FILE = Path.Combine(DATA_PATH, LOG_FILENAME);

        public static string GetExportFile(string fileName)
        {
            return Path.Combine(CACHE_PATH, fileName + ".json");
        }
    }
}
