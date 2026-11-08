namespace KernelR.AspNetCore.Conversions.JsonConverters.ValueObjects;

public sealed partial class DataSizeJsonConverter
{
    private static Result<DataSize> ReadObjectCore(ref Utf8JsonReader reader)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            return Result<DataSize>.Failure(
                new Error(
                    "KernelR.Json.DataSize.INVALID_TYPE",
                    "DataSize must be a JSON object.",
                    ErrorType.ValidationError));
        }

        using JsonDocument doc = JsonDocument.ParseValue(ref reader);
        JsonElement root = doc.RootElement;

        if (!root.TryGetProperty("value", out JsonElement valueElement)
            || !valueElement.TryGetDecimal(out decimal value))
        {
            return Result<DataSize>.Failure(
                new Error(
                    "KernelR.Json.DataSize.INVALID_VALUE",
                    "DataSize.value is missing or not a number.",
                    ErrorType.ValidationError));
        }

        if (!root.TryGetProperty("unit", out JsonElement unitElement)
            || unitElement.ValueKind != JsonValueKind.String)
        {
            return Result<DataSize>.Failure(
                new Error(
                    "KernelR.Json.DataSize.INVALID_UNIT",
                    "DataSize.unit is missing or not a string.",
                    ErrorType.ValidationError));
        }

        string unitText = unitElement.GetString() ?? string.Empty;

        if (!Enum.TryParse(unitText, ignoreCase: true, out DataSizeUnit unit) || !Enum.IsDefined(unit))
        {
            return Result<DataSize>.Failure(
                new Error(
                    "KernelR.Json.DataSize.INVALID_UNIT",
                    $"DataSize.unit '{unitText}' is not recognized.",
                    ErrorType.ValidationError));
        }

        return DataSize.Create(value, unit);
    }

    private static void WriteObjectCore(Utf8JsonWriter writer, DataSize value)
    {
        writer.WriteStartObject();
        writer.WriteNumber("value", value.Value);
        writer.WriteString("unit", value.Unit.ToString());
        writer.WriteEndObject();
    }
}
