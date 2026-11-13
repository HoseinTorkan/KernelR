using KernelR.Domain.Events;

namespace KernelR.Domain.Abstractions;

/// <summary>
/// Defines a contract for aggregate roots in Domain-Driven Design.
/// An aggregate root is the entry point for accessing and modifying a cluster
/// of related domain objects (the aggregate).
/// </summary>
/// <typeparam name="TId">
/// The type of the aggregate root's identifier.
/// Must be a value type (struct), such as <see cref="Guid"/>, <see cref="long"/>,
/// or a strongly-typed identifier implementing
/// <see cref="StronglyTypedIds.IStronglyTypedId{TSelf, TValue}"/>.
/// </typeparam>
/// <remarks>
/// <para>
/// Only aggregate roots should be referenced directly by repositories.
/// All other entities within the aggregate must be accessed through the
/// aggregate root.
/// </para>
/// <para>
/// Domain events are collected by the aggregate and dispatched by the
/// persistence layer (e.g. <c>DbContext</c>) after the aggregate is saved.
/// This keeps side effects out of the domain model while still allowing the
/// aggregate to announce what happened.
/// </para>
/// <para>
/// <b>Read vs. Write:</b> Reading and clearing events is inherited from
/// <see cref="IHasDomainEvents"/>. Raising events is exposed here, because only
/// aggregate roots may raise domain events.
/// </para>
/// </remarks>
/// <seealso cref="IHasDomainEvents"/>
/// <seealso cref="IEntity{TId}"/>
public interface IAggregateRoot<TId> : IEntity<TId>, IHasDomainEvents
    where TId : struct
{
    /// <summary>
    /// Adds a domain event to be dispatched after the aggregate is persisted.
    /// </summary>
    /// <param name="domainEvent">The event to add. Must not be <see langword="null"/>.</param>
    /// <remarks>
    /// Called by the aggregate's own domain methods (never by external code)
    /// when a state change occurs that other parts of the system should react to.
    /// </remarks>
    void AddDomainEvent(IDomainEvent domainEvent);
}
