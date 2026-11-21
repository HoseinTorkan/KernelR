using Microsoft.CodeAnalysis;

namespace KernelR.Domain.SourceGenerators;

/// <summary>
/// Defines the diagnostic descriptors emitted by the KernelR Source Generators.
/// </summary>
public static class DiagnosticDescriptors
{
    /// <summary>
    /// Emitted when a struct decorated with <c>[StronglyTypedId]</c> is not declared as <c>partial</c>.
    /// </summary>
    public static readonly DiagnosticDescriptor NotPartial = new(
        id: "KRSTID001",
        title: "Strongly-typed ID must be partial",
        messageFormat: "The struct '{0}' decorated with [StronglyTypedId] must be declared as 'partial'",
        category: "KernelR.SourceGenerators",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    /// <summary>
    /// Emitted when a type decorated with <c>[StronglyTypedId]</c> is not a <c>record struct</c>.
    /// </summary>
    public static readonly DiagnosticDescriptor NotRecordStruct = new(
        id: "KRSTID002",
        title: "Strongly-typed ID must be a record struct",
        messageFormat: "The type '{0}' decorated with [StronglyTypedId] must be a 'record struct'",
        category: "KernelR.SourceGenerators",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    /// <summary>
    /// Emitted when a type decorated with <c>[StronglyTypedId]</c> is nested inside another type.
    /// </summary>
    public static readonly DiagnosticDescriptor NestedTypeNotSupported = new(
        id: "KRSTID003",
        title: "Strongly-typed ID must not be a nested type",
        messageFormat: "The struct '{0}' decorated with [StronglyTypedId] must be declared at the namespace level, not nested inside another type",
        category: "KernelR.SourceGenerators",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    /// <summary>
    /// Emitted when the underlying value type does not implement <c>System.IParsable&lt;T&gt;</c>.
    /// </summary>
    public static readonly DiagnosticDescriptor TValueNotParsable = new(
        id: "KRSTID004",
        title: "Underlying value type must implement IParsable",
        messageFormat: "The underlying type '{0}' of '{1}' must implement 'System.IParsable<{0}>' to be used with [StronglyTypedId]",
        category: "KernelR.SourceGenerators",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    /// <summary>
    /// Emitted when the underlying value type does not implement <c>System.ISpanParsable&lt;T&gt;</c>.
    /// </summary>
    public static readonly DiagnosticDescriptor TValueNotSpanParsable = new(
        id: "KRSTID005",
        title: "Underlying value type must implement ISpanParsable",
        messageFormat: "The underlying type '{0}' of '{1}' must implement 'System.ISpanParsable<{0}>' to be used with [StronglyTypedId]",
        category: "KernelR.SourceGenerators",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    /// <summary>
    /// Emitted when the underlying value type is missing a constructor with a single parameter of that type.
    /// </summary>
    public static readonly DiagnosticDescriptor MissingValueConstructor = new(
        id: "KRSTID006",
        title: "Missing value constructor",
        messageFormat: "The struct '{0}' must declare a primary constructor with a single parameter of type '{1}' (e.g., 'record struct {0}({1} Value)')",
        category: "KernelR.SourceGenerators",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);
}
