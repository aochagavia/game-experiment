using System;
using System.IO;
using System.Reflection;

namespace Client
{
    public static class StreamExtensions
    {
        public static Stream GetResourceStream(this Assembly assembly, string name)
        {
            return assembly.GetManifestResourceStream(name) ?? throw new Exception("Resource not found");
        }

        public static byte[] ReadAsUtf8Bytes(this Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            using var memoryStream = new MemoryStream();
            stream.CopyTo(memoryStream);

            stream.Dispose();

            return memoryStream.ToArray();
        }
    }
}