namespace KernelR.AspNetCore.Conversions.JsonConverters.ValueObjects;

public sealed partial class SizeJsonConverter
{
    private static Result<Size> ReadObjectCore(ref Utf8JsonReader reader)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            return Result<Size>.Failure(
                new Error(
                    "KernelR.Json.Size.INVALID_TYPE",
                    "Size must be a JSON object.",
                    ErrorType.ValidationError));
        }

        using JsonDocument doc = JsonDocument.ParseValue(ref reader);
        JsonElement root = doc.RootElement;

        if (!root.TryGetProperty("value", out JsonElement valueElement)
            || !valueElement.TryGetDecimal(out decimal value))
        {
            return Result<Size>.Failure(
                new Error(
                    "KernelR.Json.Size.INVALID_VALUE",
                    "Size.value is missing or not a number.",
                    ErrorType.ValidationError));
        }

        if (!root.TryGetProperty("unit", out JsonElement unitElement)
            || unitElement.ValueKind != JsonValueKind.String)
        {
            return Result<Size>.Failure(
                new Error(
                    "KernelR.Json.Size.INVALID_UNIT",
                    "Size.unit is missing or not a string.",
                    ErrorType.ValidationError));
        }

        string unitText = unitElement.GetString() ?? string.Empty;

        if (!Enum.TryParse(unitText, ignoreCase: true, out SizeUnit unit) || !Enum.IsDefined(unit))
        {
            return Result<Size>.Failure(
                new Error(
                    "KernelR.Json.Size.INVALID_UNIT",
                    $"Size.unit '{unitText}' is not recognized.",
                    ErrorType.ValidationError));
        }

        return Size.Create(value, unit);
    }

    private static void WriteObjectCore(Utf8JsonWriter writer, Size value)
    {
        writer.WriteStartObject();
        writer.WriteNumber("value", value.Value);
        writer.WriteString("unit", value.Unit.ToString());
        writer.WriteEndObject();
    }
}
