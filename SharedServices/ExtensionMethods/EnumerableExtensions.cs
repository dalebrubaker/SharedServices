using System;
using System.Collections.Generic;
using System.Linq;

namespace BruSoftware.SharedServices.ExtensionMethods;

public static class EnumerableExtensions
{
    public static HashSet<TKey> ToHashSet<TSource, TKey>(this IEnumerable<TSource> repeatedField, Func<TSource, TKey> keySelector)
    {
        var result = new HashSet<TKey>();
        foreach (var element in repeatedField)
        {
            result.Add(keySelector(element));
        }
        return result;
    }

    public static IEnumerable<TSource> GetDuplicates<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> selector,
        IEqualityComparer<TKey> comparer)
    {
        var hash = new HashSet<TKey>(comparer);
        return source.Where(item => !hash.Add(selector(item))).ToList();
    }

    public static IEnumerable<TSource> GetDuplicates<TSource>(this IEnumerable<TSource> source, IEqualityComparer<TSource> comparer)
    {
        return source.GetDuplicates(x => x, comparer);
    }

    /// <summary>
    /// Thanks to https://stackoverflow.com/questions/18547354/c-sharp-linq-find-duplicates-in-list
    /// </summary>
    /// <typeparam name="TSource"></typeparam>
    /// <param name="source"></param>
    /// <returns></returns>
    public static IEnumerable<TSource> GetDuplicates<TSource>(this IEnumerable<TSource> source)
    {
        return source.GetDuplicates(x => x, null);
    }

    public static bool HasDuplicates<TSource>(this IEnumerable<TSource> source)
    {
        return GetDuplicates(source).Any();
    }
}