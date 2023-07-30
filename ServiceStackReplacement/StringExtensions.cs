using System;
using System.Globalization;

namespace ServiceStackReplacement;

public static class StringExtensions
{
    public static bool ContainsIgnoreCase(this string haystack, string needle)
    {
        return CultureInfo.InvariantCulture.CompareInfo.IndexOf(haystack, needle, CompareOptions.IgnoreCase) >= 0;
    }

    public static bool EqualsIgnoreCase(this string a, string b)
    {
        return a.Equals(b, StringComparison.InvariantCultureIgnoreCase);
    }

    public static bool IsNullOrEmpty(this string s)
    {
        return String.IsNullOrEmpty(s);
    }
}
