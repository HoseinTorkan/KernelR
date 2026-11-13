using System.Text.Json;

namespace KernelR.Primitives.Serialization;

/// <summary>
/// Provides the shared <see cref="JsonSerializerOptions"/> used across all KernelR packages.
/// </summary>
public static class KernelRJsonOptions
{
    /// <summary>
    /// Gets the shared <see cref="JsonSerializerOptions"/> used by all KernelR packages.
    /// </summary>
    public static JsonSerializerOptions Options { get; } = CreateOptions();

    private static JsonSerializerOptions CreateOptions()
    {
        JsonSerializerOptions options = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = false,
            WriteIndented = false
        };
        options.Converters.Add(new ErrorJsonConverter());
        return options;
    }
}
