using System;
using System.Collections.Generic;

namespace TalentsCli;

public static class IEnumerableExtensions
{
    public static IEnumerable<TResult> Pairwise<TSource, TResult>(
        this IEnumerable<TSource> source,
        Func<TSource, TSource, TResult> resultSelector)
    {
        TSource previous = default;

        using var it = source.GetEnumerator();

        if (it.MoveNext())
        {
            previous = it.Current;
        }

        while (it.MoveNext())
        {
            yield return resultSelector(previous, previous = it.Current);
        }
    }

    public static IEnumerable<TResult> Triplewise<TSource, TResult>(
        this IEnumerable<TSource> source,
        Func<TSource, TSource, TSource, TResult> resultSelector)
    {
        TSource a = default;
        TSource b = default;

        using var it = source.GetEnumerator();

        if (it.MoveNext())
        {
            a = it.Current;
        }

        if (it.MoveNext())
        {
            b = it.Current;
        }
        else
        {
            yield return resultSelector(default, a, default);
        }

        while (it.MoveNext())
        {
            yield return resultSelector(a, b, it.Current);
            a = b;
            b = it.Current;
        }
    }
}
