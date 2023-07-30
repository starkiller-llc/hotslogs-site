using System.Collections.Generic;

namespace HotsLogsApi.BL.Migration.Helpers;

public static class DictionaryExtensions
{
    public static TValue GetValueOrNull<TKey, TValue>(this IDictionary<TKey, TValue> dic, TKey key)
        where TValue : class
    {
        if (!dic.ContainsKey(key))
        {
            return null;
        }

        return dic[key];
    }
}
