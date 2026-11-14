using KernelR.Primitives.Results;
using System.Text.Json.Serialization;

namespace KernelR.Primitives.Serialization;

/// <summary>
/// Source-generated JSON context for KernelR primitives to ensure Native AOT and Trimming compatibility.
/// </summary>
/// <remarks>
/// This context is intentionally minimal: it only knows how to serialize and deserialize
/// <see cref="Error"/>. Other value objects (e.g., <c>Email</c>, <c>Money</c>) are
/// handled by their own converters in the outer layers (e.g., <c>KernelR.AspNetCore</c>),
/// keeping the Primitives layer free of presentation concerns.
/// </remarks>
[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    WriteIndented = false,
    UseStringEnumConverter = false)]
[JsonSerializable(typeof(Error))]
internal partial class KernelRJsonContext : JsonSerializerContext;
