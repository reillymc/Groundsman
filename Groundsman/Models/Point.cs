using Groundsman.JSONConverters;
using Newtonsoft.Json;

namespace Groundsman.Models
{
    /// <summary>
    /// A geometry object where the "coordinates" member is a single position
    /// </summary>
    [JsonConverter(typeof(DummyConverter))]
    class Point : Geometry
    {
        [JsonProperty(PropertyName = "coordinates")]
        public Position Coordinates { get; set; }

        [JsonConstructor]
        public Point(Position coordinates) : base(GeoJSONType.Point) => Coordinates = coordinates;
    }
}
