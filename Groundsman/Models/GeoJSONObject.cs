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
        [JsonProperty(PropertyName = "type", Order = 1), JsonConverter(typeof(StringEnumConverter))]
        public GeoJSONType Type { get; set; }

        protected GeoJSONObject(GeoJSONType type) => Type = type;

        /// <summary>
        /// Import a GeoJSON object. Used when type is not known ahead of time and can be cast to the correct object once imported
        /// </summary>
        /// <param name="json">GeoJSON to import</param>
        /// <returns>A valid GeoJSONObject</returns>
        public static GeoJSONObject ImportGeoJSON(string json) => JsonConvert.DeserializeObject<GeoJSONObject>(json);

        /// <summary>
        /// Export a GeoJSON object to serialised string.
        /// </summary>
        /// <param name="geoJSONObject">GeoJSON object to export</param>
        /// <returns>A serialised string</returns>
        public static string ExportGeoJSON(GeoJSONObject geoJSONObject, bool stripBlobbedObjects = false) => stripBlobbedObjects
            ? JsonConvert.SerializeObject(geoJSONObject, new JsonSerializerSettings { ContractResolver = new DynamicContractResolver("PropertiesBlobbed", "GeometryBlobbed") })
            : JsonConvert.SerializeObject(geoJSONObject);

    }
}
