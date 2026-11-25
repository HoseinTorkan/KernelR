namespace KernelR.Domain.Events;

/// <summary>
/// Provides a base implementation of <see cref="IDomainEvent"/> with an
/// automatically initialized UTC occurrence timestamp.
/// </summary>
/// <remarks>
/// <para>
/// Inherit from this record to avoid repeating the occurrence timestamp
/// initialization in individual domain event implementations.
/// </para>
/// <para>
/// <see cref="OccurredOn"/> is initialized to the current UTC time when
/// the event is created. The property is <see langword="init"/>-only
/// to allow its value to be specified or overridden during initialization,
/// while preventing subsequent modifications.
/// </para>
/// <para>
/// This record provides value-based equality through record semantics.
/// Derived records may include their own data in equality comparisons.
/// Equality does not imply that two instances represent the same
/// occurrence in the domain.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// public sealed record UserCreatedEvent(
///     UserId UserId,
///     Email Email) : DomainEvent;
/// </code>
/// </example>
/// <seealso cref="IDomainEvent"/>
public abstract record DomainEvent : IDomainEvent
{
    /// <inheritdoc />
    public DateTimeOffset OccurredOn { get; init; } = DateTimeOffset.UtcNow;
}
