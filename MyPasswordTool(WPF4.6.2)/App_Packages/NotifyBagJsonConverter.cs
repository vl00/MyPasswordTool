using Newtonsoft.Json;
using SilverEx;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace MyPasswordTool
{
    /// <summary>
    /// Converts an NotifyBag to and from JSON.
    /// </summary>
    public class NotifyBagJsonConverter : JsonConverter
    {
        /// <summary>
        /// Determines whether this instance can convert the specified object type.
        /// </summary>
        /// <param name="objectType">Type of the object.</param>
        /// <returns>
        /// 	<c>true</c> if this instance can convert the specified object type; otherwise, <c>false</c>.
        /// </returns>
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(NotifyBag) || typeof(NotifyBag).IsAssignableFrom(objectType);
        }

        /// <summary>
        /// Writes the JSON representation of the object.
        /// </summary>
        /// <param name="writer">The <see cref="T:Newtonsoft.Json.JsonWriter" /> to write to.</param>
        /// <param name="value">The value.</param>
        /// <param name="serializer">The calling serializer.</param>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, (value as NotifyBag)?.Dictionary());
        }

        /// <summary>
        /// Reads the JSON representation of the object.
        /// </summary>
        /// <param name="reader">The <see cref="T:Newtonsoft.Json.JsonReader" /> to read from.</param>
        /// <param name="objectType">Type of the object.</param>
        /// <param name="existingValue">The existing value of object being read.</param>
        /// <param name="serializer">The calling serializer.</param>
        /// <returns>The object value.</returns>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            return this.ReadValue(reader);
        }

        private object ReadValue(JsonReader reader)
        {
            while (reader.TokenType == JsonToken.Comment)
            {
                if (!reader.Read())
                {
                    throw new JsonSerializationException("Unexpected end when reading NotifyBag.");
                }
            }
            switch (reader.TokenType)
            {
                case JsonToken.StartObject:
                    return this.ReadObject(reader);
                case JsonToken.StartArray:
                    return this.ReadList(reader);
                default:
                    if (IsPrimitiveToken(reader.TokenType)) return reader.Value;
                    throw new JsonSerializationException(string.Format(CultureInfo.InvariantCulture, "Unexpected token when converting NotifyBag: {0}", reader.TokenType));
            }
        }

        private object ReadList(JsonReader reader)
        {
            IList<object> list = new List<object>();
            while (reader.Read())
            {
                JsonToken tokenType = reader.TokenType;
                if (tokenType != JsonToken.Comment)
                {
                    if (tokenType == JsonToken.EndArray)
                    {
                        return list;
                    }
                    object item = this.ReadValue(reader);
                    list.Add(item);
                }
            }
            throw new JsonSerializationException("Unexpected end when reading NotifyBag.");
        }

        private object ReadObject(JsonReader reader)
        {
            var dictionary = new NotifyBag();
            while (reader.Read())
            {
                JsonToken tokenType = reader.TokenType;
                switch (tokenType)
                {
                    case JsonToken.PropertyName:
                        {
                            string key = reader.Value.ToString();
                            if (!reader.Read())
                            {
                                throw new JsonSerializationException("Unexpected end when reading NotifyBag.");
                            }
                            object value = this.ReadValue(reader);
                            dictionary[key] = value;
                            break;
                        }
                    case JsonToken.Comment:
                        break;
                    default:
                        if (tokenType == JsonToken.EndObject) return dictionary;
                        break;
                }
            }
            throw new JsonSerializationException("Unexpected end when reading NotifyBag.");
        }

        private static bool IsPrimitiveToken(JsonToken token)
        {
            switch (token)
            {
                case JsonToken.Integer:
                case JsonToken.Float:
                case JsonToken.String:
                case JsonToken.Boolean:
                case JsonToken.Null:
                case JsonToken.Undefined:
                case JsonToken.Date:
                case JsonToken.Bytes:
                    return true;
            }
            return false;
        }
    }
}