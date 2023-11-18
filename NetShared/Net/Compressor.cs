using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetShared.Net
{
    public static class Compressor
    {
        public static byte[] Compress(byte[] bytes)
        {
            using(MemoryStream memStream = new MemoryStream())
            using(DeflateStream deflateStream = new DeflateStream(memStream, CompressionLevel.Optimal))
            {
                deflateStream.Write(bytes, 0, bytes.Length);
                return memStream.ToArray();
            }
        }

        public static byte[] Decompress(byte[] bytes)
        {
            using (MemoryStream inputStream = new MemoryStream(bytes))
            using (MemoryStream outputStream = new MemoryStream())
            using(DeflateStream dStream = new DeflateStream(inputStream, CompressionMode.Decompress))
            {
                dStream.CopyTo(outputStream);
                return outputStream.ToArray();
            }
        }

    }
}
