namespace KernelR.AspNetCore.Conversions.JsonConverters.ValueObjects;

public sealed partial class SizeJsonConverter
{
    private static Result<Size> ReadStringCore(ref Utf8JsonReader reader)
    {
        if (reader.TokenType != JsonTokenType.String)
        {
            return Result<Size>.Failure(
                new Error(
                    "KernelR.Json.Size.INVALID_TYPE",
                    "Size must be a JSON string.",
                    ErrorType.ValidationError));
        }

        return Size.Create(reader.GetString() ?? string.Empty);
    }

    private static void WriteStringCore(Utf8JsonWriter writer, Size value)
    {
        writer.WriteStringValue($"{value.Value} {value.Unit}");
    }
}
