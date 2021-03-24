using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Groundsman.JSONConverters
{
    public class PropertiesConverter<TKey, TValue> : JsonConverter<IDictionary<string, object>>
    {
        public override bool CanRead => false;

        public override IDictionary<string, object> ReadJson(JsonReader reader, Type objectType, IDictionary<string, object> existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override void WriteJson(JsonWriter writer, IDictionary<string, object> value, JsonSerializer serializer)
        {
            var dictionary = value;

            writer.WriteStartObject();

            foreach (var pair in dictionary)
            {
                if (pair.Key != "id" && pair.Key != "xamarincoordinates")
                {
                    writer.WritePropertyName(pair.Key);

                    serializer.Serialize(writer, pair.Value);
                }
            }

            writer.WriteEndObject();
        }
    }
}
