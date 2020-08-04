using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Groundsman.Models
{
    public enum FeatureType
    {
        Point,
        LineString,
        Polygon
    }

    public class GeoJSONObject
    {
        public string type { get; set; }
        public ObservableCollection<Feature> features { get; set; }

    }

    public class Feature
    {
        public string type { get; set; }
        public Geometry geometry { get; set; }
        public Properties properties { get; set; }
    }

    public class Geometry
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public FeatureType type { get; set; }
        public List<object> coordinates { get; set; }
    }

    public class Properties
    {
        public string name { get; set; }
        public string author { get; set; }
        public string date { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string metadataStringValue { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? metadataIntegerValue { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public float? metadataFloatValue { get; set; }
        [JsonIgnore]
        public string id { get; set; }
        [JsonIgnore]
        public List<Point> xamarincoordinates { get; set; }
    }
}
