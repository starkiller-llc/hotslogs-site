using Amazon;
using System.IO;

namespace HotsLogsApi.BL.Amazon;

public static class AmazonResponseExtensions
{
    public static void WriteResponseStreamToFile(this Response response, string filePath)
    {
        using var fs = new FileStream(filePath, FileMode.Create);
        response.ResponseStream.CopyTo(fs);
    }
}
