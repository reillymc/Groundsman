using Groundsman.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Groundsman.Misc
{
    public static class FeatureHelper
    {

        /// <summary>
        /// Exports features to a sharefile request
        /// </summary>
        /// <param name="items">IEnumerable of features to export</param>
        /// <returns>Export file path</returns>
        public static async Task<string> ExportFeatures(IEnumerable<Feature> items)
        {
            string fileName = Constants.GetExportFile("Groundsman Feature Collection", ExportType.GeoJSON);
            await SaveFeaturesToFile(items, fileName);
            return fileName;
        }

        /// <summary>
        /// Exports a feature to a sharefile request
        /// </summary>
        /// <param name="item">Feature to export</param>
        /// <returns>Export file path</returns>
        public static async Task<string> ExportFeatures(Feature item)
        {
            string fileName = Constants.GetExportFile(item.Name, ExportType.GeoJSON);
            await SaveFeaturesToFile(item, fileName);
            return fileName;
        }

        /// <summary>
        /// Saves a feature to file
        /// </summary>
        /// <param name="item">Feature to save</param>
        /// <param name="fileName">File name to save to</param>
        private static async Task SaveFeaturesToFile(Feature item, string fileName)
        {
            using StreamWriter writer = new StreamWriter(File.Create(fileName));
            await writer.WriteAsync(GeoJSONObject.ExportGeoJSON(item, true));
        }

        /// <summary>
        /// Saves a feature list to file
        /// </summary>
        /// <param name="item">IEnumearble of seatures to save</param>
        /// <param name="FileName">File name to save to</param>
        private static async Task SaveFeaturesToFile(IEnumerable<Feature> items, string FileName)
        {
            FeatureCollection geoJSONObject = new FeatureCollection(items);
            using StreamWriter writer = new StreamWriter(File.Create(FileName));
            await writer.WriteAsync(GeoJSONObject.ExportGeoJSON(geoJSONObject));
        }


        public static Geometry GetValidatedGeometry(IList<DisplayPosition> Positions, GeoJSONType GeometryType)
        {
            switch (GeometryType)
            {
                case GeoJSONType.Point:
                    return new Point(new Position(Positions[0]));
                case GeoJSONType.LineString:
                    return new LineString(Positions.Select(pointValue => new Position(pointValue)).ToList());
                case GeoJSONType.Polygon:
                    // This method does not allow for creating a polygon with multiple LinearRings
                    List<Position> positions = Positions.Select(pointValue => new Position(pointValue)).ToList();
                    // Close polygon with duplicated first feature
                    positions.Add(positions[0]);
                    return new Polygon(new List<LinearRing>() { new LinearRing(positions) });
                default:
                    throw new ArgumentException($"Could not save unsupported feature of type {GeometryType}", GeometryType.ToString());
            }
        }
    }
}
