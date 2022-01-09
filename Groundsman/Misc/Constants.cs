using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Groundsman.Models;
using Xamarin.Essentials;

namespace Groundsman
{
    public static class Constants
    {
        // Files paths and names
        private static readonly string DATA_PATH = FileSystem.AppDataDirectory;
        private static readonly string CACHE_PATH = FileSystem.CacheDirectory;

        private const string FEATURES_FILENAME = "locations.json";
        private const string DELETED_FEATURE_FILENAME = "deleted.json";
        private const string EXPORT_LOG_FILENAME = "Groundsman Log.csv";

        public static readonly string FEATURES_FILE = Path.Combine(DATA_PATH, FEATURES_FILENAME);
        public static readonly string DELETED_FEATURE_FILE = Path.Combine(CACHE_PATH, DELETED_FEATURE_FILENAME);
        public static readonly string EXPORT_LOG_FILE = Path.Combine(CACHE_PATH, EXPORT_LOG_FILENAME);

        // Preference keys
        public const string UserIDKey = "UserID";
        public const string GPSPrecisionKey = "GPSPrecision";
        public const string DecimalAccuracyKey = "DecimalAccuracy";
        public const string ListOrderingKey = "ListOrdering";
        public const string ShakeToUndoKey = "EnableShakeToUndo"; 
        public const string MapPreviewKey = "MapPreview";
        public const string MapDrawPointsKey = "ShowPointsOnMap";
        public const string MapDrawLinesKey = "ShowLinesOnMap";
        public const string MapDrawPolygonsKey = "ShowPolygonsOnMap";
        public const string LoggerExportFormatKey = "LoggerExportFormat";


        // Hardcoded feature property keys
        public const string IdentifierProperty = "id";
        public const string NameProperty = "name";
        public const string DateProperty = "date";
        public const string AuthorProperty = "author";
        public const string LogTimestampsProperty = "timestamps";

        // Feature property values
        public const string NewFeatureID = "-1";
        public const string DefaultUserValue = "Groundsman";
        public const int DefaultGPSPrecisionValue = 2;
        public const int DefaultDecimalAccuracyValue = 6;
        public const int DefaultListOrderingValue = 0;
        public const int LoggerExportFormatDefaultValue = 0;

        /// <summary>
        /// Fetches the local features list file if it exists, otherwise the default feature list
        /// </summary>
        /// <returns>Contents of file (serialised GeoJSON)</returns>
        public static string FeaturesFileContents => File.Exists(FEATURES_FILE) ? File.ReadAllText(FEATURES_FILE) : null;

        /// <summary>
        /// Gives the correct export file name and extension
        /// </summary>
        /// <param name="fileName">Name of the export file</param>
        /// <param name="type">the filetype to export to</param>
        /// <returns>Full export file path string</returns>
        public static string GetExportFile(string fileName, ExportType type) => type switch
        {
            ExportType.GeoJSON => Path.Combine(CACHE_PATH, fileName + ".json"),
            ExportType.CSV => Path.Combine(CACHE_PATH, fileName + ".csv"),
            _ => Path.Combine(CACHE_PATH, fileName),
        };

        public static bool FirstRun
        {
            get => Preferences.Get(nameof(FirstRun), true);
            set => Preferences.Set(nameof(FirstRun), value);
        }

        public static List<Feature> DefaultFeatures = new List<Feature> {
             new Feature(
                 new Polygon(
                     new List <LinearRing> {
                         new LinearRing(
                             new List < Position > {
                                 new Position(153.027449, -27.474751),
                                 new Position(153.031375, -27.477283),
                                 new Position(153.030560, -27.472276),
                                 new Position(153.027449, -27.474751),
                             }
                         ),
                     }
                 ),
                 new Dictionary <string, object> () {
                     {
                         "name",
                         "Example Polygon"
                     }, {
                         "author",
                         "Groundsman"
                     }, {
                         "String Property",
                         "This is an example polygon"
                     }, {
                         "Integer Property",
                         1
                     }, {
                         "Float Property",
                         1.1
                     },
                 }
             ),
             new Feature(new LineString(new List <Position> {
                     new Position(153.031504, -27.477759),
                     new Position(153.029680, -27.479339),
                 }),
                 new Dictionary <string, object> () {
                     {
                         "name",
                         "Test Line"
                     }, {
                         "author",
                         "Groundsman"
                     }, {
                         "String Property",
                         "This is a test line"
                     }, {
                         "Integer Property",
                         2
                     }, {
                         "Float Property",
                         2.2
                     },
                 }
             ),
             new Feature(new Point(new Position(153.028307, -27.477188)), new Dictionary <string, object> () {
                 {
                     "name",
                     "Sample Point"
                 }, {
                     "author",
                     "Groundsman"
                 }, {
                     "String Property",
                     "This is a sample point"
                 }, {
                     "Integer Property",
                     3
                 }, {
                     "Float Property",
                     3.3
                 },
             }),
         };
    }
}