namespace KernelR.AspNetCore.Conversions.JsonConverters.ValueObjects;

/// <summary>
/// A <see cref="BaseValueObjectJsonConverter{T}"/> that binds a JSON string to an
/// <see cref="Email"/>.
/// </summary>
/// <remarks>
/// On the wire, an email appears as a plain JSON string such as <c>"user@example.com"</c>.
/// Validation goes through <see cref="Email.Create(string)"/>.
/// </remarks>
public sealed class EmailJsonConverter : BaseValueObjectJsonConverter<Email>
{
    /// <inheritdoc />
    protected override Result<Email> ReadValueCore(ref Utf8JsonReader reader, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.String)
        {
            return Result<Email>.Failure(
                new Error(
                    "KernelR.Json.Email.INVALID_TYPE",
                    "Email must be a JSON string.",
                    ErrorType.ValidationError));
        }

        return Email.Create(reader.GetString() ?? string.Empty);
    }

    /// <inheritdoc />
    protected override void WriteValueCore(Utf8JsonWriter writer, Email email, JsonSerializerOptions options)
    {
        writer.WriteStringValue(email);
    }
}
