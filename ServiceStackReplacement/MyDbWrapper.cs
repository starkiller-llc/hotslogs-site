using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ServiceStackReplacement;

public sealed class MyDbWrapper : IDisposable
{
    public MyDbWrapper(IDatabase db)
    {
        Db = db;
    }

    public IDatabase Db { get; }

    public string this[string key]
    {
        get => Db.StringGet(key);
        set => Db.StringSet(key, value);
    }

    public void Dispose() { }

    public static MyDbWrapper Create(IServiceScope scope)
    {
        return scope.ServiceProvider.GetRequiredService<MyDbWrapper>();
    }

    public bool AddItemToSet(string key, string value, CommandFlags flags = CommandFlags.None)
    {
        return Db.SetAdd(key, value, flags);
    }

    public bool AddItemToSortedSet(
        string key,
        string member,
        double score,
        When when = When.Always,
        CommandFlags flags = CommandFlags.None)
    {
        return Db.SortedSetAdd(key, member, score, when, flags);
    }

    public bool AddToHyperLog(string key, string value, CommandFlags flags = CommandFlags.None)
    {
        return Db.HyperLogLogAdd(key, value, flags);
    }

    public long CountHyperLog(string key, CommandFlags flags = CommandFlags.None)
    {
        return Db.HyperLogLogLength(key, flags);
    }

    public bool ExpireEntryAt(string key, DateTime? expiry, CommandFlags flags = CommandFlags.None)
    {
        return Db.KeyExpire(key, expiry, flags);
    }

    public bool ExpireEntryIn(string key, TimeSpan? expiry, CommandFlags flags = CommandFlags.None)
    {
        return Db.KeyExpire(key, expiry, flags);
    }

    public string[] GetAllItemsFromSet(string key, CommandFlags flags = CommandFlags.None)
    {
        return Db.SetMembers(key, flags).Select(x => x.ToString()).ToArray();
    }

    public long? GetItemIndexInSortedSetDesc(
        string key,
        string value,
        Order order = Order.Ascending,
        CommandFlags flags = CommandFlags.None)
    {
        return Db.SortedSetRank(key, value, order, flags);
    }

    public double GetItemScoreInSortedSet(string key, string member, CommandFlags flags = CommandFlags.None)
    {
        return Db.SortedSetScore(key, member, flags).GetValueOrDefault();
    }

    public string[] GetRangeFromSortedSetDesc(
        string key,
        int start,
        int stop,
        Order order = Order.Descending,
        CommandFlags flags = CommandFlags.None)
    {
        return Db.SortedSetRangeByRank(key, start, stop, order, flags).Select(x => x.ToString()).ToArray();
    }

    public object GetSortedSetCount(
        string key,
        double min = double.NegativeInfinity,
        double max = double.PositiveInfinity,
        Exclude exclude = Exclude.None,
        CommandFlags flags = CommandFlags.None)
    {
        return Db.SortedSetLength(key, min, max, exclude, flags);
    }

    public TimeSpan? GetTimeToLive(string key, CommandFlags flags = CommandFlags.None)
    {
        return Db.KeyTimeToLive(key, flags);
    }

    public long Increment(string key, long value)
    {
        return IncrementValue(key, value);
    }

    public long IncrementValue(string key, long value = 1, CommandFlags flags = CommandFlags.None)
    {
        return Db.StringIncrement(key, value, flags);
    }

    public double IncrementValue(string key, double value, CommandFlags flags = CommandFlags.None)
    {
        return Db.StringIncrement(key, value, flags);
    }

    public bool Remove(string key, CommandFlags flags = CommandFlags.None)
    {
        return Db.KeyDelete(key, flags);
    }

    public async Task<bool> RemoveAsync(string key, CommandFlags flags = CommandFlags.None)
    {
        return await Db.KeyDeleteAsync(key, flags);
    }
}
