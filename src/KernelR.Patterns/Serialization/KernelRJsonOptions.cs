using KernelR.Patterns.Results;
using System.Text.Json;

namespace KernelR.Patterns.Serialization;

/// <summary>
/// Provides the shared <see cref="JsonSerializerOptions"/> used across all KernelR packages.
/// </summary>
/// <remarks>
/// <para>
/// This class centralizes JSON serialization settings so that all KernelR packages
/// (Domain, Application, AspNetCore) use the same conventions:
/// </para>
/// <list type="bullet">
///   <item><description><b>camelCase</b> property names (consistent with ASP.NET Core).</description></item>
///   <item><description><b>No indentation</b> (compact payloads).</description></item>
///   <item><description><see cref="ErrorJsonConverter"/> registered (for <see cref="Error"/> payloads).</description></item>
/// </list>
/// <para>
/// The options are cached in a single static instance to avoid redundant allocations
/// and to keep a single serialization contract across layers.
/// </para>
/// </remarks>
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
