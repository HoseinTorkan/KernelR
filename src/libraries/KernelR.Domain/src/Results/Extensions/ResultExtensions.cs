using System.Diagnostics.CodeAnalysis;

namespace KernelR.Domain.Results.Extensions;

/// <summary>
/// Provides extension methods to extract values from <see cref="Result{T}"/> instances.
/// </summary>
public static class ResultExtensions
{
    /// <summary>
    /// Attempts to extract the success value without throwing.
    /// </summary>
    /// <typeparam name="T">The type of the success value.</typeparam>
    /// <param name="result">The result to evaluate.</param>
    /// <param name="value">When this method returns, contains the success value, or <see langword="default"/> on failure.</param>
    /// <returns><see langword="true"/> if the result is successful; otherwise, <see langword="false"/>.</returns>
    public static bool TryGetValue<T>(this Result<T> result, [MaybeNullWhen(false)] out T value)
    {
        value = result.IsSuccess ? result.Value : default;
        return result.IsSuccess;
    }
}
