using System.Linq;
using Groundsman.Helpers;
using Newtonsoft.Json;

namespace Groundsman.Models;

/// <summary>
/// A geometry object where the "coordinates" member is an array of two or more positions
/// </summary>
[JsonConverter(typeof(DummyConverter))]
public class LineString : Geometry
{
    [JsonProperty(PropertyName = "coordinates", Order = 2)]
    public IEnumerable<Position> Coordinates { get; set; }

    public LineString(IEnumerable<Position> coordinates) : base(GeoJSONType.LineString)
    {
        if (coordinates == null)
        {
            throw new ArgumentNullException(nameof(coordinates), "A LineString must have coordinates.");
        }

        if (coordinates.Count() < 2)
        {
            throw new ArgumentException("A LineString must have two or more positions.", nameof(coordinates));
        }

        Coordinates = coordinates;
    }

    /// <summary>
    /// Checks if a given point lies on the LineString
    /// </summary>
    /// <param name="queryPosition"></param>
    /// <returns>True if point lies on the LineString, otherwise false</returns>
    public bool ContainsPosition(Position queryPosition)
    {
        double Lat1;
        double Lat2;
        double Lon1;
        double Lon2;
        double PointLat;
        double PointLon;
        double AB;
        double AP;
        double PB;
        double delta = 0.0001; // Delta determines line tap accuracy

        List<Position> polyline = (List<Position>)Coordinates;

        // Check for each individual line segment
        for (int i = 1; i < polyline.Count; i++)
        {
            Lat1 = polyline[i - 1].Latitude;
            Lat2 = polyline[i].Latitude;

            Lon1 = polyline[i - 1].Longitude;
            Lon2 = polyline[i].Longitude;

            PointLat = queryPosition.Latitude;
            PointLon = queryPosition.Longitude;

            AB = Math.Sqrt((Lat2 - Lat1) * (Lat2 - Lat1) + (Lon2 - Lon1) * (Lon2 - Lon1));
            AP = Math.Sqrt((PointLat - Lat1) * (PointLat - Lat1) + (PointLon - Lon1) * (PointLon - Lon1));
            PB = Math.Sqrt((Lat2 - PointLat) * (Lat2 - PointLat) + (Lon2 - PointLon) * (Lon2 - PointLon));

            // Check if position is between two points of a line within distance of delta from the line
            if (Math.Abs(AB - (AP + PB)) < delta)
                return true;
        }
        return false;
    }

    public Position GetCenterPosition()
    {
        List<Position> polyline = (List<Position>)Coordinates;
        Position centerPosition = new Position(0, 0);

        if (polyline.Count > 0)
        {
            double avgLat = 0;
            double avgLng = 0;

            foreach (var point in polyline)
            {
                avgLat += point.Latitude;
                avgLng += point.Longitude;
            }

            avgLat /= polyline.Count();
            avgLng /= polyline.Count();

            centerPosition = new Position(avgLng, avgLat);
        }
        return centerPosition;
    }

    public Position GetSpan()
    {
        List<Position> polyline = (List<Position>)Coordinates;
        Position spanPosition = new Position(0, 0);

        if (polyline.Count > 0)
        {
            double minLng = 180;
            double maxLng = -180;
            double minLat = 90;
            double maxLat = -90;

            foreach (var point in polyline)
            {
                minLng = Math.Min(point.Longitude, minLng);
                maxLng = Math.Max(point.Longitude, maxLng);
                minLat = Math.Min(point.Latitude, minLat);
                maxLat = Math.Max(point.Latitude, maxLat);
            }

            spanPosition = new Position((maxLng - minLng) * 1.6, (maxLat - minLat) * 1.6);
        }
        return spanPosition;
    }
}
