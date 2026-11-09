using KernelR.Patterns.Results;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace KernelR.Domain.ValueObjects.Temporal;

/// <summary>
/// Represents a contiguous range of time with a start and an end.
/// </summary>
/// <remarks>
/// This value object guarantees that the <see cref="Start"/> is always less than or equal to 
/// the <see cref="End"/>. It provides useful operations like checking if a specific date/time 
/// is within the range, or if two ranges overlap.
/// </remarks>
/// <example>
/// <code>
/// Result&lt;DateTimeRange&gt; result = DateTimeRange.Create("2023-10-01T00:00:00Z/2023-10-31T23:59:59Z");
/// if (result.IsSuccess)
/// {
///     Console.WriteLine(result.Value.Duration.TotalDays);
/// }
/// </code>
/// </example>
public readonly record struct DateTimeRange : IValueObject, IParsable<DateTimeRange>
{
    #region ═══ Constants & Fields                                          ═══

    /// <summary>
    /// The character used to separate the start and end of the time range in string representations.
    /// Follows the ISO 8601 interval standard.
    /// </summary>
    private const char RangeSeparator = '/';

    #endregion ═══ Constants & Fields                                       ═══

    #region ═══ Properties                                                 ═══

    /// <summary>
    /// Gets the start of the time range.
    /// </summary>
    public DateTimeOffset Start { get; }

    /// <summary>
    /// Gets the end of the time range.
    /// </summary>
    public DateTimeOffset End { get; }

    /// <summary>
    /// Gets the total duration of the time range.
    /// </summary>
    public TimeSpan Duration => End - Start;

    #endregion ═══ Properties                                                 ═══

    #region ═══ Constructors & Factories                                   ═══

    /// <summary>
    /// Initializes a new instance of the <see cref="DateTimeRange"/> struct.
    /// </summary>
    private DateTimeRange(DateTimeOffset start, DateTimeOffset end)
    {
        Start = start;
        End = end;
    }

    /// <summary>
    /// Creates a new <see cref="DateTimeRange"/> ensuring the start is before or equal to the end.
    /// </summary>
    /// <param name="start">The start of the range.</param>
    /// <param name="end">The end of the range.</param>
    /// <returns>
    /// A successful result carrying the new <see cref="DateTimeRange"/>, or a failure with 
    /// <c>KernelR.ValueObjects.DateTimeRange.INVALID_RANGE</c> if the start is after the end.
    /// </returns>
    public static Result<DateTimeRange> Create(DateTimeOffset start, DateTimeOffset end)
    {
        if (start > end)
        {
            return Result<DateTimeRange>.Failure(
                new Error(
                    "KernelR.ValueObjects.DateTimeRange.INVALID_RANGE",
                    "The start of the range must be before or equal to the end.",
                    ErrorType.ValidationError));
        }

        return Result<DateTimeRange>.Success(new DateTimeRange(start, end));
    }

    /// <summary>
    /// Creates a new <see cref="DateTimeRange"/> from a string in the ISO 8601 interval format
    /// (e.g., <c>"2023-10-01T00:00:00Z/2023-10-31T23:59:59Z"</c>).
    /// </summary>
    /// <param name="value">The string containing the start and end dates separated by a slash.</param>
    /// <returns>
    /// A successful result carrying the new <see cref="DateTimeRange"/>, or a failure with one of the following error codes:
    /// <list type="bullet">
    ///   <item><description><c>KernelR.ValueObjects.DateTimeRange.EMPTY</c> when the input is null or whitespace.</description></item>
    ///   <item><description><c>KernelR.ValueObjects.DateTimeRange.INVALID_FORMAT</c> when the input does not contain exactly one slash separator or dates are invalid.</description></item>
    ///   <item><description><c>KernelR.ValueObjects.DateTimeRange.INVALID_RANGE</c> when the start is after the end.</description></item>
    /// </list>
    /// </returns>
    public static Result<DateTimeRange> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Result<DateTimeRange>.Failure(
                new Error(
                    "KernelR.ValueObjects.DateTimeRange.EMPTY",
                    "DateTimeRange cannot be empty.",
                    ErrorType.ValidationError));
        }

        string[] parts = value.Split(RangeSeparator, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length != 2)
        {
            return Result<DateTimeRange>.Failure(
                new Error(
                    "KernelR.ValueObjects.DateTimeRange.INVALID_FORMAT",
                    "DateTimeRange must be formatted as '<start>/<end>', for example '2023-10-01T00:00:00Z/2023-10-31T23:59:59Z'.",
                    ErrorType.ValidationError));
        }

        if (!DateTimeOffset.TryParse(parts[0], CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out DateTimeOffset start) ||
            !DateTimeOffset.TryParse(parts[1], CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out DateTimeOffset end))
        {
            return Result<DateTimeRange>.Failure(
                new Error(
                    "KernelR.ValueObjects.DateTimeRange.INVALID_FORMAT",
                    "The start or end date is not a valid ISO 8601 date-time string.",
                    ErrorType.ValidationError));
        }

        return Create(start, end);
    }

    #endregion ═══ Constructors & Factories                                   ═══

    #region ═══ IParsable Implementation                                   ═══

    /// <summary>
    /// Parses the specified string into a <see cref="DateTimeRange"/> instance.
    /// </summary>
    /// <param name="s">The string to parse.</param>
    /// <param name="provider">An object that provides culture-specific formatting information. This parameter is ignored.</param>
    /// <returns>A <see cref="DateTimeRange"/> instance representing the parsed value.</returns>
    /// <exception cref="FormatException">
    /// Thrown when <paramref name="s"/> is invalid. The exception message contains the JSON-serialized domain <see cref="Error"/>.
    /// </exception>
    public static DateTimeRange Parse(string s, IFormatProvider? provider)
    {
        return Create(s).OrThrowFormatException();
    }

    /// <summary>
    /// Attempts to parse the specified string into a <see cref="DateTimeRange"/> instance.
    /// </summary>
    /// <param name="s">The string to parse.</param>
    /// <param name="provider">An object that provides culture-specific formatting information. This parameter is ignored.</param>
    /// <param name="result">When this method returns, contains the parsed <see cref="DateTimeRange"/> instance, if the parsing succeeded, or the default value if it failed.</param>
    /// <returns><see langword="true"/> if the parsing succeeded; otherwise, <see langword="false"/>.</returns>
    public static bool TryParse(string? s, IFormatProvider? provider, [MaybeNullWhen(false)] out DateTimeRange result)
    {
        result = default;
        if (string.IsNullOrWhiteSpace(s))
        {
            return false;
        }

        Result<DateTimeRange> createResult = Create(s);
        if (createResult.IsFailure)
        {
            return false;
        }

        result = createResult.Value;
        return true;
    }

    #endregion ═══ IParsable Implementation                                   ═══

    #region ═══ Functional Operations                                      ═══

    /// <summary>
    /// Determines whether the specified date and time falls within this range.
    /// </summary>
    /// <param name="dateTime">The date and time to check.</param>
    /// <returns><see langword="true"/> if the date and time is within the range; otherwise, <see langword="false"/>.</returns>
    public bool Contains(DateTimeOffset dateTime) => dateTime >= Start && dateTime <= End;

    /// <summary>
    /// Determines whether this range overlaps with another range.
    /// </summary>
    /// <param name="other">The other range to check against.</param>
    /// <returns><see langword="true"/> if the ranges overlap; otherwise, <see langword="false"/>.</returns>
    public bool OverlapsWith(DateTimeRange other) => Start <= other.End && other.Start <= End;

    /// <summary>
    /// Calculates the intersection between this range and another range.
    /// </summary>
    /// <param name="other">The other range to intersect with.</param>
    /// <returns>
    /// A result carrying the intersecting range, or a failure with 
    /// <c>KernelR.ValueObjects.DateTimeRange.NO_OVERLAP</c> if the ranges do not overlap.
    /// </returns>
    public Result<DateTimeRange> IntersectionWith(DateTimeRange other)
    {
        if (!OverlapsWith(other))
        {
            return Result<DateTimeRange>.Failure(
                new Error(
                    "KernelR.ValueObjects.DateTimeRange.NO_OVERLAP",
                    "The two time ranges do not overlap.",
                    ErrorType.ValidationError));
        }

        DateTimeOffset newStart = Start > other.Start ? Start : other.Start;
        DateTimeOffset newEnd = End < other.End ? End : other.End;

        return Create(newStart, newEnd);
    }

    /// <summary>
    /// Returns a new time range shifted by the specified duration.
    /// </summary>
    /// <param name="shift">The duration to shift the range by (e.g., 1 day, 2 hours).</param>
    /// <returns>A new <see cref="DateTimeRange"/> shifted by the specified duration.</returns>
    public DateTimeRange Shift(TimeSpan shift)
    {
        return new DateTimeRange(Start.Add(shift), End.Add(shift));
    }

    #endregion ═══ Functional Operations                                      ═══

    #region ═══ Helpers & Overrides                                        ═══

    /// <summary>
    /// Returns a string representation of the range in ISO 8601 interval format.
    /// </summary>
    /// <returns>A string formatted as <c>"&lt;Start&gt;/&lt;End&gt;"</c>.</returns>
    public override string ToString() => $"{Start:O}/{End:O}";

    #endregion ═══ Helpers & Overrides                                        ═══
}
