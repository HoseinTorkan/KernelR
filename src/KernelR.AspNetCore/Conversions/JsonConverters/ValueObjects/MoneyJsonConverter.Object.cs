namespace KernelR.AspNetCore.Conversions.JsonConverters.ValueObjects;

public sealed partial class MoneyJsonConverter
{
    private static Result<Money> ReadObjectCore(ref Utf8JsonReader reader)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            return Result<Money>.Failure(
                new Error(
                    "KernelR.Json.Money.INVALID_TYPE",
                    "Money must be a JSON object.",
                    ErrorType.ValidationError));
        }

        using JsonDocument doc = JsonDocument.ParseValue(ref reader);
        JsonElement root = doc.RootElement;

        if (!root.TryGetProperty("amount", out JsonElement amountElement)
            || !amountElement.TryGetDecimal(out decimal amount))
        {
            return Result<Money>.Failure(
                new Error(
                    "KernelR.Json.Money.INVALID_AMOUNT",
                    "Money.amount is missing or not a number.",
                    ErrorType.ValidationError));
        }

        if (!root.TryGetProperty("currency", out JsonElement currencyElement)
            || currencyElement.ValueKind != JsonValueKind.String)
        {
            return Result<Money>.Failure(
                new Error(
                    "KernelR.Json.Money.INVALID_CURRENCY",
                    "Money.currency is missing or not a string.",
                    ErrorType.ValidationError));
        }

        return Money.Create(amount, currencyElement.GetString() ?? string.Empty);
    }

    private static void WriteObjectCore(Utf8JsonWriter writer, Money value)
    {
        writer.WriteStartObject();
        writer.WriteNumber("amount", value.Amount);
        writer.WriteString("currency", value.Currency);
        writer.WriteEndObject();
    }
}
