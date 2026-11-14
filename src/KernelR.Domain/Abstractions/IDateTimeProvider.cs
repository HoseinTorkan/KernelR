namespace KernelR.Domain.Abstractions;

/// <summary>
/// Defines a contract for retrieving the current date and time in a testable way.
/// </summary>
/// <remarks>
/// <para>
/// Using this interface instead of directly calling <see cref="DateTimeOffset.UtcNow"/>
/// in the domain model makes the code testable. In unit tests, you can substitute
/// a deterministic implementation that returns a fixed point in time, eliminating
/// flakiness caused by wall-clock time.
/// </para>
/// <para>
/// The implementation is expected to be provided by the infrastructure layer
/// (e.g., returning <see cref="DateTimeOffset.UtcNow"/> in production) and is
/// typically registered as a singleton, because it is stateless.
/// </para>
/// <para>
/// <b>Why <see cref="DateTimeOffset"/> and not <see cref="DateTime"/>?</b>
/// <see cref="DateTimeOffset"/> carries an explicit offset from UTC, so the
/// instant is unambiguous regardless of the storage or display time zone.
/// Comparisons between <see cref="DateTimeOffset"/> values are always correct,
/// whereas <see cref="DateTime"/> comparisons can be ambiguous when
/// <see cref="DateTime.Kind"/> differs (Local vs. Utc vs. Unspecified).
/// </para>
/// </remarks>
/// <example>
/// <code>
/// public sealed class Order
/// {
///     private readonly IDateTimeProvider _timeProvider;
///
///     public Order(IDateTimeProvider timeProvider)
///     {
///         _timeProvider = timeProvider;
///         CreatedAt = timeProvider.UtcNow;
///     }
///
///     public DateTimeOffset CreatedAt { get; }
/// }
/// </code>
/// </example>
public interface IDateTimeProvider
{
    /// <summary>
    /// Gets the current UTC date and time.
    /// </summary>
    /// <value>
    /// A <see cref="DateTimeOffset"/> representing the current instant in UTC.
    /// </value>
    /// <remarks>
    /// Implementations should return a value whose <see cref="DateTimeOffset.Offset"/>
    /// is zero (i.e., UTC). Consumers may assume this and convert to local time
    /// only at the presentation layer.
    /// </remarks>
    DateTimeOffset UtcNow { get; }
}
