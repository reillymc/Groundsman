using Groundsman.JSONConverters;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Groundsman.Models
{
    /// <summary>
    /// A geometry object where the "coordinates" member is a list of closed LineStrings
    /// </summary>
    [JsonConverter(typeof(PolygonConverter))]
    internal class Polygon: Geometry
    {
        [JsonProperty(PropertyName = "coordinates")]
        public IEnumerable<LineString> Coordinates { get; set; }

        public Polygon(IEnumerable<LineString> coordinates) : base(GeoJSONType.Polygon)
        {
            Coordinates = coordinates;
        }
    }
}
