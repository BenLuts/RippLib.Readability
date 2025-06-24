using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RippLib.Readability;

public static class QueryableExtensions
{
    /// <summary>
    /// Checks if a list is not null and has at least one element.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="list">The list to check</param>
    /// <returns>True if there is atleast one element in the list, otherwise false</returns>
    public static async Task<bool> NotEmptyAsync<T>(this IQueryable<T> list, CancellationToken cancellationToken = default)
    {
        return await list.AnyAsync(cancellationToken);
    }

    ///// <summary>
    ///// Checks if an array is not null and has at least one element.
    ///// </summary>
    ///// <typeparam name="T"></typeparam>
    ///// <param name="array">The array to check</param>
    ///// <returns>True if there is atleast one element in the array, otherwise false</returns>
    //public static bool NotEmpty<T>(this T[] array)
    //{
    //    return array is not null && array.Length > 0;
    //}

    ///// <summary>
    ///// Checks if an enumerable is not null and has at least one element.
    ///// Build in fallbacks for pattern mathed List & Array
    ///// </summary>
    ///// <typeparam name="T"></typeparam>
    ///// <param name="enumerable">The enumerable to check</param>
    ///// <returns>True if there is atleast one element in the enumerable, otherwise false</returns>
    //public static bool NotEmpty<T>(this IEnumerable<T> enumerable)
    //{
    //    if (enumerable is List<T> list)
    //        return list.Count > 0;
    //    if (enumerable is T[] array)
    //        return array.Length > 0;
    //    return enumerable is not null && enumerable.Any();
    //}

    ///// <summary>
    ///// Checks if an enumerable is not null and has at least one element that matches the query.
    ///// </summary>
    ///// <typeparam name="T"></typeparam>
    ///// <param name="enumerable">The enumerable to check</param>
    ///// <param name="query">Linq expression to query</param>
    ///// <returns>True if there is atleast one matching element in the enumerable, otherwise false</returns>
    //public static bool Has<T>(this IEnumerable<T> enumerable, Func<T, bool> query)
    //{
    //    return enumerable is not null && enumerable.Any(query);
    //}

    ///// <summary>
    ///// Checks if a list is null or empty.
    ///// </summary>
    ///// <typeparam name="T"></typeparam>
    ///// <param name="list">The list to check</param>
    ///// <returns>True if null or contains no elements</returns>
    //public static bool Empty<T>(this List<T> list)
    //{
    //    return list is null || list.Count == 0;
    //}

    ///// <summary>
    ///// Checks if an array is null or empty.
    ///// </summary>
    ///// <typeparam name="T"></typeparam>
    ///// <param name="array">The array to check</param>
    ///// <returns>True if null or contains no elements</returns>
    //public static bool Empty<T>(this T[] array)
    //{
    //    return array is null || array.Length == 0;
    //}

    ///// <summary>
    ///// Checks if an enumerable is null or empty.
    ///// </summary>
    ///// <typeparam name="T"></typeparam>
    ///// <param name="enumerable">The enumerable to check</param>
    ///// <returns>True if null or contains no elements</returns>
    //public static bool Empty<T>(this IEnumerable<T> enumerable)
    //{
    //    return enumerable is null || !enumerable.Any();
    //}

    ///// <summary>
    ///// Checks if an enumerable is null or contains no elements that match the query.
    ///// </summary>
    ///// <typeparam name="T"></typeparam>
    ///// <param name="enumerable"></param>
    ///// <param name="query">Linq expression to query</param>
    ///// <returns>True if null or when the query returns no results</returns>
    //public static bool HasNo<T>(this IEnumerable<T> enumerable, Func<T, bool> query)
    //{
    //    return enumerable is null || !enumerable.Any(query);
    //}
}
