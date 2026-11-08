using KernelR.Patterns.Results;
using KernelR.Patterns.Serialization;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace KernelR.Domain.ValueObjects.Implementations;

/// <summary>
/// Represents a valid ISO 4217 currency code.
/// </summary>
/// <remarks>
/// <para>
/// The value is always a 3-letter uppercase code. Validation checks the input against
/// the ISO 4217 currency list, so structurally valid but unassigned codes
/// (for example <c>XYZ</c>) are rejected.
/// </para>
/// <para>
/// <b>Iranian Toman:</b> In addition to the official ISO 4217 codes, this type also
/// accepts <c>IRT</c> (Iranian Toman), which is widely used in Iranian applications
/// but is not part of the official ISO 4217 list (only <c>IRR</c> is).
/// </para>
/// <para>
/// <b>Maintenance:</b> The list of accepted codes is maintained internally and must
/// be updated when ISO 4217 changes (which is rare). Consumers cannot extend the list,
/// ensuring that an invalid code can never enter the domain model.
/// </para>
/// </remarks>
public readonly record struct CurrencyCode : IParsable<CurrencyCode>
{
    private static readonly HashSet<string> ValidCodes = new(StringComparer.Ordinal)
    {
        "AED", "AFN", "ALL", "AMD", "ANG", "AOA", "ARS", "AUD", "AWG", "AZN",
        "BAM", "BBD", "BDT", "BGN", "BHD", "BIF", "BMD", "BND", "BOB", "BOV",
        "BRL", "BSD", "BTN", "BWP", "BYN", "BZD", "CAD", "CDF", "CHE", "CHF",
        "CHW", "CLF", "CLP", "CNY", "COP", "COU", "CRC", "CUC", "CUP", "CVE",
        "CZK", "DJF", "DKK", "DOP", "DZD", "EGP", "ERN", "ETB", "EUR", "FJD",
        "FKP", "GBP", "GEL", "GHS", "GIP", "GMD", "GNF", "GTQ", "GYD", "HKD",
        "HNL", "HRK", "HTG", "HUF", "IDR", "ILS", "INR", "IQD", "IRR", "ISK",
        "JMD", "JOD", "JPY", "KES", "KGS", "KHR", "KMF", "KPW", "KRW", "KWD",
        "KYD", "KZT", "LAK", "LBP", "LKR", "LRD", "LSL", "LYD", "MAD", "MDL",
        "MGA", "MKD", "MMK", "MNT", "MOP", "MRU", "MUR", "MVR", "MWK", "MXN",
        "MXV", "MYR", "MZN", "NAD", "NGN", "NIO", "NOK", "NPR", "NZD", "OMR",
        "PAB", "PEN", "PGK", "PHP", "PKR", "PLN", "PYG", "QAR", "RON", "RSD",
        "RUB", "RWF", "SAR", "SBD", "SCR", "SDG", "SEK", "SGD", "SHP", "SLE",
        "SOS", "SRD", "SSP", "STN", "SVC", "SYP", "SZL", "THB", "TJS", "TMT",
        "TND", "TOP", "TRY", "TTD", "TWD", "TZS", "UAH", "UGX", "USD", "USN",
        "UYI", "UYU", "UYW", "UZS", "VED", "VES", "VND", "VUV", "WST", "XAF",
        "XCD", "XOF", "XPF", "YER", "ZAR", "ZMW", "ZWL",
        "IRT"
    };

    /// <summary>
    /// Gets the uppercase 3-letter currency code.
    /// </summary>
    public string Value { get; }

    private CurrencyCode(string value) => Value = value;

    /// <summary>
    /// Creates a new <see cref="CurrencyCode"/> from the given string.
    /// </summary>
    /// <param name="value">
    /// The raw currency code. Case-insensitive; surrounding whitespace is trimmed.
    /// </param>
    /// <returns>
    /// A successful result carrying the normalized code, or a failure with one of:
    /// <list type="bullet">
    ///   <item><description><c>KernelR.ValueObjects.CurrencyCode.EMPTY</c> when the input is null or whitespace.</description></item>
    ///   <item><description><c>KernelR.ValueObjects.CurrencyCode.INVALID</c> when the input is not a recognized ISO 4217 code.</description></item>
    /// </list>
    /// </returns>
    public static Result<CurrencyCode> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Result<CurrencyCode>.Failure(
                new Error(
                    "KernelR.ValueObjects.CurrencyCode.EMPTY",
                    "Currency code cannot be empty.",
                    ErrorType.ValidationError));
        }

        string normalized = value.Trim().ToUpperInvariant();

        if (!ValidCodes.Contains(normalized))
        {
            return Result<CurrencyCode>.Failure(
                new Error(
                    "KernelR.ValueObjects.CurrencyCode.INVALID",
                    "Currency code is not a valid ISO 4217 code.",
                    ErrorType.ValidationError));
        }

        return Result<CurrencyCode>.Success(new CurrencyCode(normalized));
    }

    /// <inheritdoc />
    /// <exception cref="FormatException">
    /// <paramref name="s"/> is <see langword="null"/>, empty, whitespace, or not a
    /// recognized ISO 4217 code. The exception message contains the JSON-serialized
    /// domain <see cref="Error"/>.
    /// </exception>
    public static CurrencyCode Parse(string s, IFormatProvider? provider)
    {
        Result<CurrencyCode> result = Create(s);
        if (result.IsSuccess)
        {
            return result.Value;
        }

        string errorJson = JsonSerializer.Serialize(result.Error, KernelRJsonOptions.Options);
        throw new FormatException(errorJson);
    }

    /// <inheritdoc />
    public static bool TryParse(string? s, IFormatProvider? provider, [MaybeNullWhen(false)] out CurrencyCode result)
    {
        result = default;
        if (string.IsNullOrWhiteSpace(s))
        {
            return false;
        }

        Result<CurrencyCode> createResult = Create(s);
        if (createResult.IsFailure)
        {
            return false;
        }

        result = createResult.Value;
        return true;
    }

    /// <summary>
    /// Returns the currency code as a string.
    /// </summary>
    public override string ToString() => Value;

    /// <summary>
    /// Implicitly converts a <see cref="CurrencyCode"/> to its string value.
    /// </summary>
    public static implicit operator string(CurrencyCode code) => code.Value;
}
