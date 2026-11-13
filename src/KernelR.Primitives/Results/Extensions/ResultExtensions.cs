using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace KernelR.Primitives.Results.Extensions;

/// <summary>
/// Provides extension methods to extract values from <see cref="Result{T}"/> or to throw 
/// standard .NET exceptions containing JSON-serialized <see cref="Error"/> details.
/// </summary>
public static class ResultExtensions
{
    /// <summary>Returns the success value, or throws a default <see cref="InvalidOperationException"/>.</summary>
    public static T OrThrow<T>(this Result<T> result)
    {
        if (result.IsSuccess) return result.Value;
        throw new InvalidOperationException(result.Error.ToJson());
    }

    /// <summary>Returns the success value, or throws a custom exception created by the <paramref name="exceptionFactory"/>.</summary>
    public static T OrThrow<T, TException>(this Result<T> result, Func<string, TException> exceptionFactory)
        where TException : Exception
    {
        if (result.IsSuccess) return result.Value;
        throw exceptionFactory(result.Error.ToJson());
    }

    /// <summary>
    /// Throws a default <see cref="InvalidOperationException"/> for an Error.
    /// </summary>
    [DoesNotReturn]
    public static void Throw(this Error error) => throw new InvalidOperationException(error.ToJson());

    /// <summary>
    /// Throws a custom exception created by the <paramref name="exceptionFactory"/> for an Error.
    /// </summary>
    [DoesNotReturn]
    public static void Throw<TException>(this Error error, Func<string, TException> exceptionFactory)
        where TException : Exception => throw exceptionFactory(error.ToJson());

    /// <summary>
    /// Serializes the error to a JSON string for use with custom exceptions.
    /// </summary>
    public static string ToJson(this Error error)
        => JsonSerializer.Serialize(error, Serialization.KernelRJsonOptions.Options);
}
