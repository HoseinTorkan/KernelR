namespace KernelR.Domain.Events;

/// <summary>
/// Defines a contract for domain events in Domain-Driven Design.
/// A domain event represents something meaningful that happened in the domain.
/// </summary>
/// <remarks>
/// <para>
/// Domain events are typically raised by aggregate roots to represent
/// significant changes in the domain.
/// </para>
/// <para>
/// <see cref="OccurredOn"/> represents the instant when the event occurred.
/// Using <see cref="DateTimeOffset"/> makes the timestamp unambiguous
/// regardless of the time zone in which it is stored or displayed.
/// Implementations should use UTC for this value.
/// </para>
/// <para>
/// The timestamp alone does not guarantee a strict ordering of events.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// public sealed record OrderCreatedEvent(OrderId OrderId, UserId CustomerId) : DomainEvent
/// {
/// }
/// </code>
/// </example>
/// <seealso cref="DomainEvent"/>
/// <seealso cref="KernelR.Domain.Abstractions.IAggregateRoot{TId}"/>
public interface IDomainEvent
{
    /// <summary>
    /// Gets the instant when this event occurred.
    /// </summary>
    /// <value>
    /// A <see cref="DateTimeOffset"/> representing the event occurrence time.
    /// </value>
    DateTimeOffset OccurredOn { get; }
}
