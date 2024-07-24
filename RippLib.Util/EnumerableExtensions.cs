using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace RippLib.Readability;

public static class EnumerableExtensions
{
    public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> collection)
    {
        var rnd = new Random();
        var shuffledList = collection.ToList();
        var n = shuffledList.Count;
        while (n > 1)
        {
            n--;
            var k = rnd.Next(n + 1);
            (shuffledList[n], shuffledList[k]) = (shuffledList[k], shuffledList[n]);
        }

        return shuffledList;
    }

    public static IEnumerable<IEnumerable<T>> Batch<T>(this IEnumerable<T> collection, int batchSize)
    {
        var nextbatch = new List<T>(batchSize);
        foreach (var item in collection)
        {
            nextbatch.Add(item);
            if (nextbatch.Count == batchSize)
            {
                yield return nextbatch;
                nextbatch = new List<T>(batchSize);
            }
        }

        if (nextbatch.Count > 0)
            yield return nextbatch;
    }

    public static bool NotEmpty<T>(this List<T> list)
    {
        return list is not null && list.Count > 0;
    }

    public static bool NotEmpty<T>(this T[] array)
    {
        return array is not null && array.Length > 0;
    }

    public static bool NotEmpty<T>(this IEnumerable<T> enumerable)
    {
        if (enumerable is List<T> list)
            return list.Count > 0;
        if (enumerable is T[] array)
            return array.Length > 0;
        return enumerable is not null && enumerable.Any();
    }

    public static bool NotEmpty<T>(this IEnumerable<T> list, Func<T, bool> query)
    {
        return list is not null && list.Any(query);
    }

    public static bool Empty<T>(this List<T> list)
    {
        return list is null || list.Count == 0;
    }

    public static bool Empty<T>(this T[] array)
    {
        return array is null || array.Length == 0;
    }

    public static bool Empty<T>(this IEnumerable<T> enumerable)
    {
        return enumerable is null || !enumerable.Any();
    }

    public static bool Empty<T>(this IEnumerable<T> enumerable, Func<T, bool> query)
    {
        return enumerable is null || !enumerable.Any(query);
    }
}
