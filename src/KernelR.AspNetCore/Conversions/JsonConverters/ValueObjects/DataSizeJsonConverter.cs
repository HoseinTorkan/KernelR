namespace KernelR.AspNetCore.Conversions.JsonConverters.ValueObjects;

/// <summary>
/// A <see cref="BaseValueObjectJsonConverter{T}"/> that binds a JSON value to a
/// <see cref="DataSize"/>.
/// </summary>
/// <remarks>
/// The wire format is determined by <see cref="ValueObjectJsonFormat"/>:
/// either a JSON object (<c>{ "value": 1.5, "unit": "Gigabyte" }</c>) or a plain string
/// (<c>"1.5 Gigabyte"</c>).
/// </remarks>
/// <param name="format">The wire format to use.</param>
public sealed partial class DataSizeJsonConverter(ValueObjectJsonFormat format) : BaseValueObjectJsonConverter<DataSize>
{
    /// <inheritdoc />
    protected override Result<DataSize> ReadValueCore(ref Utf8JsonReader reader, JsonSerializerOptions options)
    {
        return format == ValueObjectJsonFormat.Flat
            ? ReadStringCore(ref reader)
            : ReadObjectCore(ref reader);
    }

    /// <inheritdoc />
    protected override void WriteValueCore(Utf8JsonWriter writer, DataSize value, JsonSerializerOptions options)
    {
        if (format == ValueObjectJsonFormat.Flat)
        {
            WriteStringCore(writer, value);
        }
        else
        {
            WriteObjectCore(writer, value);
        }
    }
}
