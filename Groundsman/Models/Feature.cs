using System.Collections.Generic;
using Groundsman.JSONConverters;
using Newtonsoft.Json;

namespace Groundsman.Models
{
    /// <summary>
    /// A Feature object represents a spatially bounded thing.  Every Feature
    /// object is a GeoJSON object no matter where it occurs in a GeoJSON text.
    /// </summary> 
    [JsonConverter(typeof(DummyConverter))]
    public class Feature : GeoJSONObject
    {
        [JsonProperty(PropertyName = "geometry")]
        public Geometry Geometry { get; set; }

        [JsonProperty(PropertyName = "properties"), JsonConverter(typeof(PropertiesConverter<string, object>))]
        public IDictionary<string, object> Properties { get; set; }

        [JsonConstructor]
        public Feature(Geometry geometry = null, IDictionary<string, object> properties = null) : base(GeoJSONType.Feature)
        {
            Geometry = geometry;
            Properties = properties;
        }
    }
}
