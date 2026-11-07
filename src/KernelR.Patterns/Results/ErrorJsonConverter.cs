using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace KernelR.Patterns.Results;

/// <summary>
/// A <see cref="JsonConverter{T}"/> for domain <see cref="Error"/> payloads.
/// </summary>
/// <remarks>
/// <para>
/// This converter is an implementation detail used internally by
/// <see cref="ErrorSerialization"/>. It is intentionally <c>internal</c> so that
/// consumers cannot see or override the package's error-handling contract.
/// </para>
/// <para>
/// Other KernelR packages access it via <see cref="InternalsVisibleToAttribute"/>.
/// </para>
/// </remarks>
internal sealed class ErrorJsonConverter : JsonConverter<Error>
{
    /// <inheritdoc />
    /// <exception cref="JsonException">
    /// The JSON payload is missing one of the required properties (<c>code</c>, <c>text</c>,
    /// or <c>errorType</c>), or their values are of the wrong type.
    /// </exception>
    public override Error Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using JsonDocument doc = JsonDocument.ParseValue(ref reader);
        JsonElement root = doc.RootElement;

        if (!root.TryGetProperty("code", out JsonElement codeElement) || codeElement.ValueKind != JsonValueKind.String)
        {
            throw new JsonException("Missing or invalid 'code' property in Error payload.");
        }
        string code = codeElement.GetString()!;

        if (!root.TryGetProperty("text", out JsonElement textElement) || textElement.ValueKind != JsonValueKind.String)
        {
            throw new JsonException("Missing or invalid 'text' property in Error payload.");
        }
        string text = textElement.GetString()!;

        if (!root.TryGetProperty("errorType", out JsonElement errorTypeElement) || errorTypeElement.ValueKind != JsonValueKind.Number)
        {
            throw new JsonException("Missing or invalid 'errorType' property in Error payload.");
        }
        ErrorType errorType = (ErrorType)errorTypeElement.GetInt32();

        return new Error(code, text, errorType);
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, Error error, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteString("code", error.Code);
        writer.WriteString("text", error.Text);
        writer.WriteNumber("errorType", (int)error.ErrorType);
        writer.WriteEndObject();
    }
}

/// <summary>
/// Provides a shared, cached <see cref="JsonSerializerOptions"/> instance for serializing
/// domain <see cref="Error"/> payloads.
/// </summary>
/// <remarks>
/// <para>
/// This class is intentionally non-generic so that a <b>single</b> options instance is
/// shared across all value-object converters, avoiding redundant allocations and cache misses.
/// </para>
/// <para>
/// It is <c>internal</c> so that consumers cannot see or change the package's
/// error-handling contract. Other KernelR packages access it via
/// <see cref="InternalsVisibleToAttribute"/>.
/// </para>
/// </remarks>
internal static class ErrorSerialization
{
    /// <summary>
    /// Gets the shared <see cref="JsonSerializerOptions"/> used to serialize domain errors.
    /// </summary>
    public static JsonSerializerOptions Options { get; } = CreateOptions();

    private static JsonSerializerOptions CreateOptions()
    {
        JsonSerializerOptions options = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };
        options.Converters.Add(new ErrorJsonConverter());
        return options;
    }
}
