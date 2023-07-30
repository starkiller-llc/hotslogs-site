using System.IO;
using System.Net;

namespace ServiceStackReplacement;

public static class WebExceptionExtensions
{
    public static string GetResponseBody(this WebException x)
    {
        var stream = x.Response?.GetResponseStream();
        if (stream == null)
        {
            return null;
        }

        using (var s = x.Response.GetResponseStream())
        using (var b = new StreamReader(s))
        {
            return b.ReadToEnd();
        }
    }
}
