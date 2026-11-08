namespace KernelR.AspNetCore.Conversions.JsonConverters.ValueObjects;

public sealed partial class MoneyJsonConverter
{
    private static Result<Money> ReadStringCore(ref Utf8JsonReader reader)
    {
        if (reader.TokenType != JsonTokenType.String)
        {
            return Result<Money>.Failure(
                new Error(
                    "KernelR.Json.Money.INVALID_TYPE",
                    "Money must be a JSON string.",
                    ErrorType.ValidationError));
        }

        return Money.Create(reader.GetString() ?? string.Empty);
    }

    private static void WriteStringCore(Utf8JsonWriter writer, Money value)
    {
        writer.WriteStringValue($"{value.Amount} {value.Currency}");
    }
}
