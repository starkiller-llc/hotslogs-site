using System.Text.Json;

namespace HotsLogsApi;

public class MyNamingPolicy : JsonNamingPolicy
{
    public override string ConvertName(string name)
    {
        return name;
    }
}
