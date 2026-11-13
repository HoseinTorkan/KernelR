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
}
