namespace KernelR.Domain.StronglyTypedIds;

/// <summary>
/// Marks a <see langword="partial"/> <see langword="record struct"/> as a strongly-typed identifier.
/// The KernelR Source Generator will automatically implement the rest of the boilerplate.
/// </summary>
/// <typeparam name="TValue">The underlying primitive type (e.g., <see cref="Guid"/>, <see cref="int"/>).</typeparam>
/// <remarks>
/// <para>
/// The decorated struct must be declared as <c>partial</c> and must have exactly one constructor
/// that accepts a <typeparamref name="TValue"/> parameter.
/// </para>
/// <para>
/// The generator will emit:
/// </para>
/// <list type="bullet">
///   <item><description>The <see cref="IStronglyTypedId{TSelf, TValue}"/> implementation.</description></item>
///   <item><description><c>New</c>, <c>From</c>, <c>Empty</c>, and <c>Create</c> factory methods.</description></item>
///   <item><description><c>IParsable</c> and <c>ISpanParsable</c> implementations.</description></item>
///   <item><description><c>IComparable</c> implementation and comparison operators.</description></item>
///   <item><description>A string <c>ToString</c> override and explicit conversion operators.</description></item>
/// </list>
/// </remarks>
/// <example>
/// <code>
/// [StronglyTypedId&lt;Guid&gt;]
/// public readonly partial record struct UserId(Guid Value);
/// </code>
/// </example>
/// <seealso cref="IStronglyTypedId{TSelf, TValue}"/>
[AttributeUsage(AttributeTargets.Struct, Inherited = false, AllowMultiple = false)]
public sealed class StronglyTypedIdAttribute<TValue> : Attribute
    where TValue : struct
{
    /// <summary>
    /// Gets or sets a value indicating whether to use sequential GUIDs (v7) in the <c>New()</c> method.
    /// Sequential GUIDs are time-ordered and reduce database index fragmentation.
    /// Defaults to <see langword="true"/>.
    /// </summary>
    /// <remarks>
    /// This setting is only applicable when <typeparamref name="TValue"/> is <see cref="Guid"/>.
    /// If set to <see langword="false"/>, a purely random GUID (v4) is generated.
    /// </remarks>
    public bool UseSequentialGuid { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether the <c>Create</c> factory method allows the default value (e.g., <see cref="Guid.Empty"/>).
    /// Defaults to <see langword="false"/>, which means empty values are rejected.
    /// </summary>
    /// <remarks>
    /// Set this to <see langword="true"/> only if you are integrating with a legacy system
    /// where the default value (e.g., <c>0</c> or <c>Guid.Empty</c>) is a valid identifier.
    /// </remarks>
    public bool AllowEmpty { get; set; }
}
