using Groundsman.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Groundsman.Helpers;

/// <summary>
/// JSON converter used to read JSON and determine the imported items GeoJSONType
/// </summary>
internal class GeoJSONObjectConverter : JsonConverter<GeoJSONObject>
{
    public override bool CanRead => true;
    public override bool CanWrite => false;

    public override GeoJSONObject ReadJson(JsonReader reader, Type objectType, GeoJSONObject existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.Null) //GeoJSONObject can be abstract
        {
            return null;
        }

        JObject token = JObject.Load(reader);

        if (!token.TryGetValue("type", StringComparison.OrdinalIgnoreCase, out JToken TypeToken))
        {
            throw new JsonReaderException("Invalid geojson object, does not have 'type' field.");
        }

        // Match GeoJSONType to actual type
        GeoJSONType tokenType = TypeToken.ToObject<GeoJSONType>(serializer);
        Type actualType = tokenType switch
        {
            GeoJSONType.Point => typeof(Models.Point),
            GeoJSONType.LineString => typeof(LineString),
            GeoJSONType.Polygon => typeof(Polygon),
            GeoJSONType.Feature => typeof(Feature),
            GeoJSONType.FeatureCollection => typeof(FeatureCollection),
            _ => null, //TODO: proper check here
        };

        if (existingValue == null || existingValue.GetType() != actualType)
        {
            return (GeoJSONObject)token.ToObject(actualType, serializer);
        }
        else
        {
            using (JsonReader derivedTypeReader = token.CreateReader())
            {
                serializer.Populate(derivedTypeReader, existingValue);
            }

            return existingValue;
        }
    }

    public override void WriteJson(JsonWriter writer, GeoJSONObject value, JsonSerializer serializer) => throw new NotImplementedException();
}
