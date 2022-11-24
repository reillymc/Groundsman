using Groundsman.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Groundsman.Helpers;

/// <summary>
/// JSON converter used to read JSON and determine the imported item's type based one the type property
/// </summary>
internal class GeometryConverter : JsonConverter<Geometry>
{
    public override bool CanRead => true;
    public override bool CanWrite => false;

    public override Geometry ReadJson(JsonReader reader, Type objectType, Geometry existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.Null) // Geometry can be null in a feature
        {
            return null;
        }

        JObject token = JObject.Load(reader);

        if (!token.TryGetValue("type", StringComparison.OrdinalIgnoreCase, out JToken TypeToken))
        {
            throw new JsonReaderException("Invalid geojson geometry object, does not have 'type' field.");
        }

        GeoJSONType tokenType = TypeToken.ToObject<GeoJSONType>(serializer);
        Type actualType = tokenType switch
        {
            GeoJSONType.Point => typeof(Models.Point),
            GeoJSONType.LineString => typeof(LineString),
            GeoJSONType.Polygon => typeof(Polygon),
            _ => null, //TODO: proper check here
        };

        if (existingValue == null || existingValue.GetType() != actualType)
        {
            return (Geometry)token.ToObject(actualType, serializer);
        }
        else
        {
            using (JsonReader DerivedTypeReader = token.CreateReader())
            {
                serializer.Populate(DerivedTypeReader, existingValue);
            }

            return existingValue;
        }
    }

    public override void WriteJson(JsonWriter writer, Geometry value, JsonSerializer serializer) => throw new NotImplementedException();
}
