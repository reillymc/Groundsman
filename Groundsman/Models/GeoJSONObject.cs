using Groundsman.JSONConverters;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Groundsman.Models
{
    /// <summary>
    /// Represents a Geometry, Feature, or collection of Features.
    /// </summary>
    [JsonConverter(typeof(GeoJSONObjectConverter))]
    public class GeoJSONObject
    {
        [JsonProperty(PropertyName = "type"), JsonConverter(typeof(StringEnumConverter))]
        public GeoJSONType Type { get; set; }

        protected GeoJSONObject(GeoJSONType type) => Type = type;
    }
}
