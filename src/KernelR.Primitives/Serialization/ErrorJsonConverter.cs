using KernelR.Primitives.Results;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace KernelR.Primitives.Serialization;

/// <summary>
/// A <see cref="JsonConverter{T}"/> for domain <see cref="Error"/> payloads.
/// </summary>
internal sealed class ErrorJsonConverter : JsonConverter<Error>
{
    public override Error Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using JsonDocument doc = JsonDocument.ParseValue(ref reader);
        JsonElement root = doc.RootElement;

        string codeName = GetPropertyName("Code", options);
        string messageName = GetPropertyName("Message", options);
        string errorTypeName = GetPropertyName("ErrorType", options);

        if (!root.TryGetProperty(codeName, out JsonElement codeElement) || codeElement.ValueKind != JsonValueKind.String)
        {
            throw new JsonException($"Missing or invalid '{codeName}' property in Error payload.");
        }
        string code = codeElement.GetString()!;

        if (!root.TryGetProperty(messageName, out JsonElement messageElement) || messageElement.ValueKind != JsonValueKind.String)
        {
            throw new JsonException($"Missing or invalid '{messageName}' property in Error payload.");
        }
        string message = messageElement.GetString()!;

        if (!root.TryGetProperty(errorTypeName, out JsonElement errorTypeElement) || errorTypeElement.ValueKind != JsonValueKind.Number)
        {
            throw new JsonException($"Missing or invalid '{errorTypeName}' property in Error payload.");
        }
        ErrorType errorType = (ErrorType)errorTypeElement.GetInt32();

        return new Error(code, message, errorType);
    }

    public override void Write(Utf8JsonWriter writer, Error error, JsonSerializerOptions options)
    {
        string codeName = GetPropertyName("Code", options);
        string messageName = GetPropertyName("Message", options);
        string errorTypeName = GetPropertyName("ErrorType", options);

        writer.WriteStartObject();
        writer.WriteString(codeName, error.Code);
        writer.WriteString(messageName, error.Message);
        writer.WriteNumber(errorTypeName, (int)error.ErrorType);
        writer.WriteEndObject();
    }

    private static string GetPropertyName(string clrName, JsonSerializerOptions options)
        => options.PropertyNamingPolicy?.ConvertName(clrName) ?? clrName;
}
