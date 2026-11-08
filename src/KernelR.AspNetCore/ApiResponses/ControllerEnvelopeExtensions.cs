using KernelR.AspNetCore.ApiResponses.Models;
using KernelR.AspNetCore.ErrorHandling;

namespace KernelR.AspNetCore.ApiResponses;

/// <summary>
/// Converts domain <see cref="Result"/> instances into <see cref="IActionResult"/>
/// values for MVC controllers.
/// </summary>
/// <remarks>
/// <para>
/// On success, the result is wrapped in an <see cref="ApiResponse{T}"/> envelope
/// with HTTP <c>200 OK</c>. On failure, the result is mapped to a standard
/// <see cref="ProblemDetails"/> payload.
/// </para>
/// </remarks>
public static class ControllerEnvelopeExtensions
{
    /// <summary>
    /// Wraps a non-generic <see cref="Result"/> in an <see cref="ApiResponse"/>
    /// on success, or a <see cref="ProblemDetails"/> on failure.
    /// </summary>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="controller"/> or <paramref name="result"/> is <see langword="null"/>.
    /// </exception>
    public static IActionResult EnvelopeResult(this ControllerBase controller, Result result)
    {
        ArgumentNullException.ThrowIfNull(controller);
        ArgumentNullException.ThrowIfNull(result);

        if (result.IsFailure)
        {
            return BuildProblemDetails(controller, result.Error);
        }

        ApiResponse response = result.ToApiResponse(controller.HttpContext?.TraceIdentifier);
        return new ObjectResult(response) { StatusCode = StatusCodes.Status200OK };
    }

    /// <summary>
    /// Wraps a generic <see cref="Result{T}"/> in an <see cref="ApiResponse{T}"/>
    /// on success, or a <see cref="ProblemDetails"/> on failure.
    /// </summary>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="controller"/> or <paramref name="result"/> is <see langword="null"/>.
    /// </exception>
    public static IActionResult EnvelopeResult<T>(this ControllerBase controller, Result<T> result)
    {
        ArgumentNullException.ThrowIfNull(controller);
        ArgumentNullException.ThrowIfNull(result);

        if (result.IsFailure)
        {
            return BuildProblemDetails(controller, result.Error);
        }

        ApiResponse<T> response = result.ToApiResponse(controller.HttpContext?.TraceIdentifier);
        return new ObjectResult(response) { StatusCode = StatusCodes.Status200OK };
    }

    /// <summary>
    /// Builds a <see cref="ProblemDetails"/>-shaped <see cref="ObjectResult"/>
    /// from a domain <see cref="Error"/>.
    /// </summary>
    private static ObjectResult BuildProblemDetails(ControllerBase controller, Error error)
    {
        // Use the chained method to get the integer status code.
        int statusCode = error.ErrorType.ToHttpStatusCode().ToInt32();

        string traceId = controller.HttpContext?.TraceIdentifier ?? ResultConversionExtensions.ResolveTraceId();

        ProblemDetails problem = new()
        {
            Type = $"https://httpstatuses.io/{statusCode}",
            Title = error.Code,
            Detail = error.Text,
            Status = statusCode,
            Instance = controller.HttpContext?.Request.Path
        };

        problem.Extensions["traceId"] = traceId;
        problem.Extensions["timestamp"] = DateTimeOffset.UtcNow;
        problem.Extensions["errorType"] = error.ErrorType.ToString();

        return new ObjectResult(problem)
        {
            StatusCode = statusCode,
            ContentTypes = { "application/problem+json" }
        };
    }
}
