namespace KernelR.AspNetCore.ErrorHandling;

/// <summary>
/// Handles unhandled exceptions and converts them into a standardized
/// <see cref="ProblemDetails"/> response.
/// </summary>
public sealed class KernelRExceptionHandler : IExceptionHandler
{
    private const string UnexpectedErrorMessage =
        "An unexpected error occurred while processing your request.";

    /// <inheritdoc />
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        Error error = exception switch
        {
            JsonException jsonException => BuildJsonError(jsonException.Message),
            BadHttpRequestException badRequest => BuildBadRequestError(httpContext, badRequest),
            _ => new Error(
                "KernelR.Infrastructure.UnhandledException",
                UnexpectedErrorMessage,
                ErrorType.Infrastructure)
        };

        int statusCode = error.ErrorType.ToHttpStatusCode().ToInt32();

        ProblemDetails problemDetails = new()
        {
            Type = $"https://httpstatuses.io/{statusCode}",
            Title = error.Code,
            Detail = error.Text,
            Status = statusCode,
            Instance = httpContext.Request.Path
        };

        problemDetails.Extensions["traceId"] = httpContext.TraceIdentifier;
        problemDetails.Extensions["timestamp"] = DateTimeOffset.UtcNow;
        problemDetails.Extensions["errorType"] = error.ErrorType.ToString();

        httpContext.Response.StatusCode = statusCode;
        await httpContext.Response.WriteAsJsonAsync(
            problemDetails,
            options: null,
            contentType: "application/problem+json",
            cancellationToken: cancellationToken);

        return true;
    }

    private static Error BuildBadRequestError(HttpContext context, BadHttpRequestException exception)
    {
        // 👇 اگر InnerException یک JsonException بود، از آن استفاده کن
        if (exception.InnerException is JsonException inner)
        {
            return BuildJsonError(inner.Message);
        }

        // 👇 تلاش برای استخراج Error دقیق از VO
        Error? exactError = BindingErrorExtractor.TryExtract(context, exception.Message);
        if (exactError is not null)
        {
            return exactError;
        }

        // 👇 fallback: خطای عمومی
        return new Error(
            "KernelR.Validation.InvalidInput",
            exception.Message,
            ErrorType.ValidationError);
    }

    private static Error BuildJsonError(string message)
    {
        int separatorIndex = message.IndexOf(": ", StringComparison.Ordinal);

        return separatorIndex > 0
            ? new Error(
                message[..separatorIndex],
                message[(separatorIndex + 2)..],
                ErrorType.ValidationError)
            : new Error(
                "KernelR.Validation.InvalidJson",
                "The request body is not valid JSON or does not match the expected shape.",
                ErrorType.ValidationError);
    }
}
