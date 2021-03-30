using System;
using System.Collections.Generic;
using System.Linq;
using Groundsman.JSONConverters;
using Newtonsoft.Json;

namespace Groundsman.Models
{
    /// <summary>
    /// A geometry object where the "coordinates" member is an array of two or more positions
    /// </summary>
    [JsonConverter(typeof(DummyConverter))]
    public class LineString : Geometry
    {
        [JsonProperty(PropertyName = "coordinates")]
        public IEnumerable<Position> Coordinates { get; set; }

        public LineString(IEnumerable<Position> coordinates) : base(GeoJSONType.LineString)
        {
            if (coordinates != null)
            {
                if (coordinates.Count() >= 2)
                {
                    Coordinates = coordinates;
                } else
                {
                    throw new ArgumentException("A LineString must have two or more positions.", "Coordinates");
                }
            }
            else
            {
                throw new ArgumentNullException("Coordinates", "A LineString must have coordinates.");
            }
        }


        /// <summary>
        /// Import an individual LineString geometry
        /// </summary>
        /// <param name="json">GeJSON LineString geometry</param>
        /// <returns>LineString object from GeoJSON</returns>
        public static new LineString ImportGeoJSON(string json) => JsonConvert.DeserializeObject<LineString>(json);


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

            var polyline = (List<Position>)Coordinates;

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
    }
}
