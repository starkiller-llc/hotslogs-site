using Newtonsoft.Json;

namespace ServiceStackReplacement;

public static class JsonExtensions
{
    public static T FromJson<T>(this string json)
    {
        return JsonConvert.DeserializeObject<T>(json, GetSettings());
    }

    public static string ToJson<T>(this T obj)
    {
        return JsonConvert.SerializeObject(obj, GetSettings());
    }

    private static JsonSerializerSettings GetSettings()
    {
        var rc = new JsonSerializerSettings();
        rc.Converters.Add(new TimeSpanConverter());
        return rc;
    }
}
