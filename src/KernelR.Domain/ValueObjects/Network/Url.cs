using KernelR.Patterns.Results;
using System.Diagnostics.CodeAnalysis;

namespace KernelR.Domain.ValueObjects.Network;

/// <summary>
/// Represents a valid absolute HTTP or HTTPS URL.
/// </summary>
/// <remarks>
/// <para>
/// This value object guarantees that:
/// </para>
/// <list type="bullet">
///   <item><description>The value is a syntactically valid absolute URI.</description></item>
///   <item><description>The URI scheme is either <c>http</c> or <c>https</c>.</description></item>
///   <item><description>
///     The stored value is the <b>normalized</b> form produced by
///     <see cref="Uri.AbsoluteUri"/>: percent-encoding is applied to unsafe characters,
///     and the scheme and host are lowercased. This guarantees that two URLs differing
///     only in casing or unescaped characters compare equal.
///   </description></item>
/// </list>
/// <para>
/// <b>What this type does not check:</b> Whether the URL points to a specific kind of
/// resource (image, HTML page, API endpoint, etc.). The content type of a URL is only
/// known after a request and is a concern of the application layer, not of this value
/// object. Use property names (for example <c>AvatarUrl</c>, <c>WebsiteUrl</c>) to
/// express intent.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// Result&lt;Url&gt; result = Url.Create("https://example.com/path");
/// if (result.IsSuccess)
/// {
///     Console.WriteLine(result.Value); // Outputs: https://example.com/path
///     Console.WriteLine(result.Value.Host); // Outputs: example.com
/// }
/// </code>
/// </example>
public readonly record struct Url :
    IValueObject,
    IParsable<Url>
{
    #region ═══ Properties                                                 ═══

    /// <summary>
    /// Gets the normalized absolute URL.
    /// </summary>
    /// <value>A string containing the normalized URL.</value>
    public string Value { get; }

    /// <summary>
    /// Gets a value indicating whether the URL uses the secure HTTPS scheme.
    /// </summary>
    /// <value><see langword="true"/> if the URL starts with "https://"; otherwise, <see langword="false"/>.</value>
    public bool IsSecure => Value.StartsWith("https://", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Gets the host (domain) component of the URL.
    /// </summary>
    /// <value>The domain name (e.g., "google.com") extracted from the URL.</value>
    /// <remarks>
    /// Since the <see cref="Value"/> is guaranteed to be a valid absolute URI (validated in <see cref="Create"/>), 
    /// this property will never throw an exception.
    /// </remarks>
    public string Host => new Uri(Value).Host;

    #endregion ═══ Properties                                                 ═══

    #region ═══ Constructors & Factories                                   ═══

    /// <summary>
    /// Initializes a new instance of the <see cref="Url"/> struct.
    /// </summary>
    /// <param name="value">The normalized URL.</param>
    private Url(string value) => Value = value;

    /// <summary>
    /// Creates a new <see cref="Url"/> from the given string.
    /// </summary>
    /// <param name="value">The raw URL. Leading and trailing whitespace is trimmed.</param>
    /// <returns>
    /// A successful result carrying the normalized URL, or a failure with one of the following error codes:
    /// <list type="bullet">
    ///   <item><description><c>KernelR.ValueObjects.Url.EMPTY</c> when the input is null or whitespace.</description></item>
    ///   <item><description><c>KernelR.ValueObjects.Url.INVALID_FORMAT</c> when the input is not a valid absolute URI.</description></item>
    ///   <item><description><c>KernelR.ValueObjects.Url.INVALID_SCHEME</c> when the scheme is not <c>http</c> or <c>https</c>.</description></item>
    /// </list>
    /// </returns>
    public static Result<Url> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Result<Url>.Failure(
                new Error(
                    "KernelR.ValueObjects.Url.EMPTY",
                    "URL cannot be empty.",
                    ErrorType.ValidationError));
        }

        string trimmed = value.Trim();

        if (!Uri.TryCreate(trimmed, UriKind.Absolute, out Uri? uri))
        {
            return Result<Url>.Failure(
                new Error(
                    "KernelR.ValueObjects.Url.INVALID_FORMAT",
                    "URL is not a valid absolute URI.",
                    ErrorType.ValidationError));
        }

        if (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
        {
            return Result<Url>.Failure(
                new Error(
                    "KernelR.ValueObjects.Url.INVALID_SCHEME",
                    "URL must use http or https.",
                    ErrorType.ValidationError));
        }

        return Result<Url>.Success(new Url(uri.AbsoluteUri));
    }

    #endregion ═══ Constructors & Factories                                   ═══

    #region ═══ IParsable Implementation                                   ═══

    /// <summary>
    /// Parses the specified string into a <see cref="Url"/> instance.
    /// </summary>
    /// <param name="s">The string to parse.</param>
    /// <param name="provider">An object that provides culture-specific formatting information. This parameter is ignored.</param>
    /// <returns>A <see cref="Url"/> instance representing the parsed value.</returns>
    /// <exception cref="FormatException">
    /// Thrown when <paramref name="s"/> is null, empty, whitespace, not a valid absolute URI, 
    /// or uses a scheme other than http/https. The exception message contains the JSON-serialized domain <see cref="Error"/>.
    /// </exception>
    public static Url Parse(string s, IFormatProvider? provider)
    {
        return Create(s).OrThrowFormatException();
    }

    /// <summary>
    /// Attempts to parse the specified string into a <see cref="Url"/> instance.
    /// </summary>
    /// <param name="s">The string to parse.</param>
    /// <param name="provider">An object that provides culture-specific formatting information. This parameter is ignored.</param>
    /// <param name="result">When this method returns, contains the parsed <see cref="Url"/> instance, if the parsing succeeded, or the default value if it failed.</param>
    /// <returns><see langword="true"/> if the parsing succeeded; otherwise, <see langword="false"/>.</returns>
    public static bool TryParse(string? s, IFormatProvider? provider, [MaybeNullWhen(false)] out Url result)
    {
        result = default;
        if (string.IsNullOrWhiteSpace(s))
        {
            return false;
        }

        Result<Url> createResult = Create(s);
        if (createResult.IsFailure)
        {
            return false;
        }

        result = createResult.Value;
        return true;
    }

    #endregion ═══ IParsable Implementation                                   ═══

    #region ═══ Helpers & Overrides                                        ═══

    /// <summary>
    /// Returns the normalized URL as a string.
    /// </summary>
    /// <returns>The normalized absolute URL.</returns>
    public override string ToString() => Value;

    /// <summary>
    /// Explicitly converts a <see cref="Url"/> to its normalized string value.
    /// </summary>
    /// <param name="url">The URL instance to convert.</param>
    /// <returns>The normalized string representation of the URL.</returns>
    public static explicit operator string(Url url) => url.Value;

    #endregion ═══ Helpers & Overrides                                        ═══
}
