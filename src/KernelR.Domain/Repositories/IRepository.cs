using KernelR.Domain.Abstractions;
using System.Linq.Expressions;

namespace KernelR.Domain.Repositories;

/// <summary>
/// Defines a generic contract for repositories that manage the persistence
/// of aggregate roots with a unique identifier.
/// </summary>
/// <typeparam name="TId">
/// The type of the aggregate root's identifier. Typically a strongly-typed
/// identifier (for example <c>ProductId</c>) or a primitive value type
/// (for example <see cref="Guid"/> or <see cref="long"/>).
/// </typeparam>
/// <typeparam name="TAggregateRoot">
/// The type of the aggregate root managed by this repository.
/// Must implement <see cref="IAggregateRoot{TId}"/>.
/// </typeparam>
/// <remarks>
/// <para>
/// In Domain-Driven Design, only aggregate roots are allowed to have
/// repositories. Other entities within an aggregate must be accessed through
/// their aggregate root.
/// </para>
/// <para>
/// This interface is intentionally generic and only covers the most common
/// operations. More specific queries should be defined in dedicated repository
/// interfaces (for example <c>IOrderRepository</c>) in the domain layer.
/// </para>
/// <para>
/// <typeparamref name="TId"/> is expected to match
/// <c>TAggregateRoot.Id</c>; implementations should enforce this by
/// delegating <see cref="GetByIdAsync"/> to the aggregate's identity.
/// </para>
/// </remarks>
/// <seealso cref="IAggregateRoot{TId}"/>
public interface IRepository<TId, TAggregateRoot>
    where TId : struct
    where TAggregateRoot : IAggregateRoot<TId>
{
    /// <summary>
    /// Gets the aggregate root with the specified identifier, or
    /// <see langword="null"/> if not found.
    /// </summary>
    /// <param name="id">The identifier of the aggregate root to retrieve.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>
    /// The aggregate root with the specified identifier, or
    /// <see langword="null"/> if no such aggregate root exists.
    /// </returns>
    Task<TAggregateRoot?> GetByIdAsync(
        TId id,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the first aggregate root matching the specified predicate, or
    /// <see langword="null"/> if none found.
    /// </summary>
    /// <param name="predicate">The condition to filter aggregate roots.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>
    /// The first aggregate root matching the predicate, or
    /// <see langword="null"/> if no match exists.
    /// </returns>
    /// <remarks>
    /// The order in which rows are returned is not defined; implementations
    /// should apply an explicit ordering if the caller relies on it.
    /// </remarks>
    Task<TAggregateRoot?> FirstOrDefaultAsync(
        Expression<Func<TAggregateRoot, bool>> predicate,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all aggregate roots matching the specified predicate.
    /// </summary>
    /// <param name="predicate">The condition to filter aggregate roots.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>
    /// A read-only list of aggregate roots matching the predicate.
    /// Returns an empty list if no match exists.
    /// </returns>
    /// <remarks>
    /// The order in which rows are returned is not defined; implementations
    /// should apply an explicit ordering if the caller relies on it.
    /// </remarks>
    Task<IReadOnlyList<TAggregateRoot>> ListAsync(
        Expression<Func<TAggregateRoot, bool>> predicate,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all aggregate roots in the repository.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>
    /// A read-only list of all aggregate roots. Returns an empty list if the
    /// repository is empty.
    /// </returns>
    /// <remarks>
    /// This method is typically used for small datasets only. For large
    /// datasets, prefer <see cref="ListAsync"/> with a predicate or implement
    /// pagination in a dedicated repository.
    /// </remarks>
    Task<IReadOnlyList<TAggregateRoot>> GetAllAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks whether any aggregate root matches the specified predicate.
    /// </summary>
    /// <param name="predicate">The condition to test aggregate roots against.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>
    /// <see langword="true"/> if at least one aggregate root matches the predicate;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    Task<bool> AnyAsync(
        Expression<Func<TAggregateRoot, bool>> predicate,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks whether an aggregate root with the specified identifier exists.
    /// </summary>
    /// <param name="id">The identifier to check.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>
    /// <see langword="true"/> if an aggregate root with the specified identifier
    /// exists; otherwise, <see langword="false"/>.
    /// </returns>
    Task<bool> ExistsAsync(
        TId id,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Counts the aggregate roots matching the specified predicate.
    /// </summary>
    /// <param name="predicate">The condition to count aggregate roots against.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>
    /// The number of aggregate roots matching the predicate.
    /// </returns>
    Task<int> CountAsync(
        Expression<Func<TAggregateRoot, bool>> predicate,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new aggregate root to the repository.
    /// </summary>
    /// <param name="aggregateRoot">The aggregate root to add.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task AddAsync(
        TAggregateRoot aggregateRoot,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a collection of new aggregate roots to the repository.
    /// </summary>
    /// <param name="aggregateRoots">The aggregate roots to add.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task AddRangeAsync(
        IEnumerable<TAggregateRoot> aggregateRoots,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks an existing aggregate root as modified.
    /// </summary>
    /// <param name="aggregateRoot">The aggregate root to update.</param>
    /// <remarks>
    /// This method is synchronous because it only changes the tracking state;
    /// the actual persistence happens in <c>IUnitOfWork.SaveChangesAsync</c>.
    /// </remarks>
    void Update(TAggregateRoot aggregateRoot);

    /// <summary>
    /// Marks a collection of existing aggregate roots as modified.
    /// </summary>
    /// <param name="aggregateRoots">The aggregate roots to update.</param>
    /// <remarks>
    /// This method is synchronous because it only changes the tracking state;
    /// the actual persistence happens in <c>IUnitOfWork.SaveChangesAsync</c>.
    /// </remarks>
    void UpdateRange(IEnumerable<TAggregateRoot> aggregateRoots);

    /// <summary>
    /// Marks an existing aggregate root for deletion.
    /// </summary>
    /// <param name="aggregateRoot">The aggregate root to delete.</param>
    /// <remarks>
    /// This method is synchronous because it only changes the tracking state;
    /// the actual persistence happens in <c>IUnitOfWork.SaveChangesAsync</c>.
    /// </remarks>
    void Delete(TAggregateRoot aggregateRoot);

    /// <summary>
    /// Marks a collection of existing aggregate roots for deletion.
    /// </summary>
    /// <param name="aggregateRoots">The aggregate roots to delete.</param>
    /// <remarks>
    /// This method is synchronous because it only changes the tracking state;
    /// the actual persistence happens in <c>IUnitOfWork.SaveChangesAsync</c>.
    /// </remarks>
    void DeleteRange(IEnumerable<TAggregateRoot> aggregateRoots);
}
