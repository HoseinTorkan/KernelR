namespace KernelR.Domain.UnitOfWork;

/// <summary>
/// Represents an active transaction started by
/// <see cref="IUnitOfWork.BeginTransactionAsync"/>.
/// </summary>
/// <remarks>
/// <para>
/// A transaction groups multiple <see cref="IUnitOfWork.SaveChangesAsync"/>
/// calls into a single atomic unit. Either all changes are committed together,
/// or none of them are.
/// </para>
/// <para>
/// The transaction must be disposed after commit or rollback. Disposing an
/// uncommitted transaction rolls it back automatically, which makes
/// <c>await using</c> a safe default.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// await using (IUnitOfWorkTransaction transaction = await unitOfWork.BeginTransactionAsync())
/// {
///     // ... changes ...
///     await unitOfWork.SaveChangesAsync();
///     await transaction.CommitAsync();
/// }
/// </code>
/// </example>
/// <seealso cref="IUnitOfWork"/>
public interface IUnitOfWorkTransaction : IAsyncDisposable
{
    /// <summary>
    /// Commits the transaction, persisting all changes made since it began.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous commit operation.</returns>
    /// <remarks>
    /// After this method returns successfully, the transaction is complete.
    /// Calling <see cref="IAsyncDisposable.DisposeAsync"/> afterwards is safe.
    /// </remarks>
    Task CommitAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Rolls back the transaction, discarding all changes made since it began.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous rollback operation.</returns>
    /// <remarks>
    /// After this method returns, the transaction is complete. Calling
    /// <see cref="IAsyncDisposable.DisposeAsync"/> afterwards is safe.
    /// </remarks>
    Task RollbackAsync(CancellationToken cancellationToken = default);
}
