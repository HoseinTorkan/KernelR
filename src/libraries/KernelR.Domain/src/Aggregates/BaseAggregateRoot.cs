using KernelR.Domain.Abstractions;
using KernelR.Domain.Entities;
using KernelR.Domain.Events;
using System.Collections.ObjectModel;

namespace KernelR.Domain.Aggregates;

/// <summary>
/// Provides a base implementation for aggregate roots with domain event support.
/// </summary>
/// <typeparam name="TId">
/// The type of the aggregate root identifier.
/// </typeparam>
/// <remarks>
/// <para>
/// This class combines entity identity, aggregate root semantics, and domain event
/// collection management.
/// </para>
/// <para>
/// Domain events can be added through <see cref="AddDomainEvent(IDomainEvent)"/>
/// and removed from the collection through <see cref="ClearDomainEvents"/>.
/// This class does not dispatch domain events or determine when they should be
/// dispatched.
/// </para>
/// <para>
/// Derived classes are responsible for implementing aggregate-specific business
/// behavior and maintaining their domain invariants.
/// </para>
/// </remarks>
public abstract class BaseAggregateRoot<TId> :
    BaseEntity<TId>,
    IAggregateRoot<TId>,
    IHasDomainEvents
    where TId : struct
{
    private readonly List<IDomainEvent> _domainEvents = [];

    private readonly ReadOnlyCollection<IDomainEvent> _domainEventsView;

    /// <summary>
    /// Initializes a new instance of the <see cref="BaseAggregateRoot{TId}"/> class.
    /// </summary>
    /// <param name="id">
    /// The identifier of the aggregate root.
    /// </param>
    protected BaseAggregateRoot(TId id) : base(id) => _domainEventsView = _domainEvents.AsReadOnly();

    /// <summary>
    /// Gets the domain events recorded by the aggregate root.
    /// </summary>
    /// <remarks>
    /// The returned collection is a read-only view of the internal domain event
    /// collection. Changes made through <see cref="AddDomainEvent(IDomainEvent)"/>
    /// or <see cref="ClearDomainEvents"/> are reflected in this view.
    /// </remarks>
    public IReadOnlyCollection<IDomainEvent> DomainEvents
        => _domainEventsView;

    /// <summary>
    /// Adds a domain event to the aggregate root.
    /// </summary>
    /// <param name="domainEvent">
    /// The domain event to add.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="domainEvent"/> is <see langword="null"/>.
    /// </exception>
    /// <remarks>
    /// This method records the event but does not dispatch it.
    /// </remarks>
    public void AddDomainEvent(IDomainEvent domainEvent)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);
        _domainEvents.Add(domainEvent);
    }

    /// <summary>
    /// Removes all domain events recorded by the aggregate root.
    /// </summary>
    /// <remarks>
    /// This method clears the internal event collection without dispatching
    /// the events.
    /// </remarks>
    public void ClearDomainEvents() => _domainEvents.Clear();

}
