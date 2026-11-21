namespace KernelR.AspNetCore.Conversions.JsonConverters;

/// <summary>
/// Provides a base <see cref="JsonConverter{T}"/> for KernelAbstractions value objects,
/// handling common error-to-exception conversion while allowing derived types to define
/// their specific JSON representation.
/// </summary>
/// <typeparam name="T">The value object type. Must be a value type (struct).</typeparam>
/// <remarks>
/// Derived converters implement <see cref="ReadValueCore"/> and <see cref="WriteValueCore"/>
/// to support any JSON shape (string, object, number, etc.) without redundant boilerplate.
/// </remarks>
public abstract class BaseValueObjectJsonConverter<T> : JsonConverter<T>
    where T : struct
{
    /// <summary>
    /// Reads the value object from the current JSON reader position.
    /// </summary>
    /// <param name="reader">The current <see cref="Utf8JsonReader"/>.</param>
    /// <param name="options">The active <see cref="JsonSerializerOptions"/>.</param>
    /// <returns>
    /// A successful result carrying the value object, or a failure carrying the domain error.
    /// </returns>
    protected abstract Result<T> ReadValueCore(ref Utf8JsonReader reader, JsonSerializerOptions options);

    /// <summary>
    /// Writes the value object to the given writer.
    /// </summary>
    /// <param name="writer">The target <see cref="Utf8JsonWriter"/>.</param>
    /// <param name="value">The value object to write.</param>
    /// <param name="options">The active <see cref="JsonSerializerOptions"/>.</param>
    protected abstract void WriteValueCore(Utf8JsonWriter writer, T value, JsonSerializerOptions options);

    /// <inheritdoc />
    /// <exception cref="JsonException">
    /// <see cref="ReadValueCore"/> returned a failure. The exception message contains the
    /// JSON-serialized <see cref="Error"/>.
    /// </exception>
    public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        Result<T> result = ReadValueCore(ref reader, options);

        if (result.IsFailure)
        {
            string errorJson = JsonSerializer.Serialize(result.Error, KernelRJsonOptions.Options);
            throw new JsonException(errorJson);
        }

        return result.Value;
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        WriteValueCore(writer, value, options);
    }
}
