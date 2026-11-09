using KernelR.Patterns.Serialization;
using System.Text.Json;

namespace KernelR.Patterns.Results;

public static class ResultExtensions
{
    /// <summary>
    /// Returns the success value of the result, or throws a <see cref="FormatException"/> 
    /// containing the serialized <see cref="Error"/> if the result is a failure.
    /// </summary>
    /// <typeparam name="T">The type of the success value.</typeparam>
    /// <param name="result">The result to evaluate.</param>
    /// <returns>The success value.</returns>
    /// <exception cref="FormatException">Thrown when the result is a failure.</exception>
    public static T OrThrowFormatException<T>(this Result<T> result)
    {
        if (result.IsSuccess)
        {
            return result.Value;
        }

        string errorJson = JsonSerializer.Serialize(result.Error, KernelRJsonOptions.Options);
        throw new FormatException(errorJson);
    }
}
