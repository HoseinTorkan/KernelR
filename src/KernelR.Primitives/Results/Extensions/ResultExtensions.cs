using System.Diagnostics.CodeAnalysis;

namespace KernelR.Primitives.Results.Extensions;

/// <summary>
/// Provides extension methods to extract values from <see cref="Result{T}"/> instances.
/// </summary>
public static class ResultExtensions
{
    /// <summary>Returns the success value, or throws a default <see cref="InvalidOperationException"/>.</summary>
    public static T OrThrowInvalidOperationException<T>(this Result<T> result)
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

    /// <summary>Returns the success value, or throws a <see cref="FormatException"/>.</summary>
    public static T OrThrowFormatException<T>(this Result<T> result)
    {
        if (result.IsSuccess) return result.Value;
        throw new FormatException(result.Error.ToJson());
    }

    /// <summary>
    /// Attempts to extract the success value without throwing. Returns <see langword="false"/> on failure.
    /// </summary>
    public static bool TryGetValue<T>(this Result<T> result, [MaybeNullWhen(false)] out T value)
    {
        value = result.IsSuccess ? result.Value : default;
        return result.IsSuccess;
    }

    /// <summary>
    /// Throws a default <see cref="InvalidOperationException"/> for a non-generic failed result.
    /// </summary>
    /// <param name="result">The result to evaluate.</param>
    /// <exception cref="InvalidOperationException">Thrown when the result is a failure.</exception>
    public static void OrThrowInvalidOperationException(this Result result)
    {
        if (result.IsFailure)
            throw new InvalidOperationException(result.Error.ToJson());
    }
}
