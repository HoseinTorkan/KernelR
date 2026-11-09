namespace KernelR.Domain.Events;

/// <summary>
/// Provides a base implementation of <see cref="IDomainEvent"/> that
/// supplies a UTC timestamp (<see cref="OccurredOn"/>) automatically.
/// </summary>
/// <remarks>
/// <para>
/// Inherit from this record to avoid repeating the boilerplate of
/// capturing the occurrence time in every domain event.
/// </para>
/// <para>
/// <see cref="OccurredOn"/> is expressed as a <see cref="DateTimeOffset"/> so
/// that the instant is unambiguous regardless of the storage or display
/// time zone, and so that comparisons between events are always correct.
/// The default value is the current UTC time; the property is
/// <see langword="init"/>-only so tests and replay scenarios can override it
/// while the event remains immutable after construction.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// public sealed record UserCreatedEvent(
///     UserId UserId,
///     PhoneNumber PhoneNumber) : DomainEvent;
/// </code>
/// </example>
/// <seealso cref="IDomainEvent"/>
public abstract record DomainEvent : IDomainEvent
{
    #region ═══ Properties                                                 ═══

    /// <inheritdoc />
    public DateTimeOffset OccurredOn { get; init; } = DateTimeOffset.UtcNow;

    #endregion ═══ Properties                                                 ═══
}
