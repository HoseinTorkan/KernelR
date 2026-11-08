namespace KernelR.AspNetCore.Conversions.JsonConverters.ValueObjects;

/// <summary>
/// A <see cref="BaseValueObjectJsonConverter{T}"/> that binds a JSON value to a
/// <see cref="Money"/>.
/// </summary>
/// <remarks>
/// The wire format is determined by <see cref="ValueObjectJsonFormat"/>:
/// either a JSON object (<c>{ "amount": 100, "currency": "IRR" }</c>) or a plain string
/// (<c>"100 IRR"</c>).
/// </remarks>
/// <param name="format">The wire format to use.</param>
public sealed partial class MoneyJsonConverter(ValueObjectJsonFormat format) : BaseValueObjectJsonConverter<Money>
{
    /// <inheritdoc />
    protected override Result<Money> ReadValueCore(ref Utf8JsonReader reader, JsonSerializerOptions options)
    {
        return format == ValueObjectJsonFormat.Flat
            ? ReadStringCore(ref reader)
            : ReadObjectCore(ref reader);
    }

    /// <inheritdoc />
    protected override void WriteValueCore(Utf8JsonWriter writer, Money value, JsonSerializerOptions options)
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
