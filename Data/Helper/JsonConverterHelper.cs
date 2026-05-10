using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http;
using JsonConverter = Newtonsoft.Json.JsonConverter;
using System.Linq;

namespace Data.Helper
{
    public class SafeCollectionConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return true;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            return serializer.Deserialize<JToken>(reader).ToObjectCollectionSafe(objectType, serializer);
        }

        public override bool CanWrite => false;

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
    public static class SafeJsonConvertExtensions
    {
        public static object ToObjectCollectionSafe(this JToken jToken, Type objectType)
        {
            return ToObjectCollectionSafe(jToken, objectType, JsonSerializer.CreateDefault());
        }

        public static object ToObjectCollectionSafe(this JToken jToken, Type objectType, JsonSerializer jsonSerializer)
        {
            var expectArray = typeof(System.Collections.IEnumerable).IsAssignableFrom(objectType);

            if (jToken is JArray jArray)
            {
                if (!expectArray)
                {
                    if (jArray.Count == 0)
                        return JValue.CreateNull().ToObject(objectType, jsonSerializer);

                    if (jArray.Count == 1)
                        return jArray.First.ToObject(objectType, jsonSerializer);
                }
            }
            else if (expectArray)
            {
                return new JArray(jToken).ToObject(objectType, jsonSerializer);
            }

            return jToken.ToObject(objectType, jsonSerializer);
        }

        public static T ToObjectCollectionSafe<T>(this JToken jToken)
        {
            return (T)ToObjectCollectionSafe(jToken, typeof(T));
        }

        public static T ToObjectCollectionSafe<T>(this JToken jToken, JsonSerializer jsonSerializer)
        {
            return (T)ToObjectCollectionSafe(jToken, typeof(T), jsonSerializer);
        }
    }
    public class BoolConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteValue(((bool)value) ? true : false);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var expectArray = typeof(bool).IsAssignableFrom(objectType);
            return reader.Value?.ToString() == "1" || reader.Value?.ToString().ToUpper() == "TRUE";
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(bool);
        }
    }
}
