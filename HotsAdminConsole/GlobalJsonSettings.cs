using Newtonsoft.Json;
using System;

namespace HotsAdminConsole;

public class GlobalJsonSettings : IDisposable
{
    private readonly Func<JsonSerializerSettings> _oldSettings;

    public GlobalJsonSettings(JsonSerializerSettings settings)
    {
        _oldSettings = JsonConvert.DefaultSettings;
        JsonConvert.DefaultSettings = () => settings;
    }

    public void Dispose()
    {
        JsonConvert.DefaultSettings = _oldSettings;
    }
}
