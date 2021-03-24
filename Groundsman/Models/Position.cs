using Newtonsoft.Json;

namespace Groundsman.Models {
    /// <summary>
    /// A position is the fundamental geometry construct. An array of two - three values
    /// </summary>
    public class Position {

        [JsonProperty(PropertyName = "latitude")]
        public double Latitude { get; set; }

        [JsonProperty(PropertyName = "longitude")]
        public double Longitude { get; set; }

        [JsonProperty(PropertyName = "altitude")]
        public double? Altitude { get; set; }

        [JsonConstructor]
        public Position(double lat, double lng, double? alt) {
            Latitude = lat;
            Longitude = lng;
            Altitude = alt;
        }
    }
}