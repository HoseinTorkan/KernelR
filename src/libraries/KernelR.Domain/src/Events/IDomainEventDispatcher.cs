namespace KernelR.Domain.Events;

/// <summary>
/// Defines a contract for dispatching domain events to their registered handlers.
/// </summary>
/// <remarks>
/// <para>
/// Dispatching is responsible for delivering domain events to their handlers.
/// It is separate from raising events within the domain model.
/// </para>
/// <para>
/// The dispatching strategy, handler execution order, exception handling,
/// and transaction boundaries are determined by the implementation.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// await _dispatcher.DispatchAsync(order.DomainEvents, cancellationToken);
/// </code>
/// </example>
/// <seealso cref="IDomainEvent"/>
/// <seealso cref="IDomainEventHandler{TEvent}"/>
public interface IDomainEventDispatcher
{
    /// <summary>
    /// Dispatches the specified domain events to their registered handlers.
    /// </summary>
    /// <param name="domainEvents">
    /// The domain events to dispatch.
    /// </param>
    /// <param name="cancellationToken">
    /// A token to cancel the dispatching operation.
    /// </param>
    /// <returns>
    /// A task representing the asynchronous dispatching operation.
    /// </returns>
    Task DispatchAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken cancellationToken = default);
}
