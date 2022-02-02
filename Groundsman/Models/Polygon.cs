using System.Linq;
using Groundsman.JSONConverters;
using Newtonsoft.Json;

namespace Groundsman.Models;

/// <summary>
/// A geometry object where the "coordinates" member is a list of closed LineStrings
/// </summary>
[JsonConverter(typeof(PolygonConverter))]
public class Polygon : Geometry
{
    [JsonProperty(PropertyName = "coordinates", Order = 2)]
    public IEnumerable<LinearRing> Coordinates { get; set; }

    public Polygon(IEnumerable<LinearRing> coordinates) : base(GeoJSONType.Polygon)
    {
        if (coordinates == null)
        {
            throw new ArgumentNullException(nameof(coordinates), "A Polygon must have coordinates.");
        }

        if (coordinates.Count() <= 0)
        {
            throw new ArgumentException("A Polygon must have one or more linear rings.", nameof(coordinates));
        }

        Coordinates = coordinates;
    }

    /// <summary>
    /// Check if a given position is contained within the polygon's area. With one LinearRing the point must simply be in the bounds, with multiple overlapping rings the point can be excluded from being in the polygon area by being inside a contained LinearRing 
    /// </summary>
    /// <param name="queryPosition">Position to check is in polygon</param>
    /// <returns>True if point is contained within the polygon, otherwise false</returns>
    public bool ContainsPosition(Position queryPosition)
    {
        bool inside = false;

        // Operate for each LR in polygon. If contained in the ring it will invert the inside bool variable. Its inverted becasue this way this method will support multiple overlapping LRs. E.g. In LR 1 = true, in LR 1 and in LR 2 = false.
        foreach (LinearRing linearRing in Coordinates)
        {
            List<Position> coords = (List<Position>)linearRing.Coordinates;
            double minX = coords[0].Longitude;
            double maxX = coords[0].Longitude;
            double minY = coords[0].Latitude;
            double maxY = coords[0].Latitude;

            // Get the absolute max bounds
            foreach (Position positison in linearRing.Coordinates)
            {
                minX = Math.Min(positison.Longitude, minX);
                maxX = Math.Max(positison.Longitude, maxX);
                minY = Math.Min(positison.Latitude, minY);
                maxY = Math.Max(positison.Latitude, maxY);
            }

            // Check if pos is within these max bounds, otherwise no point in checking for finer boundaries
            if (queryPosition.Longitude >= minX && queryPosition.Longitude <= maxX && queryPosition.Latitude >= minY && queryPosition.Latitude <= maxY)
            {
                // http://www.ecse.rpi.edu/Homepages/wrf/Research/Short_Notes/pnpoly.html
                for (int i = 0, j = coords.Count - 1; i < coords.Count; j = i++)
                {
                    if ((coords[i].Latitude > queryPosition.Latitude) != (coords[j].Latitude > queryPosition.Latitude) &&
                         queryPosition.Longitude < (coords[j].Longitude - coords[i].Longitude) * (queryPosition.Latitude - coords[i].Latitude) / (coords[j].Latitude - coords[i].Latitude) + coords[i].Longitude)
                    {
                        inside = !inside;
                    }
                }
            }
        }
        return inside;
    }

    public Position GetCenterPosition()
    {
        double minLng = 180;
        double maxLng = -180;
        double minLat = 90;
        double maxLat = -90;

        foreach (LinearRing linearRing in Coordinates)
        {
            List<Position> coords = (List<Position>)linearRing.Coordinates;

            // Get the absolute max bounds
            foreach (Position positison in linearRing.Coordinates)
            {
                minLng = Math.Min(positison.Longitude, minLng);
                maxLng = Math.Max(positison.Longitude, maxLng);
                minLat = Math.Min(positison.Latitude, minLat);
                maxLat = Math.Max(positison.Latitude, maxLat);
            }
        }

        Position centerPosition = new Position((maxLng + minLng) / 2, (maxLat + minLat) / 2);
        return centerPosition;
    }

    public Position GetSpan()
    {
        double minLng = 180;
        double maxLng = -180;
        double minLat = 90;
        double maxLat = -90;

        foreach (LinearRing linearRing in Coordinates)
        {
            List<Position> coords = (List<Position>)linearRing.Coordinates;

            // Get the absolute max bounds
            foreach (Position positison in linearRing.Coordinates)
            {
                minLng = Math.Min(positison.Longitude, minLng);
                maxLng = Math.Max(positison.Longitude, maxLng);
                minLat = Math.Min(positison.Latitude, minLat);
                maxLat = Math.Max(positison.Latitude, maxLat);
            }
        }

        Position spanPosition = new Position((maxLng - minLng) * 1.6, (maxLat - minLat) * 1.6);

        return spanPosition;
    }
}
