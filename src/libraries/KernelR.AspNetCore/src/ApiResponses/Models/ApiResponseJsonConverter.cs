namespace KernelR.AspNetCore.ApiResponses.Models;

/// <summary>
/// A <see cref="JsonConverter{T}"/> for <see cref="ApiResponse"/> that reconstructs
/// instances through <see cref="ApiResponse.Success"/> and <see cref="ApiResponse.Failure"/>,
/// preserving the record's invariant (<c>IsSuccess == (Error is null)</c>).
/// </summary>
/// <remarks>
/// <para>
/// Using a dedicated converter instead of a public <c>[JsonConstructor]</c> keeps
/// <see cref="ApiResponse"/> constructible only through its factory methods, so no
/// caller can create an invalid instance (e.g. <c>IsSuccess = false, Error = null</c>).
/// </para>
/// </remarks>
public sealed class ApiResponseJsonConverter : JsonConverter<ApiResponse>
{
    /// <inheritdoc />
    public override ApiResponse Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        using JsonDocument doc = JsonDocument.ParseValue(ref reader);
        JsonElement root = doc.RootElement;

        bool isSuccess = root.GetProperty("success").GetBoolean();
        string? traceId = root.TryGetProperty("traceId", out JsonElement traceIdElement)
            && traceIdElement.ValueKind == JsonValueKind.String
            ? traceIdElement.GetString()
            : null;
        DateTimeOffset timestamp = root.TryGetProperty("timestamp", out JsonElement timestampElement)
            && timestampElement.TryGetDateTimeOffset(out DateTimeOffset parsedTimestamp)
            ? parsedTimestamp
            : DateTimeOffset.UtcNow;

        if (isSuccess)
        {
            return ApiResponse.Success() with { TraceId = traceId, Timestamp = timestamp };
        }

        Error error = ReadError(root);
        return ApiResponse.Failure(error) with { TraceId = traceId, Timestamp = timestamp };
    }

    /// <inheritdoc />
    public override void Write(
        Utf8JsonWriter writer,
        ApiResponse value,
        JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteBoolean("success", value.IsSuccess);
        writer.WriteString("traceId", value.TraceId);
        writer.WriteString("timestamp", value.Timestamp);

        if (value.Error is not null)
        {
            writer.WritePropertyName("error");
            JsonSerializer.Serialize(writer, value.Error, options);
        }

        writer.WriteEndObject();
    }

    private static Error ReadError(JsonElement root)
    {
        if (!root.TryGetProperty("error", out JsonElement errorElement)
            || errorElement.ValueKind == JsonValueKind.Null)
        {
            throw new JsonException(
                "ApiResponse has success=false but no 'error' property. " +
                "This violates the ApiResponse invariant.");
        }

        Error? error = errorElement.Deserialize<Error>(DomainErrorJson.Options);
        return error ?? throw new JsonException("Failed to deserialize ApiResponse.Error.");
    }
}

/// <summary>
/// A <see cref="JsonConverter{T}"/> for <see cref="ApiResponse{T}"/> that reconstructs
/// instances through <see cref="ApiResponse{T}.Success"/> and
/// <see cref="ApiResponse{T}.Failure"/>, preserving the record's invariant.
/// </summary>
/// <typeparam name="T">The type of the value produced on success.</typeparam>
public sealed class ApiResponseJsonConverter<T> : JsonConverter<ApiResponse<T>>
{
    /// <inheritdoc />
    public override ApiResponse<T> Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        using JsonDocument doc = JsonDocument.ParseValue(ref reader);
        JsonElement root = doc.RootElement;

        bool isSuccess = root.GetProperty("success").GetBoolean();
        string? traceId = root.TryGetProperty("traceId", out JsonElement traceIdElement)
            && traceIdElement.ValueKind == JsonValueKind.String
            ? traceIdElement.GetString()
            : null;
        DateTimeOffset timestamp = root.TryGetProperty("timestamp", out JsonElement timestampElement)
            && timestampElement.TryGetDateTimeOffset(out DateTimeOffset parsedTimestamp)
            ? parsedTimestamp
            : DateTimeOffset.UtcNow;

        if (isSuccess)
        {
            T? value = root.TryGetProperty("data", out JsonElement dataElement)
                ? dataElement.Deserialize<T>(options)
                : default;

            if (value is null && default(T) is null)
            {
                throw new JsonException(
                    $"ApiResponse<{typeof(T).Name}> has success=true but 'data' is missing.");
            }

            return ApiResponse<T>.Success(value!) with { TraceId = traceId, Timestamp = timestamp };
        }

        Error error = ReadError(root);
        return ApiResponse<T>.Failure(error) with { TraceId = traceId, Timestamp = timestamp };
    }

    /// <inheritdoc />
    public override void Write(
        Utf8JsonWriter writer,
        ApiResponse<T> value,
        JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteBoolean("success", value.IsSuccess);
        writer.WriteString("traceId", value.TraceId);
        writer.WriteString("timestamp", value.Timestamp);

        if (value.IsSuccess)
        {
            writer.WritePropertyName("data");
            JsonSerializer.Serialize(writer, value.Value, options);
        }
        else if (value.Error is not null)
        {
            writer.WritePropertyName("error");
            JsonSerializer.Serialize(writer, value.Error, options);
        }

        writer.WriteEndObject();
    }

    private static Error ReadError(JsonElement root)
    {
        if (!root.TryGetProperty("error", out JsonElement errorElement)
            || errorElement.ValueKind == JsonValueKind.Null)
        {
            throw new JsonException(
                "ApiResponse has success=false but no 'error' property. " +
                "This violates the ApiResponse invariant.");
        }

        Error? error = errorElement.Deserialize<Error>(DomainErrorJson.Options);
        return error ?? throw new JsonException("Failed to deserialize ApiResponse.Error.");
    }
}
