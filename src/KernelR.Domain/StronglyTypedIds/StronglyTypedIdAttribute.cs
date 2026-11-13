namespace KernelR.Domain.StronglyTypedIds;

/// <summary>
/// Marks a <see langword="partial"/> <see langword="record struct"/> as a strongly-typed identifier.
/// The KernelR Source Generator will automatically implement the rest of the boilerplate.
/// </summary>
/// <typeparam name="TValue">The underlying primitive type (e.g., <see cref="Guid"/>, <see cref="int"/>).</typeparam>
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
    /// </remarks>
    public bool UseSequentialGuid { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether the <c>Create</c> factory method allows the default value (e.g., <see cref="Guid.Empty"/>).
    /// Defaults to <see langword="false"/>, which means empty values are rejected.
    /// </summary>
    public bool AllowEmpty { get; set; }
}
