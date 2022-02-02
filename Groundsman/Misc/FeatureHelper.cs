using Groundsman.Models;
using System.Linq;

namespace Groundsman.Misc;

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
    /// Exports a feature
    /// </summary>
    /// <param name="item">Feature to export</param>
    /// <returns>Export file path</returns>
    public static async Task<string> ExportFeatures(Feature item)
    {
        string fileName = Constants.GetExportFile(item.Name, ExportType.GeoJSON);
        await SaveFeaturesToFile(GeoJSONObject.ExportGeoJSON(item), fileName);
        return fileName;
    }

    /// <summary>
    /// Exports a log feature
    /// </summary>
    /// <param name="item">Feature to export</param>
    /// <returns>Export file path</returns>
    public static async Task<string> ExportLog(Feature item)
    {
        var log = (LineString)item.Geometry;
        string logString = "Timestamp, Longitude, Latitude, Altitude\n";
        foreach (Position position in log.Coordinates.Reverse())
        {
            logString += $"{position}\n";
        }

        string fileName = Constants.GetExportFile(item.Name, ExportType.CSV);
        await SaveFeaturesToFile(logString, fileName);
        return fileName;
    }

    /// <summary>
    /// Saves a feature to file
    /// </summary>
    /// <param name="item">Feature to save</param>
    /// <param name="fileName">File name to save to</param>
    private static async Task SaveFeaturesToFile(string contents, string fileName)
    {
        using StreamWriter writer = new StreamWriter(File.Create(fileName));
        await writer.WriteAsync(contents);
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


    public static Geometry GetGeometry(IList<DisplayPosition> Positions, GeoJSONType GeometryType)
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

    public static Dictionary<string, object> GetProperties(IList<DisplayProperty> Properties)
    {
        var FinalProperties = new Dictionary<string, object>();
        foreach (DisplayProperty property in Properties)
        {
            if (!string.IsNullOrEmpty(property.Key.ToString()))
            {
                FinalProperties[property.Key] = property.ToValueObject();
            }
        }
        return FinalProperties;
    }
}
