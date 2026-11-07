using KernelR.Patterns.Results;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace KernelR.Patterns.Serialization;

/// <summary>
/// A <see cref="JsonConverter{T}"/> for domain <see cref="Error"/> payloads.
/// </summary>
/// <remarks>
/// <para>
/// The converter respects the <see cref="JsonSerializerOptions.PropertyNamingPolicy"/>
/// passed in <c>options</c>, so the same converter works whether the caller uses
/// <c>camelCase</c>, <c>PascalCase</c>, or any other naming policy.
/// </para>
/// <para>
/// This is the recommended behavior for library converters: never hard-code property
/// names; always delegate to <c>options</c> so the converter composes with the caller's
/// configuration.
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

        string codeName = GetPropertyName("Code", options);
        string textName = GetPropertyName("Text", options);
        string errorTypeName = GetPropertyName("ErrorType", options);

        if (!root.TryGetProperty(codeName, out JsonElement codeElement) || codeElement.ValueKind != JsonValueKind.String)
        {
            throw new JsonException($"Missing or invalid '{codeName}' property in Error payload.");
        }
        string code = codeElement.GetString()!;

        if (!root.TryGetProperty(textName, out JsonElement textElement) || textElement.ValueKind != JsonValueKind.String)
        {
            throw new JsonException($"Missing or invalid '{textName}' property in Error payload.");
        }
        string text = textElement.GetString()!;

        if (!root.TryGetProperty(errorTypeName, out JsonElement errorTypeElement) || errorTypeElement.ValueKind != JsonValueKind.Number)
        {
            throw new JsonException($"Missing or invalid '{errorTypeName}' property in Error payload.");
        }
        ErrorType errorType = (ErrorType)errorTypeElement.GetInt32();

        return new Error(code, text, errorType);
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, Error error, JsonSerializerOptions options)
    {
        string codeName = GetPropertyName("Code", options);
        string textName = GetPropertyName("Text", options);
        string errorTypeName = GetPropertyName("ErrorType", options);

        writer.WriteStartObject();
        writer.WriteString(codeName, error.Code);
        writer.WriteString(textName, error.Text);
        writer.WriteNumber(errorTypeName, (int)error.ErrorType);
        writer.WriteEndObject();
    }

    /// <summary>
    /// Resolves the JSON property name for the given CLR name, applying the
    /// <see cref="JsonSerializerOptions.PropertyNamingPolicy"/> if one is set.
    /// </summary>
    /// <param name="clrName">The CLR property name (for example <c>Code</c>).</param>
    /// <param name="options">The active <see cref="JsonSerializerOptions"/>.</param>
    /// <returns>The JSON property name to use.</returns>
    private static string GetPropertyName(string clrName, JsonSerializerOptions options)
        => options.PropertyNamingPolicy?.ConvertName(clrName) ?? clrName;
}
