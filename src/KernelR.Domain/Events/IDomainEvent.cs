using KernelR.Domain.Abstractions;

namespace KernelR.Domain.Events;

/// <summary>
/// Defines a contract for domain events in Domain-Driven Design.
/// A domain event represents something meaningful that happened in the domain.
/// </summary>
/// <remarks>
/// <para>
/// Domain events are typically raised by aggregate roots and dispatched after
/// the aggregate has been successfully persisted.
/// </para>
/// <para>
/// <see cref="OccurredOn"/> is expressed as a <see cref="DateTimeOffset"/> so
/// that the instant is unambiguous regardless of the storage or display
/// time zone, and so that events can be ordered reliably.
/// The property is read-only on the interface so implementations are free to
/// use <c>init</c> or a private setter; the only contract is that the value is
/// available after construction.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// public sealed record OrderCreatedEvent(OrderId OrderId, UserId CustomerId) : DomainEvent
/// {
/// }
/// </code>
/// </example>
/// <seealso cref="IAggregateRoot{TId}"/>
/// <seealso cref="DomainEvent"/>
public interface IDomainEvent
{
    /// <summary>
    /// Gets the moment when this event occurred.
    /// </summary>
    /// <remarks>
    /// Implementations should persist this value in UTC.
    /// </remarks>
    DateTimeOffset OccurredOn { get; }
}
