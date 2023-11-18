using NetShared.Exceptions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace NetShared.Net
{
    public class Serializer : ISerializer
    {
        public byte[] Serialize(object obj)
        {
            try
            {
                
                using (MemoryStream memStream = new MemoryStream())
                {
                    new BinaryFormatter().Serialize(memStream, obj);
                    return memStream.ToArray();
                }
            }
            catch (Exception e)
            {
                throw new SerializationException(e.Message);
            }
        }

        public T Deserialize<T>(byte[] bytes, int index, int count)
        {
            try
            {
                using (MemoryStream memStream = new MemoryStream(bytes, index, count))
                {
                    var obj = new BinaryFormatter().Deserialize(memStream);
                    return (T)obj;
                }
            }
            catch (Exception e)
            {
                throw new SerializationException($"Error Deserializing #{bytes.Length} starting from {index} with count {bytes.Length - index} =>\"{e.Message}\"");
            }
        }
    }
}
