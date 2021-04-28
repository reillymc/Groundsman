using System;
using System.Linq;
using Groundsman.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Groundsman.JSONConverters
{
    /// <summary>
    /// JSON converter used to handle the structuring and destructuring points to/from positions that have an optional altitude parameter
    /// </summary>
    public class PositionConverter : JsonConverter<Position>
    {
        public override bool CanRead => true;

        public override bool CanWrite => true;

        public override Position ReadJson(JsonReader reader, Type objectType, Position existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            // This is an array of doubles
            JArray token = JArray.Load(reader);

            double longitude = token.ElementAt(0).ToObject<double>(serializer);
            double latitude = token.ElementAt(1).ToObject<double>(serializer);
            double elevation = double.NaN;

            if (token.Count == 3)
            {
                elevation = token.ElementAt(2).ToObject<double>(serializer);
            }

            return new Position(longitude, latitude, elevation);
        }

        public override void WriteJson(JsonWriter writer, Position value, JsonSerializer serializer)
        {
            if (value.HasAltitude())
            {
                JToken.FromObject(new double[3] { value.Longitude, value.Latitude, value.Altitude }).WriteTo(writer);
            }
            else
            {
                JToken.FromObject(new double[2] { value.Longitude, value.Latitude }).WriteTo(writer);
            }
        }
    }
}
