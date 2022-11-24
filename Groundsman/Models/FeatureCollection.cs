using Groundsman.Helpers;
using Newtonsoft.Json;

namespace Groundsman.Models;

/// <summary>
/// Array of feature objects
/// </summary>
[JsonConverter(typeof(DummyConverter))]
public class FeatureCollection : GeoJSONObject
{
    [JsonProperty(PropertyName = "features", Order = 2)]
    public IEnumerable<Feature> Features { get; set; }

    [JsonConstructor]
    public FeatureCollection(IEnumerable<Feature> features) : base(GeoJSONType.FeatureCollection) => Features = features ?? throw new ArgumentNullException("features");
}
