namespace KernelR.AspNetCore.Conversions.JsonConverters.ValueObjects;

/// <summary>
/// A <see cref="BaseValueObjectJsonConverter{T}"/> that binds a JSON string to a
/// <see cref="PersonName"/>.
/// </summary>
/// <remarks>
/// On the wire, a person name appears as a plain JSON string such as <c>"علی"</c>.
/// Validation goes through <see cref="PersonName.Create(string)"/>.
/// </remarks>
public sealed class PersonNameJsonConverter : BaseValueObjectJsonConverter<PersonName>
{
    /// <inheritdoc />
    protected override Result<PersonName> ReadValueCore(ref Utf8JsonReader reader, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.String)
        {
            return Result<PersonName>.Failure(
                new Error(
                    "KernelR.Json.PersonName.INVALID_TYPE",
                    "PersonName must be a JSON string.",
                    ErrorType.ValidationError));
        }

        return PersonName.Create(reader.GetString() ?? string.Empty);
    }

    /// <inheritdoc />
    protected override void WriteValueCore(Utf8JsonWriter writer, PersonName personName, JsonSerializerOptions options)
    {
        writer.WriteStringValue(personName);
    }
}
