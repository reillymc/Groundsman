using Newtonsoft.Json;
using System;

namespace Groundsman.JSONConverters
{
    /// <summary>
    /// This converter is used to block the base Geometry converter so that any inherited classes
    /// use the default serialiser/deserialiser when the GeometryConverter calls ToObject() on them
    /// This avoids an infinitive recursive loop
    /// </summary>
    public class DummyConverter : JsonConverter
    {
        public override bool CanRead => false;

        public override bool CanWrite => false;

        public override bool CanConvert(Type objectType) => true;

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) => throw new NotImplementedException();

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) => throw new NotImplementedException();
    }
}
