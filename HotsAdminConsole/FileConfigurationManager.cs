using Newtonsoft.Json;
using System.IO;

namespace HotsAdminConsole;

public class FileConfigurationManager
{
    public bool IsInitialized { get; set; }

    public string Load(string fn, string @default)
    {
        const string basePath = @"c:\minilog";
        var path = Path.Combine(basePath, fn);
        return File.Exists(path) ? File.ReadAllText(path) : @default;
    }

    public T Load<T>(string fn, T @default)
    {
        const string basePath = @"c:\minilog";
        var path = Path.Combine(basePath, fn);
        var txt = File.Exists(path) ? File.ReadAllText(path) : null;
        if (txt == null)
        {
            return @default;
        }

        try
        {
            var rc = JsonConvert.DeserializeObject<T>(txt);
            return rc ?? @default;
        }
        catch
        {
            return @default;
        }
    }

    public void Save(string fn, string val)
    {
        if (!IsInitialized)
        {
            return;
        }

        const string basePath = @"c:\minilog";
        var path = Path.Combine(basePath, fn);
        try
        {
            File.WriteAllText(path, val);
        }
        catch
        {
            /* ignored */
        }
    }

    public void Save<T>(string fn, T val)
    {
        var txt = JsonConvert.SerializeObject(val);
        Save(fn, txt);
    }
}
