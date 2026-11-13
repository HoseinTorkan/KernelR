using KernelR.Primitives.Results;
using System.Text.Json.Serialization;

namespace KernelR.Primitives.Serialization;

/// <summary>
/// Source-generated JSON context for KernelR primitives to ensure Native AOT and Trimming compatibility.
/// </summary>
[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    WriteIndented = false,
    UseStringEnumConverter = false)]
[JsonSerializable(typeof(Error))]
internal partial class KernelRJsonContext : JsonSerializerContext;
