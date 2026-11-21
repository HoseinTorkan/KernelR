namespace KernelR.AspNetCore.Conversions.JsonConverters.ValueObjects;

/// <summary>
/// A <see cref="BaseValueObjectJsonConverter{T}"/> that binds a JSON string to a
/// <see cref="Url"/>.
/// </summary>
/// <remarks>
/// On the wire, a URL appears as a plain JSON string such as <c>"https://example.com/path"</c>.
/// Validation goes through <see cref="Url.Create(string)"/>.
/// </remarks>
public sealed class UrlJsonConverter : BaseValueObjectJsonConverter<Url>
{
    /// <inheritdoc />
    protected override Result<Url> ReadValueCore(ref Utf8JsonReader reader, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.String)
        {
            return Result<Url>.Failure(
                new Error(
                    "KernelR.Json.Url.INVALID_TYPE",
                    "Url must be a JSON string.",
                    ErrorType.ValidationError));
        }

        return Url.Create(reader.GetString() ?? string.Empty);
    }

    /// <inheritdoc />
    protected override void WriteValueCore(Utf8JsonWriter writer, Url url, JsonSerializerOptions options)
    {
        writer.WriteStringValue(url);
    }
}
