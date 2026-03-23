using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Linq.Expressions;

namespace RippLib.Readability.EFExtensions;

public static class QueryableExtensions
{
    /// <summary>
    ///     Asynchronously determines whether a sequence has elements.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Multiple active operations on the same context instance are not supported. Use <see langword="await" /> to ensure
    ///         that any asynchronous operations have completed before calling another method on this context.
    ///         See <see href="https://aka.ms/efcore-docs-threading">Avoiding DbContext threading issues</see> for more information and examples.
    ///     </para>
    ///     <para>
    ///         See <see href="https://aka.ms/efcore-docs-async-linq">Querying data with EF Core</see> for more information and examples.
    ///     </para>
    /// </remarks>
    /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
    /// <param name="source">An <see cref="IQueryable{T}" /> whose elements to test for a condition.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation.
    ///     The task result contains <see langword="true" /> if the source sequence has any elements; otherwise, <see langword="false" />.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="source" /> is <see langword="null" />.
    /// </exception>
    /// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
    public static Task<bool> NotEmptyAsync<T>(this IQueryable<T> queryable, CancellationToken cancellationToken = default)
    {
        return queryable.AnyAsync(cancellationToken);
    }

    /// <summary>
    ///     Asynchronously determines whether a sequence has no elements.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Multiple active operations on the same context instance are not supported. Use <see langword="await" /> to ensure
    ///         that any asynchronous operations have completed before calling another method on this context.
    ///         See <see href="https://aka.ms/efcore-docs-threading">Avoiding DbContext threading issues</see> for more information and examples.
    ///     </para>
    ///     <para>
    ///         See <see href="https://aka.ms/efcore-docs-async-linq">Querying data with EF Core</see> for more information and examples.
    ///     </para>
    /// </remarks>
    /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
    /// <param name="source">An <see cref="IQueryable{T}" /> whose elements to test for a condition.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation.
    ///     The task result contains <see langword="true" /> if the source sequence has no elements; otherwise, <see langword="false" />.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="source" /> is <see langword="null" />.
    /// </exception>
    /// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
    public static async Task<bool> EmptyAsync<T>(this IQueryable<T> queryable, CancellationToken cancellationToken = default)
    {
        return !await queryable.AnyAsync(cancellationToken);
    }

    /// <summary>
    ///     Asynchronously determines whether any element of a sequence satisfies a condition.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Multiple active operations on the same context instance are not supported. Use <see langword="await" /> to ensure
    ///         that any asynchronous operations have completed before calling another method on this context.
    ///         See <see href="https://aka.ms/efcore-docs-threading">Avoiding DbContext threading issues</see> for more information and examples.
    ///     </para>
    ///     <para>
    ///         See <see href="https://aka.ms/efcore-docs-async-linq">Querying data with EF Core</see> for more information and examples.
    ///     </para>
    /// </remarks>
    /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
    /// <param name="source">An <see cref="IQueryable{T}" /> whose elements to test for a condition.</param>
    /// <param name="predicate">A function to test each element for a condition.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation.
    ///     The task result contains <see langword="true" /> if any elements in the source sequence pass the test in the specified
    ///     predicate; otherwise, <see langword="false" />.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="source" /> or <paramref name="predicate" /> is <see langword="null" />.
    /// </exception>
    /// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
    public static Task<bool> ContainsAnyAsync<T>(this IQueryable<T> queryable, Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return queryable.AnyAsync(predicate, cancellationToken);
    }

    /// <summary>
    ///     Asynchronously determines whether no elements of a sequence satisfy a condition.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Multiple active operations on the same context instance are not supported. Use <see langword="await" /> to ensure
    ///         that any asynchronous operations have completed before calling another method on this context.
    ///         See <see href="https://aka.ms/efcore-docs-threading">Avoiding DbContext threading issues</see> for more information and examples.
    ///     </para>
    ///     <para>
    ///         See <see href="https://aka.ms/efcore-docs-async-linq">Querying data with EF Core</see> for more information and examples.
    ///     </para>
    /// </remarks>
    /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
    /// <param name="source">An <see cref="IQueryable{T}" /> whose elements to test for a condition.</param>
    /// <param name="predicate">A function to test each element for a condition.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation.
    ///     The task result contains <see langword="true" /> if no elements in the source sequence satisfy the condition;
    ///     otherwise, <see langword="false" />.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="source" /> or <paramref name="predicate" /> is <see langword="null" />.
    /// </exception>
    /// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
    public static async Task<bool> ContainsNoneAsync<T>(this IQueryable<T> queryable, Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return !await queryable.AnyAsync(predicate, cancellationToken);
    }

}
