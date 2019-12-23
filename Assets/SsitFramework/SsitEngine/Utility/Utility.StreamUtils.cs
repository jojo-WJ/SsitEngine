using System.IO;

namespace SsitEngine
{
    /// <summary>
    ///     IO流的工具类.
    /// </summary>
    public static class StreamUtils
    {
        /// <summary>
        ///     读取完整的流到byte[].
        /// </summary>
        /// <returns>The stream content into a byte array.</returns>
        /// <param name="stream">Input stream.</param>
        public static byte[] ReadFullStream( Stream stream )
        {
            var buffer = new byte[4096];
            using (var memoryStream = new MemoryStream())
            {
                int read;
                while ((read = stream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    memoryStream.Write(buffer, 0, read);
                }
                return memoryStream.ToArray();
            }
        }
    }
}