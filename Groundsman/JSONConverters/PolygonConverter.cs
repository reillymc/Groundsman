using System;
using System.Collections.Generic;
using Groundsman.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Groundsman.JSONConverters
{
    /// <summary>
    /// JSON converter used to handle the structuring and destructuring of polygons with LineStrings instead of nested list of positions
    /// </summary>
    internal class PolygonConverter : JsonConverter<Polygon>
    {
        public override bool CanRead => true;
        public override bool CanWrite => true;

        public override Polygon ReadJson(JsonReader reader, Type objectType, Polygon existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            JObject token = JObject.Load(reader);

            IEnumerable<IEnumerable<Position>> coordinates = token.GetValue("coordinates", StringComparison.OrdinalIgnoreCase).ToObject<IEnumerable<IEnumerable<Position>>>(serializer);

            // Take this array of arrays of arrays and create line strings
            // and use those to create create polygons

            List<LinearRing> coordList = new List<LinearRing>();
            foreach (IEnumerable<Position> coordPos in coordinates)
            {
                coordList.Add(new LinearRing(coordPos));
            }

            Polygon polygon = new Polygon(coordList);
            return polygon;
        }

        public override void WriteJson(JsonWriter writer, Polygon value, JsonSerializer serializer)
        {
            Polygon test = value;
            List<List<Position>> coordinates = new List<List<Position>>();
            foreach (LinearRing linearRing in value.Coordinates)
            {
                coordinates.Add((List<Position>)linearRing.Coordinates);
            }
            JToken.FromObject(new { type = value.Type.ToString(), coordinates }).WriteTo(writer);
        }
    }
}
