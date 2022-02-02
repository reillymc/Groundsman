using Groundsman.JSONConverters;
using Newtonsoft.Json;

namespace Groundsman.Models;

/// <summary>
/// A geometry object where the "coordinates" member is a single position
/// </summary>
[JsonConverter(typeof(DummyConverter))]
public class Point : Geometry
{
    [JsonProperty(PropertyName = "coordinates", Order = 2)]
    public Position Coordinates { get; set; }

    [JsonConstructor]
    public Point(Position coordinates) : base(GeoJSONType.Point) => Coordinates = coordinates ?? throw new ArgumentNullException(nameof(coordinates), "A Point must have coordinates.");

    // TODO implement when upgraded to .NET 6
    //public override Point FromPositions(IEnumerable<DisplayPosition> displayPositions)
    //{
    //    throw new NotImplementedException();
    //}
}
