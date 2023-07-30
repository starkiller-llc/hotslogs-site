using System.IO;

namespace ServiceStackReplacement;

public static class StreamExtensions
{
    public static byte[] ReadFully(this Stream stream)
    {
        using (var ms = new MemoryStream())
        {
            stream.CopyTo(ms);
            return ms.ToArray();
        }
    }
}
