using System;
using Groundsman.JSONConverters;
using Newtonsoft.Json;

namespace Groundsman.Models
{
    /// <summary>
    /// A geometry object where the "coordinates" member is a single position
    /// </summary>
    [JsonConverter(typeof(DummyConverter))]
    public class Point : Geometry
    {
        [JsonProperty(PropertyName = "coordinates", Order = 2)]
        public Position Coordinates { get; set; }

        [JsonConstructor]
        public Point(Position coordinates) : base(GeoJSONType.Point) => Coordinates = coordinates ?? throw new ArgumentNullException(nameof(coordinates), "A Point must have coordinates.");

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
            {
                return true;
            }
            if ((obj == null) || !GetType().Equals(obj.GetType()))
            {
                return false;
            }
            else
            {
                Point comparePoint = (Point)obj;
                return Coordinates.Equals(comparePoint.Coordinates);
            }
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Coordinates);
        }
    }
}
