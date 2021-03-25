using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Groundsman.JSONConverters
{
    /// <summary>
    /// This converter is used to delete internal dictionary entries that shouldnt be exported when writing JSON
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    public class PropertiesConverter<TKey, TValue> : JsonConverter<IDictionary<string, object>>
    {
        public override bool CanRead => false;

        public override IDictionary<string, object> ReadJson(JsonReader reader, Type objectType, IDictionary<string, object> existingValue, bool hasExistingValue, JsonSerializer serializer) => throw new NotImplementedException();

        public override void WriteJson(JsonWriter writer, IDictionary<string, object> value, JsonSerializer serializer)
        {
            writer.WriteStartObject();

            foreach (var pair in value)
            {
                if (pair.Key != "id")
                {
                    writer.WritePropertyName(pair.Key);

                    serializer.Serialize(writer, pair.Value);
                }
            }
            writer.WriteEndObject();
        }
    }
}
