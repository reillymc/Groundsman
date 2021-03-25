using Groundsman.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Groundsman.JSONConverters
{
    /// <summary>
    /// JSON converter used to handle the structuring and destructuring of polygons with LineStrings instead of nested list of positions
    /// </summary>
    class PolygonConverter : JsonConverter<Polygon>
    {
        public override bool CanRead => true;
        public override bool CanWrite => true;

        public override Polygon ReadJson(JsonReader reader, Type objectType, Polygon existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            JObject token = JObject.Load(reader);

            IEnumerable<IEnumerable<Position>> coordinates = token.GetValue("coordinates", StringComparison.OrdinalIgnoreCase).ToObject<IEnumerable<IEnumerable<Position>>>(serializer);

            // Take this array of arrays of arrays and create line strings
            // and use those to create create polygons
            return new Polygon(coordinates.Select(x => new LineString(x)));
        }

        public override void WriteJson(JsonWriter writer, Polygon value, JsonSerializer serializer)
        {
            JToken.FromObject(new { type = value.Type.ToString(), coordinates = value.Coordinates.Select(x => x.Coordinates) }).WriteTo(writer);
        }
    }
}
