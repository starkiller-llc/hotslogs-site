using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Threading.Tasks;

namespace ServiceStackReplacement;

public static class RedisExtensions
{
    public static bool ContainsKey(this MyDbWrapper db, string key)
    {
        if (db == null)
        {
            return false;
        }

        return db.Db.KeyExists(key);
    }

    public static T Get<T>(this MyDbWrapper db, string key)
    {
        var value = db.Db.StringGet(key);
        return value.IsNull ? default : JsonConvert.DeserializeObject<T>(value, GetSettings());
    }

    public static async Task<T> GetAsync<T>(this MyDbWrapper db, string key)
    {
        var value = await db.Db.StringGetAsync(key);
        return value.IsNull ? default : JsonConvert.DeserializeObject<T>(value, GetSettings());
    }

    public static MyDbWrapper GetClient(this ConnectionMultiplexer mux)
    {
        if (mux == null)
        {
            return null;
        }

        return new MyDbWrapper(mux.GetDatabase());
    }

    public static int GetInt(this MyDbWrapper db, string key)
    {
        var value = db.Db.StringGet(key);
        if (value.IsNull)
        {
            return default;
        }

        return int.TryParse(value, out var rc) ? rc : 0;
    }

    public static bool Set(this MyDbWrapper db, string key, object obj)
    {
        var value = JsonConvert.SerializeObject(obj, GetSettings());
        return db.Db.StringSet(key, value);
    }

    public static bool Set(this MyDbWrapper db, string key, object obj, TimeSpan expiration)
    {
        var value = JsonConvert.SerializeObject(obj, GetSettings());
        return db.Db.StringSet(key, value, expiration);
    }

    public static bool Set(this MyDbWrapper db, string key, object obj, DateTime expiration)
    {
        var value = JsonConvert.SerializeObject(obj, GetSettings());
        return db.Db.StringSet(key, value, expiration - DateTime.UtcNow);
    }

    private static JsonSerializerSettings GetSettings()
    {
        var rc = new JsonSerializerSettings();
        rc.Converters.Add(new TimeSpanConverter());
        return rc;
    }
}
