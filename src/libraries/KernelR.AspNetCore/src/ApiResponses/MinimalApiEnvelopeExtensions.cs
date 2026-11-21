using KernelR.AspNetCore.ApiResponses.Models;
using KernelR.AspNetCore.ErrorHandling;

namespace KernelR.AspNetCore.ApiResponses;

/// <summary>
/// Converts domain <see cref="Result"/> instances into <see cref="IResult"/>
/// values suitable for Minimal API endpoints.
/// </summary>
/// <remarks>
/// <para>
/// On success, the result is wrapped in an <see cref="ApiResponse{T}"/> envelope
/// with HTTP <c>200 OK</c>. On failure, the result is mapped to a standard
/// <see cref="Microsoft.AspNetCore.Mvc.ProblemDetails"/> payload.
/// </para>
/// </remarks>
public static class MinimalApiEnvelopeExtensions
{
    /// <summary>
    /// Converts a <see cref="Result"/> to an <see cref="IResult"/>: <c>200 OK</c>
    /// with an <see cref="ApiResponse"/> on success, or a <see cref="Microsoft.AspNetCore.Mvc.ProblemDetails"/>
    /// on failure.
    /// </summary>
    /// <param name="result">The domain result to convert.</param>
    /// <param name="traceId">
    /// The correlation identifier for the request. If <see langword="null"/> or whitespace,
    /// the current Activity Id is used, or a fresh GUID as a last resort.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="result"/> is <see langword="null"/>.
    /// </exception>
    public static IResult ToHttpResult(this Result result, string? traceId = null)
    {
        ArgumentNullException.ThrowIfNull(result);

        if (result.IsSuccess)
        {
            if (string.IsNullOrWhiteSpace(traceId))
            {
                traceId = ResultConversionExtensions.ResolveTraceId();
            }

            ApiResponse response = ApiResponse.Success() with { TraceId = traceId };
            return Results.Ok(response);
        }

        return BuildProblemDetails(result.Error, traceId);
    }

    /// <summary>
    /// Converts a <see cref="Result{T}"/> to an <see cref="IResult"/>: <c>200 OK</c>
    /// with an <see cref="ApiResponse{T}"/> on success, or a <see cref="Microsoft.AspNetCore.Mvc.ProblemDetails"/>
    /// on failure.
    /// </summary>
    /// <typeparam name="T">The type of the value produced on success.</typeparam>
    /// <param name="result">The domain result to convert.</param>
    /// <param name="traceId">
    /// The correlation identifier for the request. If <see langword="null"/> or whitespace,
    /// the current Activity Id is used, or a fresh GUID as a last resort.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="result"/> is <see langword="null"/>.
    /// </exception>
    public static IResult ToHttpResult<T>(this Result<T> result, string? traceId = null)
    {
        ArgumentNullException.ThrowIfNull(result);

        if (result.IsSuccess)
        {
            if (string.IsNullOrWhiteSpace(traceId))
            {
                traceId = ResultConversionExtensions.ResolveTraceId();
            }

            ApiResponse<T> response = ApiResponse<T>.Success(result.Value) with { TraceId = traceId };
            return Results.Ok(response);
        }

        return BuildProblemDetails(result.Error, traceId);
    }

    /// <summary>
    /// Builds an <see cref="IResult"/> that serializes a <see cref="Microsoft.AspNetCore.Mvc.ProblemDetails"/>
    /// payload for the given domain <paramref name="error"/>.
    /// </summary>
    /// <returns>An <see cref="IResult"/> representing the standard ProblemDetails response.</returns>
    private static IResult BuildProblemDetails(Error error, string? traceId)
    {
        // Use the chained method to get the integer status code.
        int statusCode = error.ErrorType.ToHttpStatusCode().ToInt32();

        if (string.IsNullOrWhiteSpace(traceId))
        {
            traceId = ResultConversionExtensions.ResolveTraceId();
        }

        return Results.Problem(
            type: $"https://httpstatuses.io/{statusCode}",
            title: error.Code,
            detail: error.Text,
            statusCode: statusCode,
            extensions: new Dictionary<string, object?>
            {
                ["traceId"] = traceId,
                ["timestamp"] = DateTimeOffset.UtcNow,
                ["errorType"] = error.ErrorType.ToString()
            });
    }
}
