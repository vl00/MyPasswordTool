using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;

namespace Newtonsoft.Json
{
    public static partial class JsonNetExtensions
    {
        static JsonNetExtensions()
        {
            SerializerSettings = new JsonSerializerSettings
            {
                MissingMemberHandling = MissingMemberHandling.Ignore,
                NullValueHandling = NullValueHandling.Include,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                TypeNameHandling = TypeNameHandling.None,
                ConstructorHandling = ConstructorHandling.Default
            };
        }

        public static JsonSerializerSettings SerializerSettings { get; set; }

        public static JsonSerializerSettings CopyTo(this JsonSerializerSettings src, JsonSerializerSettings dest)
        {
            if (dest == null) throw new ArgumentNullException(nameof(dest));
            //dest.Binder = src.Binder;
            dest.CheckAdditionalContent = src.CheckAdditionalContent;
            dest.ConstructorHandling = src.ConstructorHandling;
            dest.Context = src.Context;
            dest.ContractResolver = src.ContractResolver;
            dest.Converters = src.Converters;
            dest.Culture = src.Culture;
            dest.DateFormatHandling = src.DateFormatHandling;
            dest.DateFormatString = src.DateFormatString;
            dest.DateParseHandling = src.DateParseHandling;
            dest.DateTimeZoneHandling = src.DateTimeZoneHandling;
            dest.DefaultValueHandling = src.DefaultValueHandling;
            dest.EqualityComparer = src.EqualityComparer;
            dest.Error = src.Error;
            dest.FloatFormatHandling = src.FloatFormatHandling;
            dest.FloatParseHandling = src.FloatParseHandling;
            dest.Formatting = src.Formatting;
            dest.MaxDepth = src.MaxDepth;
            dest.MetadataPropertyHandling = src.MetadataPropertyHandling;
            dest.MissingMemberHandling = src.MissingMemberHandling;
            dest.NullValueHandling = src.NullValueHandling;
            dest.ObjectCreationHandling = src.ObjectCreationHandling;
            dest.PreserveReferencesHandling = src.PreserveReferencesHandling;
            dest.ReferenceLoopHandling = src.ReferenceLoopHandling;
            //dest.ReferenceResolver = src.ReferenceResolver;
            dest.ReferenceResolverProvider = src.ReferenceResolverProvider;
            dest.SerializationBinder = src.SerializationBinder;
            dest.StringEscapeHandling = src.StringEscapeHandling;
            dest.TraceWriter = src.TraceWriter;
            //dest.TypeNameAssemblyFormat = src.TypeNameAssemblyFormat;
            dest.TypeNameAssemblyFormatHandling = src.TypeNameAssemblyFormatHandling;
            dest.TypeNameHandling = src.TypeNameHandling;
            return dest;
        }

        public static string ToJson(this object value)
        {
            return ToJson(value, SerializerSettings);
        }

        public static string ToJson(this object value, JsonSerializerSettings jsonSerializerSettings)
        {
			if (value == null) return null;
            return JsonConvert.SerializeObject(value, Formatting.None, jsonSerializerSettings);
        }

        public static T ToObject<T>(this string json)
        {
            return (T)ToObject(json, typeof(T));
        }

        public static object ToObject(this string json, Type type = null)
        {
            return ToObject(json, type, SerializerSettings);
        }

        public static T ToObject<T>(this string json, JsonSerializerSettings jsonSerializerSettings)
        {
            return (T)ToObject(json, typeof(T), jsonSerializerSettings);
        }

        public static object ToObject(this string json, Type type, JsonSerializerSettings jsonSerializerSettings)
        {
            try
            {
                return JsonConvert.DeserializeObject(json, type, jsonSerializerSettings);
            }
            catch (Exception ex) //JsonSerializationException ex
            {
                if (string.IsNullOrEmpty(json)) return null;
                throw new SerializationException(ex.Message, ex);
            }
        }
    }
}