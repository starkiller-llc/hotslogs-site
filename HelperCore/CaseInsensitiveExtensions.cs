using System;
using System.Collections.Generic;
using System.Linq;

namespace HelperCore;

public static class CaseInsensitiveExtensions
{
    public static Dictionary<string, TValue> ToCaseInsensitiveDictionary<T, TValue>(
        this IEnumerable<T> collection,
        Func<T, string> keySelector,
        Func<T, TValue> valueSelector)
    {
        return collection.ToDictionary(keySelector, valueSelector, StringComparer.OrdinalIgnoreCase);
    }

    public static HashSet<string> ToCaseInsensitiveHashSet(
        this IEnumerable<string> collection)
    {
        return new HashSet<string>(collection, StringComparer.OrdinalIgnoreCase);
    }
}
