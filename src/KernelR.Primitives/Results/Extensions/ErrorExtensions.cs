using KernelR.Primitives.Serialization;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace KernelR.Primitives.Results.Extensions;

/// <summary>
/// Provides extension methods for the <see cref="Error"/> type.
/// </summary>
public static class ErrorExtensions
{
    /// <summary>
    /// Throws a custom exception created by the <paramref name="exceptionFactory"/>.
    /// </summary>
    /// <typeparam name="TException">The type of the custom exception.</typeparam>
    /// <param name="error">The error to serialize.</param>
    /// <param name="exceptionFactory">A factory that receives the JSON-serialized error and returns the custom exception.</param>
    /// <exception cref="Exception">Always thrown.</exception>
    [DoesNotReturn]
    public static void Throw<TException>(this Error error, Func<string, TException> exceptionFactory)
        where TException : Exception
        => throw exceptionFactory(error.ToJson());

    /// <summary>
    /// Throws a default <see cref="InvalidOperationException"/> containing the serialized error.
    /// </summary>
    /// <param name="error">The error to serialize.</param>
    /// <exception cref="InvalidOperationException">Always thrown.</exception>
    [DoesNotReturn]
    public static void ThrowInvalidOperationException(this Error error)
        => throw new InvalidOperationException(error.ToJson());

    /// <summary>
    /// Serializes the error to a JSON string using the Source-Generated Context (AOT-safe).
    /// </summary>
    /// <param name="error">The error to serialize.</param>
    /// <returns>The JSON-serialized representation of the error.</returns>
    public static string ToJson(this Error error)
        => JsonSerializer.Serialize(error, KernelRJsonContext.Default.Error);
}
