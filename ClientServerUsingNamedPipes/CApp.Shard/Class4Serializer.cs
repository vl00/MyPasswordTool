using Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Common
{
    interface ISerializer
    {
        Task<T> ReadAsync<T>(Stream stream, CancellationToken cancellationToken);
        Task WriteAsync(Stream stream, object serializableObject);
    }

    class StreamJsonSerializer : ISerializer
    {
        static byte[] serialize(object o)
        {
            var json = JsonConvert.SerializeObject(o);
            return Encoding.UTF8.GetBytes(json);
        }

        static JToken deserialize(byte[] buffer)
        {
            return JToken.Parse(Encoding.UTF8.GetString(buffer));
        }
    }
}
