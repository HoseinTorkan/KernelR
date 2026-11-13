using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Text;

namespace KernelR.Domain.SourceGenerators;

/// <summary>
/// An incremental source generator that implements the boilerplate for strongly-typed identifiers.
/// </summary>
[Generator(LanguageNames.CSharp)]
public sealed class StronglyTypedIdGenerator : IIncrementalGenerator
{
    private const string AttributeName = "KernelR.Domain.StronglyTypedIds.StronglyTypedIdAttribute`1";
    private const string IParsableName = "System.IParsable`1";
    private const string ISpanParsableName = "System.ISpanParsable`1";

    /// <inheritdoc />
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        IncrementalValuesProvider<Target> targets = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                AttributeName,
                predicate: static (node, _) => node is TypeDeclarationSyntax,
                transform: static (ctx, _) => GetTarget(ctx))
            .Where(static m => m is not null)!;

        context.RegisterSourceOutput(targets, static (spc, source) => Execute(source, spc));
    }

    private static Target GetTarget(GeneratorAttributeSyntaxContext context)
    {
        INamedTypeSymbol structSymbol = (INamedTypeSymbol)context.TargetSymbol;
        AttributeData attr = context.Attributes[0];
        ITypeSymbol tValueType = attr.AttributeClass!.TypeArguments[0];

        bool useSequentialGuid = true;
        bool allowEmpty = false;

        foreach (KeyValuePair<string, TypedConstant> namedArgument in attr.NamedArguments)
        {
            if (namedArgument.Key == "UseSequentialGuid" && namedArgument.Value.Value is bool useSeq)
                useSequentialGuid = useSeq;
            if (namedArgument.Key == "AllowEmpty" && namedArgument.Value.Value is bool allow)
                allowEmpty = allow;
        }

        return new Target(structSymbol, tValueType, useSequentialGuid, allowEmpty, context.SemanticModel.Compilation);
    }

    private static void Execute(Target target, SourceProductionContext context)
    {
        INamedTypeSymbol structSymbol = target.StructSymbol;
        ITypeSymbol tValueType = target.TValueType;

        // 1. Must be a record struct
        if (!structSymbol.IsRecord || structSymbol.TypeKind != TypeKind.Struct)
        {
            context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.NotRecordStruct, structSymbol.Locations.FirstOrDefault(), structSymbol.Name));
            return;
        }

        // 2. Must be partial
        bool isPartial = structSymbol.DeclaringSyntaxReferences
            .Select(r => r.GetSyntax())
            .OfType<TypeDeclarationSyntax>()
            .Any(s => s.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword)));

        if (!isPartial)
        {
            context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.NotPartial, structSymbol.Locations.FirstOrDefault(), structSymbol.Name));
            return;
        }

        // 3. Must NOT be nested
        if (structSymbol.ContainingType is not null)
        {
            context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.NestedTypeNotSupported, structSymbol.Locations.FirstOrDefault(), structSymbol.Name));
            return;
        }

        // 4. Must have a constructor with a single parameter of type TValue
        bool hasValueConstructor = structSymbol.InstanceConstructors
            .Any(c => c.Parameters.Length == 1
                && SymbolEqualityComparer.Default.Equals(c.Parameters[0].Type, tValueType));

        if (!hasValueConstructor)
        {
            context.ReportDiagnostic(Diagnostic.Create(
                DiagnosticDescriptors.MissingValueConstructor,
                structSymbol.Locations.FirstOrDefault(),
                structSymbol.Name,
                tValueType.ToDisplayString()));
            return;
        }

        // 5. TValue must implement IParsable<TValue>
        INamedTypeSymbol? parsableInterface = target.Compilation.GetTypeByMetadataName(IParsableName);
        if (parsableInterface is not null)
        {
            INamedTypeSymbol constructedParsable = parsableInterface.Construct(tValueType);
            bool implementsParsable = tValueType.AllInterfaces.Any(i => SymbolEqualityComparer.Default.Equals(i, constructedParsable));

            if (!implementsParsable)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    DiagnosticDescriptors.TValueNotParsable,
                    structSymbol.Locations.FirstOrDefault(),
                    tValueType.ToDisplayString(),
                    structSymbol.Name));
                return;
            }
        }

        // 6. TValue must implement ISpanParsable<TValue>
        INamedTypeSymbol? spanParsableInterface = target.Compilation.GetTypeByMetadataName(ISpanParsableName);
        if (spanParsableInterface is not null)
        {
            INamedTypeSymbol constructedSpanParsable = spanParsableInterface.Construct(tValueType);
            bool implementsSpanParsable = tValueType.AllInterfaces.Any(i => SymbolEqualityComparer.Default.Equals(i, constructedSpanParsable));

            if (!implementsSpanParsable)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    DiagnosticDescriptors.TValueNotSpanParsable,
                    structSymbol.Locations.FirstOrDefault(),
                    tValueType.ToDisplayString(),
                    structSymbol.Name));
                return;
            }
        }

        // 7. Generate the code
        string ns = structSymbol.ContainingNamespace.ToDisplayString();
        string typeName = structSymbol.Name;
        string accessibility = GetAccessibility(structSymbol.DeclaredAccessibility);
        string tValue = tValueType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        string tValueShort = tValueType.ToDisplayString();

        string emptyExpr = $"default({tValue})";
        string newExpr = $"throw new NotSupportedException(\"New() is not supported for {tValue}.\")";
        string fromExpr = "Create(value).OrThrow()";
        string parseExpr = $"{tValue}.Parse(s)";
        string tryParseExpr = $"{tValue}.TryParse(s, out var parsedValue)";
        string spanParseExpr = $"{tValue}.Parse(s)";
        string spanTryParseExpr = $"{tValue}.TryParse(s, out var parsedValue)";

        if (tValue == "global::System.Guid")
        {
            newExpr = target.UseSequentialGuid
                ? $"new {typeName}(global::System.Guid.CreateVersion7())"
                : $"new {typeName}(global::System.Guid.NewGuid())";
            parseExpr = "global::System.Guid.Parse(s)";
            tryParseExpr = "global::System.Guid.TryParse(s, out var parsedValue)";
            spanParseExpr = "global::System.Guid.Parse(s)";
            spanTryParseExpr = "global::System.Guid.TryParse(s, out var parsedValue)";
        }

        StringBuilder sb = new();
        sb.AppendLine("// <auto-generated />");
        sb.AppendLine("#nullable enable");
        sb.AppendLine("#pragma warning disable");
        sb.AppendLine("using System;");
        sb.AppendLine("using System.Diagnostics;");
        sb.AppendLine("using System.Diagnostics.CodeAnalysis;");
        sb.AppendLine("using KernelR.Domain.StronglyTypedIds;");
        sb.AppendLine("using KernelR.Domain.ValueObjects;");
        sb.AppendLine("using KernelR.Primitives.Results;");
        sb.AppendLine();
        sb.AppendLine($"namespace {ns};");
        sb.AppendLine();
        sb.AppendLine($"[DebuggerDisplay(\"{{Value}}\")]");
        sb.AppendLine($"{accessibility} readonly partial record struct {typeName} : IStronglyTypedId<{typeName}, {tValue}>, IValueObject, IComparable<{typeName}>, IComparable");
        sb.AppendLine("{");
        sb.AppendLine("    /// <inheritdoc />");
        sb.AppendLine($"    public static {typeName} Empty => new({emptyExpr});");
        sb.AppendLine();
        sb.AppendLine("    /// <inheritdoc />");
        sb.AppendLine($"    public static {typeName} New() => {newExpr};");
        sb.AppendLine();
        sb.AppendLine("    /// <inheritdoc />");
        sb.AppendLine($"    public static {typeName} From({tValue} value) => {fromExpr};");
        sb.AppendLine();
        sb.AppendLine("    /// <inheritdoc />");
        sb.AppendLine($"    public static Result<{typeName}> Create({tValue} value)");
        sb.AppendLine("    {");

        if (!target.AllowEmpty)
        {
            sb.AppendLine($"        if (EqualityComparer<{tValue}>.Default.Equals(value, default({tValue})))");
            sb.AppendLine($"            return new Error(\"KernelR.StronglyTypedId.{typeName}.EMPTY\", \"{typeName} cannot be empty.\", ErrorType.ValidationError);");
        }

        sb.AppendLine($"        return new {typeName}(value);");
        sb.AppendLine("    }");
        sb.AppendLine();
        sb.AppendLine("    /// <inheritdoc />");
        sb.AppendLine($"    public static {typeName} Parse(string s, IFormatProvider? provider) => new({parseExpr});");
        sb.AppendLine();
        sb.AppendLine("    /// <inheritdoc />");
        sb.AppendLine($"    public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, [MaybeNullWhen(false)] out {typeName} result)");
        sb.AppendLine("    {");
        sb.AppendLine("        result = default;");
        sb.AppendLine("        if (string.IsNullOrWhiteSpace(s)) return false;");
        sb.AppendLine($"        if (!{tryParseExpr}) return false;");
        sb.AppendLine($"        result = new {typeName}(parsedValue);");
        sb.AppendLine("        return true;");
        sb.AppendLine("    }");
        sb.AppendLine();
        sb.AppendLine("    /// <inheritdoc />");
        sb.AppendLine($"    public static {typeName} Parse(ReadOnlySpan<char> s, IFormatProvider? provider) => new({spanParseExpr});");
        sb.AppendLine();
        sb.AppendLine("    /// <inheritdoc />");
        sb.AppendLine($"    public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, [MaybeNullWhen(false)] out {typeName} result)");
        sb.AppendLine("    {");
        sb.AppendLine("        result = default;");
        sb.AppendLine($"        if (!{spanTryParseExpr}) return false;");
        sb.AppendLine($"        result = new {typeName}(parsedValue);");
        sb.AppendLine("        return true;");
        sb.AppendLine("    }");
        sb.AppendLine();
        sb.AppendLine("    /// <inheritdoc />");
        sb.AppendLine($"    public int CompareTo({typeName} other) => Value.CompareTo(other.Value);");
        sb.AppendLine();
        sb.AppendLine("    /// <inheritdoc />");
        sb.AppendLine("    int IComparable.CompareTo(object? obj)");
        sb.AppendLine("    {");
        sb.AppendLine("        return obj switch");
        sb.AppendLine("        {");
        sb.AppendLine("            null => 1,");
        sb.AppendLine($"            {typeName} other => CompareTo(other),");
        sb.AppendLine($"            _ => throw new ArgumentException($\"Object must be of type {{nameof({typeName})}}.\", nameof(obj)),");
        sb.AppendLine("        };");
        sb.AppendLine("    }");
        sb.AppendLine();
        sb.AppendLine($"    /// <summary>Determines whether the left <see cref=\"{typeName}\"/> is less than the right <see cref=\"{typeName}\"/>.</summary>");
        sb.AppendLine("    /// <param name=\"left\">The left identifier.</param>");
        sb.AppendLine("    /// <param name=\"right\">The right identifier.</param>");
        sb.AppendLine("    /// <returns><see langword=\"true\"/> if left is less than right; otherwise, <see langword=\"false\"/>.</returns>");
        sb.AppendLine($"    public static bool operator <({typeName} left, {typeName} right) => left.CompareTo(right) < 0;");
        sb.AppendLine();
        sb.AppendLine($"    /// <summary>Determines whether the left <see cref=\"{typeName}\"/> is greater than the right <see cref=\"{typeName}\"/>.</summary>");
        sb.AppendLine("    /// <param name=\"left\">The left identifier.</param>");
        sb.AppendLine("    /// <param name=\"right\">The right identifier.</param>");
        sb.AppendLine("    /// <returns><see langword=\"true\"/> if left is greater than right; otherwise, <see langword=\"false\"/>.</returns>");
        sb.AppendLine($"    public static bool operator >({typeName} left, {typeName} right) => left.CompareTo(right) > 0;");
        sb.AppendLine();
        sb.AppendLine($"    /// <summary>Determines whether the left <see cref=\"{typeName}\"/> is less than or equal to the right <see cref=\"{typeName}\"/>.</summary>");
        sb.AppendLine("    /// <param name=\"left\">The left identifier.</param>");
        sb.AppendLine("    /// <param name=\"right\">The right identifier.</param>");
        sb.AppendLine("    /// <returns><see langword=\"true\"/> if left is less than or equal to right; otherwise, <see langword=\"false\"/>.</returns>");
        sb.AppendLine($"    public static bool operator <=({typeName} left, {typeName} right) => left.CompareTo(right) <= 0;");
        sb.AppendLine();
        sb.AppendLine($"    /// <summary>Determines whether the left <see cref=\"{typeName}\"/> is greater than or equal to the right <see cref=\"{typeName}\"/>.</summary>");
        sb.AppendLine("    /// <param name=\"left\">The left identifier.</param>");
        sb.AppendLine("    /// <param name=\"right\">The right identifier.</param>");
        sb.AppendLine("    /// <returns><see langword=\"true\"/> if left is greater than or equal to right; otherwise, <see langword=\"false\"/>.</returns>");
        sb.AppendLine($"    public static bool operator >=({typeName} left, {typeName} right) => left.CompareTo(right) >= 0;");
        sb.AppendLine();
        sb.AppendLine("    /// <summary>Returns the string representation of the underlying value.</summary>");
        sb.AppendLine("    /// <returns>The string representation of the underlying value.</returns>");
        sb.AppendLine($"    public override string ToString() => Value.ToString()!;");
        sb.AppendLine();
        sb.AppendLine($"    /// <summary>Explicitly converts a <see cref=\"{typeName}\"/> to its underlying <see cref=\"{tValueShort}\"/> value.</summary>");
        sb.AppendLine("    /// <param name=\"id\">The identifier to convert.</param>");
        sb.AppendLine("    /// <returns>The underlying value.</returns>");
        sb.AppendLine($"    public static explicit operator {tValue}({typeName} id) => id.Value;");
        sb.AppendLine();
        sb.AppendLine($"    /// <summary>Explicitly converts an underlying value to a <see cref=\"{typeName}\"/>, validating it via <see cref=\"Create({tValue})\"/>.</summary>");
        sb.AppendLine("    /// <param name=\"value\">The underlying value.</param>");
        sb.AppendLine("    /// <returns>The validated identifier.</returns>");
        sb.AppendLine("    /// <exception cref=\"InvalidOperationException\">Thrown if the value is invalid (e.g., default).</exception>");
        sb.AppendLine($"    public static explicit operator {typeName}({tValue} value) => Create(value).OrThrow();");
        sb.AppendLine("}");

        context.AddSource($"{typeName}.StronglyTypedId.g.cs", SourceText.From(sb.ToString(), Encoding.UTF8));
    }

    private static string GetAccessibility(Accessibility accessibility) => accessibility switch
    {
        Accessibility.Public => "public",
        Accessibility.Internal => "internal",
        Accessibility.Protected => "protected",
        Accessibility.ProtectedOrInternal => "protected internal",
        Accessibility.ProtectedAndInternal => "private protected",
        Accessibility.Private => "private",
        _ => "public",
    };

    private sealed class Target : IEquatable<Target>
    {
        public INamedTypeSymbol StructSymbol { get; }
        public ITypeSymbol TValueType { get; }
        public bool UseSequentialGuid { get; }
        public bool AllowEmpty { get; }
        public Compilation Compilation { get; }

        public Target(INamedTypeSymbol structSymbol, ITypeSymbol tValueType, bool useSequentialGuid, bool allowEmpty, Compilation compilation)
        {
            StructSymbol = structSymbol;
            TValueType = tValueType;
            UseSequentialGuid = useSequentialGuid;
            AllowEmpty = allowEmpty;
            Compilation = compilation;
        }

        public bool Equals(Target? other)
        {
            if (other is null) return false;
            return SymbolEqualityComparer.Default.Equals(StructSymbol, other.StructSymbol)
                && SymbolEqualityComparer.Default.Equals(TValueType, other.TValueType)
                && UseSequentialGuid == other.UseSequentialGuid
                && AllowEmpty == other.AllowEmpty;
        }

        public override bool Equals(object? obj) => Equals(obj as Target);

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 31 + SymbolEqualityComparer.Default.GetHashCode(StructSymbol);
                hash = hash * 31 + SymbolEqualityComparer.Default.GetHashCode(TValueType);
                hash = hash * 31 + UseSequentialGuid.GetHashCode();
                hash = hash * 31 + AllowEmpty.GetHashCode();
                return hash;
            }
        }
    }
}
