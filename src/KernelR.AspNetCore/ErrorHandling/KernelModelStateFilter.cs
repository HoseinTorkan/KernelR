using KernelR.AspNetCore.Conversions.JsonConverters;
using KernelR.Patterns.Results;
using KernelR.Patterns.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Text.Json;

namespace KernelR.AspNetCore.ErrorHandling;

/// <summary>
/// Intercepts invalid <see cref="ModelStateDictionary"/> entries (e.g., from failed model binding
/// or FluentValidation) and converts them into a standardized <see cref="ProblemDetails"/> response.
/// </summary>
/// <remarks>
/// <para>
/// Domain value objects (such as <c>PhoneNumber</c>) serialize their <see cref="Error"/>
/// into the model-binding error message as JSON. This filter searches for the first
/// model-state error whose message is a valid serialized <see cref="Error"/> and uses
/// that error to build the response. If no such error exists, a generic validation
/// error is produced.
/// </para>
/// <para>
/// Searching for the first <em>parseable</em> error (rather than simply the first error)
/// ensures that domain errors are not lost when another field's validation error
/// happens to appear earlier in <see cref="ModelStateDictionary"/>.
/// </para>
/// <para>
/// <b>Scope:</b> This filter handles errors produced by <c>TypeConverter</c>
/// (query strings, route values, form data). Errors thrown by <c>JsonConverter</c>
/// (JSON bodies) do <b>not</b> populate <see cref="ModelStateDictionary"/> and are
/// handled separately by <see cref="KernelRExceptionHandler"/>.
/// </para>
/// <para>
/// <b>Serialization contract:</b> Deserialization uses
/// <see cref="ErrorSerialization.Options"/>, the same shared options instance used by
/// the Web-layer <c>BaseValueObjectTypeConverter&lt;T&gt;</c>.
/// </para>
/// </remarks>
public sealed class KernelModelStateFilter : IAlwaysRunResultFilter
{
    private const string FallbackMessage = "One or more validation errors occurred.";

    /// <inheritdoc />
    public void OnResultExecuting(ResultExecutingContext context)
    {
        if (context.ModelState.IsValid)
        {
            return;
        }

        Error error = ResolveError(context.ModelState);
        int statusCode = error.ErrorType.ToHttpStatusCode().ToInt32();

        ProblemDetails problemDetails = new()
        {
            Type = $"https://httpstatuses.io/{statusCode}",
            Title = error.Code,
            Detail = error.Text,
            Status = statusCode,
            Instance = context.HttpContext.Request.Path
        };

        problemDetails.Extensions["traceId"] = context.HttpContext.TraceIdentifier;
        problemDetails.Extensions["timestamp"] = DateTimeOffset.UtcNow;
        problemDetails.Extensions["errorType"] = error.ErrorType.ToString();

        context.Result = new ObjectResult(problemDetails)
        {
            StatusCode = statusCode,
            ContentTypes = { "application/problem+json" }
        };
    }

    /// <inheritdoc />
    public void OnResultExecuted(ResultExecutedContext context)
    {
        // No action is required after the result has been executed.
    }

    /// <summary>
    /// Resolves the domain <see cref="Error"/> to report, preferring the first model-state
    /// error whose message is a valid serialized <see cref="Error"/>.
    /// </summary>
    private static Error ResolveError(ModelStateDictionary modelState)
    {
        foreach (ModelStateEntry entry in modelState.Values)
        {
            foreach (ModelError modelError in entry.Errors)
            {
                if (TryParseDomainError(modelError.ErrorMessage, out Error? parsed) && parsed is not null)
                {
                    return parsed;
                }
            }
        }

        string fallbackText = modelState.Values
            .SelectMany(v => v.Errors)
            .Select(e => e.ErrorMessage)
            .FirstOrDefault(m => !string.IsNullOrWhiteSpace(m))
            ?? FallbackMessage;

        return new Error("KernelR.Validation.InvalidInput", fallbackText, ErrorType.ValidationError);
    }

    /// <summary>
    /// Attempts to deserialize a model-state error message into a domain <see cref="Error"/>.
    /// </summary>
    private static bool TryParseDomainError(string? message, out Error? error)
    {
        error = null;

        if (string.IsNullOrWhiteSpace(message) || message[0] != '{')
        {
            return false;
        }

        try
        {
            Error? parsed = JsonSerializer.Deserialize<Error>(message, ErrorSerialization.Options);

            if (parsed is null || !Enum.IsDefined(parsed.ErrorType))
            {
                return false;
            }

            error = parsed;
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}
