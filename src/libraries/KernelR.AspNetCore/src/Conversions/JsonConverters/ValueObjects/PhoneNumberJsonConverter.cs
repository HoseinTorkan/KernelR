namespace KernelR.AspNetCore.Conversions.JsonConverters.ValueObjects;

/// <summary>
/// A <see cref="BaseValueObjectJsonConverter{T}"/> that binds a JSON string to a
/// <see cref="PhoneNumber"/>.
/// </summary>
/// <remarks>
/// On the wire, a phone number appears as a plain JSON string such as <c>"09121234567"</c>.
/// Validation goes through <see cref="PhoneNumber.Create(string)"/>.
/// </remarks>
public sealed class PhoneNumberJsonConverter : BaseValueObjectJsonConverter<PhoneNumber>
{
    /// <inheritdoc />
    protected override Result<PhoneNumber> ReadValueCore(ref Utf8JsonReader reader, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.String)
        {
            return Result<PhoneNumber>.Failure(
                new Error(
                    "KernelR.Json.PhoneNumber.INVALID_TYPE",
                    "PhoneNumber must be a JSON string.",
                    ErrorType.ValidationError));
        }

        return PhoneNumber.Create(reader.GetString() ?? string.Empty);
    }

    /// <inheritdoc />
    protected override void WriteValueCore(Utf8JsonWriter writer, PhoneNumber phoneNumber, JsonSerializerOptions options)
    {
        writer.WriteStringValue(phoneNumber);
    }
}
