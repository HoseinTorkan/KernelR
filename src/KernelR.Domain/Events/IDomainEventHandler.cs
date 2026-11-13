namespace KernelR.Domain.Events;

/// <summary>
/// Defines a handler for a specific type of domain event.
/// </summary>
/// <typeparam name="TEvent">The concrete type of the domain event to handle.</typeparam>
/// <remarks>
/// <para>
/// Handlers are invoked by <see cref="IDomainEventDispatcher"/> after an aggregate root
/// has been successfully persisted. Multiple handlers can be registered for the same event;
/// the dispatcher invokes all of them.
/// </para>
/// <para>
/// <b>Event inheritance:</b> A handler registered for <c>TEvent</c> receives only events
/// whose runtime type is exactly <c>TEvent</c>. A handler for a base event type does
/// <b>not</b> automatically receive derived events. This keeps dispatch predictable
/// and avoids the "shotgun surgery" that comes with polymorphic handling.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// public sealed class OrderPlacedEventHandler : IDomainEventHandler&lt;OrderPlacedEvent&gt;
/// {
///     private readonly IInventoryService _inventory;
///     
///     public OrderPlacedEventHandler(IInventoryService inventory) => _inventory = inventory;
///     
///     public Task HandleAsync(OrderPlacedEvent domainEvent, CancellationToken cancellationToken)
///         => _inventory.ReserveAsync(domainEvent.OrderId, cancellationToken);
/// }
/// </code>
/// </example>
/// <seealso cref="IDomainEvent"/>
/// <seealso cref="IDomainEventDispatcher"/>
public interface IDomainEventHandler<TEvent>
    where TEvent : IDomainEvent
{
    /// <summary>
    /// Handles the specified domain event.
    /// </summary>
    /// <param name="domainEvent">The event to handle. Must not be <see langword="null"/>.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous handling operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="domainEvent"/> is <see langword="null"/>.</exception>
    Task HandleAsync(TEvent domainEvent, CancellationToken cancellationToken = default);
}
