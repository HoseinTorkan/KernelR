namespace KernelR.Domain.Events;

/// <summary>
/// Defines a contract for entities that track domain events.
/// </summary>
/// <remarks>
/// <para>
/// This is a marker interface used by the persistence layer (e.g., an EF Core
/// interceptor) to identify entities that have pending domain events, without
/// taking a dependency on <see cref="Abstractions.IAggregateRoot{TId}"/>.
/// </para>
/// <para>
/// <b>Read-only by design:</b> This interface exposes only the <i>reading</i> and
/// <i>clearing</i> of events. Raising new events (via <c>AddDomainEvent</c>) is a
/// write operation that belongs to <see cref="Abstractions.IAggregateRoot{TId}"/>.
/// Plain entities that are not aggregate roots should not raise events directly;
/// they should let their aggregate root do so.
/// </para>
/// </remarks>
/// <seealso cref="Abstractions.IAggregateRoot{TId}"/>
/// <seealso cref="IDomainEvent"/>
public interface IHasDomainEvents
{
    /// <summary>
    /// Gets the collection of domain events raised by this entity.
    /// </summary>
    /// <value>
    /// A read-only collection of pending <see cref="IDomainEvent"/> instances.
    /// Returns an empty collection if no events have been raised.
    /// </value>
    IReadOnlyCollection<IDomainEvent> DomainEvents { get; }

    /// <summary>
    /// Clears all domain events from this entity.
    /// </summary>
    /// <remarks>
    /// Called by the persistence layer after the collected events have been
    /// dispatched, so they are not dispatched again on the next save.
    /// </remarks>
    void ClearDomainEvents();
}
