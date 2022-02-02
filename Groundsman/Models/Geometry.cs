using System.Linq;
using Newtonsoft.Json;

namespace Groundsman.Models;

/// <summary>
/// An abstract class inherited by all geometry types - essentially verifies they are correct types
/// </summary>
public abstract class Geometry : GeoJSONObject
{
    private readonly GeoJSONType[] ValidGeometries = { GeoJSONType.Point, GeoJSONType.LineString, GeoJSONType.Polygon }; // Should also support Multi-variants and GeometryCollection eventually
    [JsonConstructor]
    public Geometry(GeoJSONType type) : base(type)
    {
        if (!ValidGeometries.Contains(Type))
        {
            throw new ArgumentException($"The type {type} is not a valid geometry type.");
        }
    }

    // TODO implement when upgraded to .NET 6
    //public abstract Geometry FromPositions(IEnumerable<DisplayPosition> displayPositions);

    public override bool Equals(object obj)
    {
        return base.Equals(obj);
    }
}
