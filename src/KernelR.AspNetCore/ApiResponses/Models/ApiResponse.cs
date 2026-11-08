namespace KernelR.AspNetCore.ApiResponses.Models;

/// <summary>
/// Represents the serialized form of a <see cref="Result"/> that is returned
/// from a Web API endpoint to a front-end client <strong>on success</strong>.
/// </summary>
/// <remarks>
/// <para>
/// Instances are created exclusively through <see cref="Success"/> and
/// <see cref="Failure"/>, so the invariant
/// <c>IsSuccess == (Error is null)</c> is always preserved.
/// </para>
/// <para>
/// Deserialization is handled by <see cref="ApiResponseJsonConverter"/>, which
/// reconstructs instances through the same factory methods rather than exposing
/// a public constructor.
/// </para>
/// </remarks>
/// <seealso cref="ApiResponse{T}"/>
/// <seealso cref="ApiResponseJsonConverter"/>
[JsonConverter(typeof(ApiResponseJsonConverter))]
public sealed record ApiResponse
{
    /// <summary>
    /// Gets a value indicating whether the operation succeeded.
    /// </summary>
    [JsonPropertyName("success")]
    [MemberNotNullWhen(false, nameof(Error))]
    public bool IsSuccess { get; }

    /// <summary>
    /// Gets a value indicating whether the operation failed.
    /// </summary>
    [JsonIgnore]
    [MemberNotNullWhen(true, nameof(Error))]
    public bool IsFailure => !IsSuccess;

    /// <summary>
    /// Gets the error describing the failure, or <see langword="null"/> on success.
    /// </summary>
    [JsonPropertyName("error")]
    public Error? Error { get; }

    /// <summary>
    /// Gets the correlation identifier for the request that produced this response.
    /// </summary>
    [JsonPropertyName("traceId")]
    public string? TraceId { get; init; }

    /// <summary>
    /// Gets the UTC timestamp at which this response was produced.
    /// </summary>
    [JsonPropertyName("timestamp")]
    public DateTimeOffset Timestamp { get; init; }

    private ApiResponse(bool isSuccess, Error? error)
    {
        IsSuccess = isSuccess;
        Error = error;
        Timestamp = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Creates a successful response.
    /// </summary>
    public static ApiResponse Success()
        => new(true, null);

    /// <summary>
    /// Creates a failed response carrying the specified <paramref name="error"/>.
    /// </summary>
    public static ApiResponse Failure(Error error)
    {
        ArgumentNullException.ThrowIfNull(error);
        return new(false, error);
    }
}

/// <summary>
/// Represents the serialized form of a <see cref="Result{T}"/> that is returned
/// from a Web API endpoint to a front-end client <strong>on success</strong>.
/// </summary>
/// <typeparam name="T">The type of the value produced on success.</typeparam>
/// <remarks>
/// See <see cref="ApiResponse"/> for the design rationale.
/// </remarks>
/// <seealso cref="ApiResponseJsonConverter{T}"/>
[JsonConverter(typeof(ApiResponseJsonConverter<>))]
public sealed record ApiResponse<T>
{
    /// <summary>
    /// Gets a value indicating whether the operation succeeded.
    /// </summary>
    [JsonPropertyName("success")]
    [MemberNotNullWhen(true, nameof(Value))]
    [MemberNotNullWhen(false, nameof(Error))]
    public bool IsSuccess { get; }

    /// <summary>
    /// Gets a value indicating whether the operation failed.
    /// </summary>
    [JsonIgnore]
    [MemberNotNullWhen(true, nameof(Error))]
    public bool IsFailure => !IsSuccess;

    /// <summary>
    /// Gets the value produced by a successful operation.
    /// </summary>
    [JsonPropertyName("data")]
    public T? Value { get; }

    /// <summary>
    /// Gets the error describing the failure, or <see langword="null"/> on success.
    /// </summary>
    [JsonPropertyName("error")]
    public Error? Error { get; }

    /// <summary>
    /// Gets the correlation identifier for the request that produced this response.
    /// </summary>
    [JsonPropertyName("traceId")]
    public string? TraceId { get; init; }

    /// <summary>
    /// Gets the UTC timestamp at which this response was produced.
    /// </summary>
    [JsonPropertyName("timestamp")]
    public DateTimeOffset Timestamp { get; init; }

    private ApiResponse(bool isSuccess, T? value, Error? error)
    {
        IsSuccess = isSuccess;
        Value = value;
        Error = error;
        Timestamp = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Creates a successful response carrying the specified <paramref name="value"/>.
    /// </summary>
    public static ApiResponse<T> Success(T value)
    {
        if (value is null && default(T) is null)
        {
            throw new ArgumentNullException(
                nameof(value),
                $"ApiResponse<{typeof(T).Name}>.Success requires a non-null value.");
        }

        return new(true, value, null);
    }

    /// <summary>
    /// Creates a failed response carrying the specified <paramref name="error"/>.
    /// </summary>
    public static ApiResponse<T> Failure(Error error)
    {
        ArgumentNullException.ThrowIfNull(error);
        return new(false, default, error);
    }
}
