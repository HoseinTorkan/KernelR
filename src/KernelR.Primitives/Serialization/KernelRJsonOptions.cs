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
    /// <value>A pre-configured <see cref="JsonSerializerOptions"/> instance.</value>
    /// <remarks>
    /// The options are cached in a single static instance to avoid redundant allocations
    /// and to keep a single serialization contract across layers. They are configured
    /// with camelCase naming and a source-generated context for AOT and Trimming safety.
    /// </remarks>
    public static JsonSerializerOptions Options { get; } = CreateOptions();

    /// <summary>
    /// Creates the configured <see cref="JsonSerializerOptions"/>.
    /// </summary>
    /// <returns>The configured options.</returns>
    private static JsonSerializerOptions CreateOptions()
    {
        JsonSerializerOptions options = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = false,
            WriteIndented = false,
            // Link the Source-Generated context for AOT and Trimming safety.
            TypeInfoResolver = KernelRJsonContext.Default
        };

        return options;
    }
}
