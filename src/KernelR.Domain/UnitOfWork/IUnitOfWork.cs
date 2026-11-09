namespace KernelR.Domain.UnitOfWork;

/// <summary>
/// Defines a contract for the Unit of Work pattern, which coordinates the
/// persistence of changes made to aggregate roots within a single transaction.
/// </summary>
/// <remarks>
/// <para>
/// The Unit of Work pattern maintains a list of objects affected by a business
/// transaction and coordinates the writing out of changes. It ensures that all
/// changes are committed together or none at all, preserving data consistency.
/// </para>
/// <para>
/// In Domain-Driven Design, a Unit of Work typically spans a single application
/// use case (command). Changes made to multiple aggregate roots within that use
/// case are committed atomically.
/// </para>
/// <para>
/// The implementation of this interface is expected to be provided by the
/// infrastructure layer (e.g., using Entity Framework Core's <c>DbContext</c>),
/// and is typically registered with a scoped lifetime so that all repositories
/// share the same unit of work.
/// </para>
/// </remarks>
/// <seealso cref="Repository{TId, TAggregateRoot}"/>
/// <seealso cref="IUnitOfWorkTransaction"/>
public interface IUnitOfWork
{
    /// <summary>
    /// Saves all pending changes made within the current unit of work.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>
    /// The number of state entries written to the underlying data store.
    /// <b>Note:</b> This value is a hint for logging and diagnostics; it has
    /// no domain meaning and should not be relied upon by application code.
    /// </returns>
    /// <remarks>
    /// <para>
    /// In most implementations (e.g. EF Core), this method implicitly wraps the
    /// pending changes in a single transaction and commits them atomically.
    /// If the underlying store does not support implicit transactions, the
    /// implementation must ensure that all changes are applied atomically.
    /// </para>
    /// <para>
    /// <b>Domain events:</b> Domain events raised by aggregate roots are not
    /// dispatched by this method. Dispatching belongs to the persistence layer
    /// (for example, an EF Core interceptor or a domain-event dispatcher invoked
    /// after this method returns).
    /// </para>
    /// </remarks>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Begins a new transaction that spans multiple calls to
    /// <see cref="SaveChangesAsync"/>.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>
    /// A disposable transaction handle. The caller must call
    /// <see cref="IUnitOfWorkTransaction.CommitAsync"/> to commit the
    /// transaction, or <see cref="IUnitOfWorkTransaction.RollbackAsync"/>
    /// to roll it back.
    /// </returns>
    /// <remarks>
    /// <para>
    /// Use this method when a single use case needs multiple
    /// <see cref="SaveChangesAsync"/> calls that must all succeed or all fail.
    /// For a single <see cref="SaveChangesAsync"/> call, this method is not
    /// needed because the call is implicitly transactional.
    /// </para>
    /// <para>
    /// The returned transaction must be disposed after commit or rollback.
    /// Disposing an uncommitted transaction rolls it back.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// await using (IUnitOfWorkTransaction transaction = await unitOfWork.BeginTransactionAsync())
    /// {
    ///     order.MarkAsPaid();
    ///     await unitOfWork.SaveChangesAsync();
    ///
    ///     payment.RecordReceipt();
    ///     await unitOfWork.SaveChangesAsync();
    ///
    ///     await transaction.CommitAsync();
    /// }
    /// </code>
    /// </example>
    Task<IUnitOfWorkTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);
}
