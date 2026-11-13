using KernelR.Domain.Abstractions;
using KernelR.Domain.Events;

namespace KernelR.Domain.Entities;

/// <summary>
/// A base class for all aggregate roots, providing identity and domain event management.
/// </summary>
/// <typeparam name="TId">The type of the aggregate root's identifier.</typeparam>
public abstract class BaseAggregateRoot<TId> : BaseEntity<TId>, IAggregateRoot<TId>
    where TId : struct
{
    private readonly List<IDomainEvent> _domainEvents = [];

    /// <inheritdoc />
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents;

    /// <summary>
    /// Initializes a new instance of the <see cref="BaseAggregateRoot{TId}"/> class.
    /// </summary>
    /// <param name="id">The unique identifier of the aggregate root.</param>
    protected BaseAggregateRoot(TId id) : base(id) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="BaseAggregateRoot{TId}"/> class.
    /// Required for ORM (e.g., EF Core) to materialize objects from the database.
    /// </summary>
    protected BaseAggregateRoot() { }

    /// <inheritdoc />
    public void AddDomainEvent(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);

    /// <inheritdoc />
    public void ClearDomainEvents() => _domainEvents.Clear();
}
