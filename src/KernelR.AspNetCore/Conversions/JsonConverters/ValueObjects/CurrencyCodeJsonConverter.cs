namespace KernelR.AspNetCore.Conversions.JsonConverters.ValueObjects;

/// <summary>
/// A <see cref="BaseValueObjectJsonConverter{T}"/> that binds a JSON string to a
/// <see cref="CurrencyCode"/>.
/// </summary>
/// <remarks>
/// On the wire, a currency code appears as a plain JSON string such as <c>"IRR"</c>.
/// Validation goes through <see cref="CurrencyCode.Create(string)"/>.
/// </remarks>
public sealed class CurrencyCodeJsonConverter : BaseValueObjectJsonConverter<CurrencyCode>
{
    /// <inheritdoc />
    protected override Result<CurrencyCode> ReadValueCore(ref Utf8JsonReader reader, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.String)
        {
            return Result<CurrencyCode>.Failure(
                new Error(
                    "KernelR.Json.CurrencyCode.INVALID_TYPE",
                    "CurrencyCode must be a JSON string.",
                    ErrorType.ValidationError));
        }

        return CurrencyCode.Create(reader.GetString() ?? string.Empty);
    }

    /// <inheritdoc />
    protected override void WriteValueCore(Utf8JsonWriter writer, CurrencyCode currencyCode, JsonSerializerOptions options)
    {
        writer.WriteStringValue(currencyCode);
    }
}
