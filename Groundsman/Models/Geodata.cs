using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Groundsman.Models
{

    public class GeoJSONObject
    {
        [JsonProperty(PropertyName = "type"), JsonConverter(typeof(StringEnumConverter))]
        public GeoJSONType Type { get; set; }
    }

    public class FeatureCollection: GeoJSONObject
    {
        [JsonProperty(PropertyName = "features")]
        public IEnumerable<Feature> Features { get; set; }
    }
    

    public class Geometry: GeoJSONObject
    {
        [JsonProperty(PropertyName = "coordinates")]
        public List<object> Coordinates { get; set; }
    }
}
