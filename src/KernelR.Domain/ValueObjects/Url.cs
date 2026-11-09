using KernelR.Patterns.Results;
using KernelR.Patterns.Serialization;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace KernelR.Domain.ValueObjects;

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
public readonly record struct Url : IParsable<Url>
{
    /// <summary>
    /// Gets the normalized absolute URL.
    /// </summary>
    public string Value { get; }

    private Url(string value) => Value = value;

    /// <summary>
    /// Creates a new <see cref="Url"/> from the given string.
    /// </summary>
    /// <param name="value">The raw URL. Leading and trailing whitespace is trimmed.</param>
    /// <returns>
    /// A successful result carrying the normalized URL, or a failure with one of:
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

    /// <inheritdoc />
    /// <exception cref="FormatException">
    /// <paramref name="s"/> is <see langword="null"/>, empty, whitespace, not a valid
    /// absolute URI, or uses a scheme other than http/https. The exception message
    /// contains the JSON-serialized domain <see cref="Error"/>.
    /// </exception>
    public static Url Parse(string s, IFormatProvider? provider)
    {
        Result<Url> result = Create(s);
        if (result.IsSuccess)
        {
            return result.Value;
        }

        string errorJson = JsonSerializer.Serialize(result.Error, KernelRJsonOptions.Options);
        throw new FormatException(errorJson);
    }

    /// <inheritdoc />
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

    /// <summary>
    /// Returns the normalized URL as a string.
    /// </summary>
    public override string ToString() => Value;

    /// <summary>
    /// Implicitly converts a <see cref="Url"/> to its normalized string value.
    /// </summary>
    public static implicit operator string(Url url) => url.Value;
}
