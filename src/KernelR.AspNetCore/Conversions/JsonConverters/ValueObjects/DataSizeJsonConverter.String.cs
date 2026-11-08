namespace KernelR.AspNetCore.Conversions.JsonConverters.ValueObjects;

public sealed partial class DataSizeJsonConverter
{
    private static Result<DataSize> ReadStringCore(ref Utf8JsonReader reader)
    {
        if (reader.TokenType != JsonTokenType.String)
        {
            return Result<DataSize>.Failure(
                new Error(
                    "KernelR.Json.DataSize.INVALID_TYPE",
                    "DataSize must be a JSON string.",
                    ErrorType.ValidationError));
        }

        return DataSize.Create(reader.GetString() ?? string.Empty);
    }

    private static void WriteStringCore(Utf8JsonWriter writer, DataSize dataSize)
    {
        writer.WriteStringValue($"{dataSize.Value} {dataSize.Unit}");
    }
}
