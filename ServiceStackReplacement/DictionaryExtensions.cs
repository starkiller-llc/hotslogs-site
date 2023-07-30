using System.Collections.Concurrent;
using System.Collections.Generic;

namespace ServiceStackReplacement;

public static class DictionaryExtensions
{
    public static TValue GetValueOrDefault<TKey, TValue>(this Dictionary<TKey, TValue> dic, TKey key)
    {
        return !dic.ContainsKey(key) ? default : dic[key];
    }

    public static TValue GetValueOrDefault<TKey, TValue>(this ConcurrentDictionary<TKey, TValue> dic, TKey key)
    {
        return !dic.ContainsKey(key) ? default : dic[key];
    }
}
