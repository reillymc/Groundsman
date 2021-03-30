using System;
using System.Collections.Generic;
using System.Linq;
using Groundsman.JSONConverters;
using Newtonsoft.Json;

namespace Groundsman.Models
{
    /// <summary>
    /// A geometry object where the "coordinates" member is a list of closed LineStrings
    /// </summary>
    [JsonConverter(typeof(PolygonConverter))]
    public class Polygon : Geometry
    {
        [JsonProperty(PropertyName = "coordinates")]
        public IEnumerable<LinearRing> Coordinates { get; set; }

        public Polygon(IEnumerable<LinearRing> coordinates) : base(GeoJSONType.Polygon)
        {
            if (coordinates != null)
            {
                if (coordinates.Count() > 0)
                {
                    Coordinates = coordinates;
                }
                else
                {
                    throw new ArgumentException("A Polygon must have one or more linear rings.", "Coordinates");
                }
            }
            else
            {
                throw new ArgumentNullException("Coordinates", "A Polygon must have coordinates.");
            }
        }

        /// <summary>
        /// Import an individual polygon geometry
        /// </summary>
        /// <param name="json">GeJSON polygon geometry</param>
        /// <returns>Polygon object from GeoJSON</returns>
        public static new Polygon ImportGeoJSON(string json) => JsonConvert.DeserializeObject<Polygon>(json);


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
    }
}
