namespace KernelR.AspNetCore.Conversions.JsonConverters.ValueObjects;

/// <summary>
/// A <see cref="BaseValueObjectJsonConverter{T}"/> that binds a JSON string to a
/// <see cref="NationalCode"/>.
/// </summary>
/// <remarks>
/// On the wire, a national code appears as a plain JSON string such as <c>"0012345678"</c>.
/// Validation goes through <see cref="NationalCode.Create(string)"/>.
/// </remarks>
public sealed class NationalCodeJsonConverter : BaseValueObjectJsonConverter<NationalCode>
{
    /// <inheritdoc />
    protected override Result<NationalCode> ReadValueCore(ref Utf8JsonReader reader, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.String)
        {
            return Result<NationalCode>.Failure(
                new Error(
                    "KernelR.Json.NationalCode.INVALID_TYPE",
                    "NationalCode must be a JSON string.",
                    ErrorType.ValidationError));
        }

        return NationalCode.Create(reader.GetString() ?? string.Empty);
    }

    /// <inheritdoc />
    protected override void WriteValueCore(Utf8JsonWriter writer, NationalCode nationalCode, JsonSerializerOptions options)
    {
        writer.WriteStringValue(nationalCode);
    }
}
