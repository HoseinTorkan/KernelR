using KernelR.Domain.Abstractions;

namespace KernelR.Domain.Events;

/// <summary>
/// Defines a contract for dispatching domain events to their registered handlers.
/// </summary>
/// <remarks>
/// <para>
/// The dispatcher is typically invoked by the persistence layer (e.g., an EF Core
/// <c>SaveChanges</c> interceptor) after aggregate roots have been saved but before
/// the surrounding transaction commits. This ordering lets handlers participate in the
/// same transaction as the state change that triggered the event.
/// </para>
/// <para>
/// <b>Order of operations:</b>
/// </para>
/// <list type="number">
///   <item><description>Collect events from all tracked aggregate roots.</description></item>
///   <item><description>Clear the events from each aggregate (so they are not dispatched twice).</description></item>
///   <item><description>Dispatch each event to all registered handlers for its exact runtime type.</description></item>
///   <item><description>If any handler throws, the exception propagates and the transaction rolls back.</description></item>
/// </list>
/// <para>
/// <b>Why "dispatch" and not just "raise"?</b> The term is intentional. Raising happens
/// inside the aggregate (<see cref="IAggregateRoot{TId}.AddDomainEvent"/>); dispatching
/// happens outside the aggregate, in the application or persistence layer. Keeping the two
/// separated preserves the purity of the domain model.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// await _dispatcher.DispatchAsync(order.DomainEvents, cancellationToken);
/// order.ClearDomainEvents();
/// </code>
/// </example>
/// <seealso cref="IDomainEvent"/>
/// <seealso cref="IDomainEventHandler{TEvent}"/>
public interface IDomainEventDispatcher
{
    /// <summary>
    /// Dispatches the specified domain events to all registered handlers.
    /// </summary>
    /// <param name="domainEvents">The events to dispatch.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous dispatch operation.</returns>
    /// <remarks>
    /// <para>
    /// Handlers are invoked sequentially, in an unspecified order. If a handler throws,
    /// the exception propagates and the remaining events (and their handlers) are not invoked.
    /// This fail-fast behavior is intentional: it lets the surrounding transaction roll back
    /// and keeps the system in a consistent state.
    /// </para>
    /// <para>
    /// If <paramref name="domainEvents"/> is empty, the returned task completes immediately.
    /// </para>
    /// </remarks>
    Task DispatchAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken cancellationToken = default);
}
