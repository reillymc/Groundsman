using Groundsman.JSONConverters;
using Newtonsoft.Json;
using System.Collections.Generic;

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

        public LineString(IEnumerable<Position> coordinates) : base(GeoJSONType.LineString) => Coordinates = coordinates;
    }
}
