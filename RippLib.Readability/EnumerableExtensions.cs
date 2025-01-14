using System;
using System.Collections.Generic;
using System.Linq;

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

    /// <summary>
    /// Checks if a list is not null and has at least one element.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="list">The list to check</param>
    /// <returns>True if there is atleast one element in the list, otherwise false</returns>
    public static bool NotEmpty<T>(this List<T> list)
    {
        return list is not null && list.Count > 0;
    }

    /// <summary>
    /// Checks if an array is not null and has at least one element.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="array">The array to check</param>
    /// <returns>True if there is atleast one element in the array, otherwise false</returns>
    public static bool NotEmpty<T>(this T[] array)
    {
        return array is not null && array.Length > 0;
    }

    /// <summary>
    /// Checks if an enumerable is not null and has at least one element.
    /// Build in fallbacks for pattern mathed List & Array
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="enumerable">The enumerable to check</param>
    /// <returns>True if there is atleast one element in the enumerable, otherwise false</returns>
    public static bool NotEmpty<T>(this IEnumerable<T> enumerable)
    {
        if (enumerable is List<T> list)
            return list.Count > 0;
        if (enumerable is T[] array)
            return array.Length > 0;
        return enumerable is not null && enumerable.Any();
    }

    /// <summary>
    /// Checks if an enumerable is not null and has at least one element that matches the query.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="enumerable">The enumerable to check</param>
    /// <param name="query">Linq expression to query</param>
    /// <returns>True if there is atleast one matching element in the enumerable, otherwise false</returns>
    public static bool NotEmpty<T>(this IEnumerable<T> enumerable, Func<T, bool> query)
    {
        return enumerable is not null && enumerable.Any(query);
    }

    /// <summary>
    /// Checks if a list is null or empty.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="list">The list to check</param>
    /// <returns>True if null or contains no elements</returns>
    public static bool Empty<T>(this List<T> list)
    {
        return list is null || list.Count == 0;
    }

    /// <summary>
    /// Checks if an array is null or empty.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="array">The array to check</param>
    /// <returns>True if null or contains no elements</returns>
    public static bool Empty<T>(this T[] array)
    {
        return array is null || array.Length == 0;
    }

    /// <summary>
    /// Checks if an enumerable is null or empty.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="enumerable">The enumerable to check</param>
    /// <returns>True if null or contains no elements</returns>
    public static bool Empty<T>(this IEnumerable<T> enumerable)
    {
        return enumerable is null || !enumerable.Any();
    }

    /// <summary>
    /// Checks if an enumerable is null or contains no elements that match the query.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="enumerable"></param>
    /// <param name="query">Linq expression to query</param>
    /// <returns>True if null or when the query returns no results</returns>
    /// <remarks>Will return True even if the enumerable contains data. We only look at the query results</remarks>
    public static bool Empty<T>(this IEnumerable<T> enumerable, Func<T, bool> query)
    {
        return enumerable is null || !enumerable.Any(query);
    }
}
