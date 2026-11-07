using KernelR.Patterns.Results;
using Microsoft.AspNetCore.Http;
using System.Collections.Concurrent;
using System.Reflection;
using System.Text.RegularExpressions;

namespace KernelR.AspNetCore.ErrorHandling;

/// <summary>
/// Extracts the exact domain <see cref="Error"/> from an ASP.NET Core binding-failure
/// message by re-invoking the value object's <c>Create(string)</c> factory.
/// </summary>
/// <remarks>
/// <para>
/// ASP.NET Core raises <see cref="BadHttpRequestException"/> with a message of the form
/// <c>Failed to bind parameter "PhoneNumber phone" from "123".</c>. This class parses
/// that message, locates the <c>Create(string)</c> method on the value-object type, and
/// invokes it to recover the exact <see cref="Error"/>.
/// </para>
/// <para>
/// <b>Performance:</b> The <c>Create</c> method is resolved once per type and cached as a
/// compiled <see cref="Func{T, TResult}"/> delegate, so repeated failures do not pay the
/// Reflection cost.
/// </para>
/// </remarks>
internal static partial class BindingErrorExtractor
{
    /// <summary>
    /// Caches the compiled <c>Create(string)</c> delegate for each value-object type.
    /// </summary>
    private static readonly ConcurrentDictionary<Type, Func<string, object?>?> CreateFuncCache = new();

    /// <summary>
    /// Matches the binding-failure message produced by ASP.NET Core Minimal API.
    /// </summary>
    [GeneratedRegex(@"Failed to bind parameter ""(?<type>.+?) (?<name>.+?)"" from ""(?<value>.+?)""\.")]
    private static partial Regex BindingFailurePattern();

    /// <summary>
    /// Attempts to extract the exact domain <see cref="Error"/> from a binding-failure
    /// message. Returns <see langword="null"/> when the message does not match the
    /// expected pattern or the value object does not expose a <c>Create(string)</c> method.
    /// </summary>
    public static Error? TryExtract(HttpContext context, string message)
    {
        // ۱. پارس کردن پیام
        Match match = BindingFailurePattern().Match(message);
        if (!match.Success)
        {
            return null;
        }

        string parameterName = match.Groups["name"].Value;
        string rawValue = match.Groups["value"].Value;

        // ۲. پیدا کردن پارامتر در endpoint
        Endpoint? endpoint = context.GetEndpoint();
        MethodInfo? method = endpoint?.Metadata.GetMetadata<MethodInfo>();
        ParameterInfo? parameter = method?.GetParameters()
            .FirstOrDefault(p => string.Equals(p.Name, parameterName, StringComparison.OrdinalIgnoreCase));

        if (parameter is null)
        {
            return null;
        }

        // ۳. گرفتن Create از کش (یا ساخت و کش کردن)
        Func<string, object?>? createFunc = CreateFuncCache.GetOrAdd(
            parameter.ParameterType,
            BuildCreateFunc);

        if (createFunc is null)
        {
            return null;
        }

        // ۴. صدا زدن Create (سریع، چون Delegate است)
        object? result = createFunc(rawValue);
        if (result is null)
        {
            return null;
        }

        // ۵. استخراج Error
        PropertyInfo? isFailureProp = result.GetType().GetProperty("IsFailure");
        if (isFailureProp?.GetValue(result) is not true)
        {
            return null;
        }

        PropertyInfo? errorProp = result.GetType().GetProperty("Error");
        return errorProp?.GetValue(result) as Error;
    }

    /// <summary>
    /// Builds a compiled <see cref="Func{T, TResult}"/> delegate for the
    /// <c>Create(string)</c> method on <paramref name="type"/>, or returns
    /// <see langword="null"/> when no such method exists.
    /// </summary>
    private static Func<string, object?>? BuildCreateFunc(Type type)
    {
        MethodInfo? createMethod = type.GetMethod(
            "Create",
            BindingFlags.Public | BindingFlags.Static,
            binder: null,
            types: [typeof(string)],
            modifiers: null);

        if (createMethod is null)
        {
            return null;
        }

        // 👇 تبدیل MethodInfo به Delegate (سریع‌تر از Invoke)
        return (Func<string, object?>)Delegate.CreateDelegate(
            typeof(Func<string, object?>),
            createMethod);
    }
}
