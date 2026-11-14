using KernelR.Domain.Abstractions;
using KernelR.Domain.Events;

namespace KernelR.Domain.Entities;

/// <summary>
/// A base class for all aggregate roots, providing identity and domain event management.
/// </summary>
/// <typeparam name="TId">The type of the aggregate root's identifier.</typeparam>
/// <remarks>
/// <para>
/// Aggregate roots are the only entities that repositories should directly reference.
/// All other entities within the aggregate must be accessed through the aggregate root.
/// </para>
/// <para>
/// Domain events are collected by the aggregate and dispatched by the persistence layer
/// (e.g. an EF Core interceptor) after the aggregate is saved. This keeps side effects
/// out of the domain model while still allowing the aggregate to announce what happened.
/// </para>
/// <para>
/// <b>Thread safety:</b> This class is not thread-safe. Aggregate roots are expected to be
/// accessed by a single thread (or a single logical unit of work) at a time.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// public sealed class Order : BaseAggregateRoot&lt;Guid&gt;
/// {
///     public Order(Guid id) : base(id)
///     {
///         AddDomainEvent(new OrderCreatedEvent(id));
///     }
/// }
/// </code>
/// </example>
/// <seealso cref="IAggregateRoot{TId}"/>
/// <seealso cref="BaseEntity{TId}"/>
public abstract class BaseAggregateRoot<TId> : BaseEntity<TId>, IAggregateRoot<TId>
    where TId : struct
{
    private readonly List<IDomainEvent> _domainEvents = [];

    /// <inheritdoc />
    /// <remarks>
    /// The collection is read-only; use <see cref="AddDomainEvent"/> to add events
    /// and <see cref="ClearDomainEvents"/> to clear them.
    /// </remarks>
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
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="domainEvent"/> is <see langword="null"/>.</exception>
    public void AddDomainEvent(IDomainEvent domainEvent)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);
        _domainEvents.Add(domainEvent);
    }

    /// <inheritdoc />
    public void ClearDomainEvents() => _domainEvents.Clear();
}
