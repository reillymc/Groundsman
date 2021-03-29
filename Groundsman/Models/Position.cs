using System;
using Groundsman.JSONConverters;
using Newtonsoft.Json;

namespace Groundsman.Models
{
    /// <summary>
    /// A position is the fundamental geometry construct. An array of two - three values
    /// </summary>
    [JsonConverter(typeof(PositionConverter))]
    public class Position
    {
        [JsonProperty(PropertyName = "longitude")]
        public double Longitude { get; set; }

        [JsonProperty(PropertyName = "latitude")]
        public double Latitude { get; set; }

        [JsonProperty(PropertyName = "altitude")]
        public double Altitude { get; set; }

        public Position(double longitude, double latitude) : this(longitude, latitude, double.NaN)
        {
        }

        [JsonConstructor]
        public Position(double longitude, double latitude, double altitude)
        {
            if (double.IsNaN(longitude) || double.IsNaN(latitude))
            {
                throw new ArgumentNullException("Longitude and Latitude cannot be null in a position");
            }
                Longitude = longitude;
            Latitude = latitude;
            Altitude = altitude;
        }

        public bool HasAltitude() => !double.IsNaN(Altitude);

        public override string ToString() => Latitude + ", " + Longitude + ", " + Altitude;

        public bool Equals(Position comparePosition)
        {
            if (ReferenceEquals(this, comparePosition))
            {
                return true;
            }
            bool comparison = Latitude == comparePosition.Latitude && Longitude == comparePosition.Longitude;

            if (!double.IsNaN(Altitude) || !double.IsNaN(comparePosition.Altitude))
            {
                comparison = comparison && (Altitude == comparePosition.Altitude);
            }

            return comparison;
        }
    }
}
