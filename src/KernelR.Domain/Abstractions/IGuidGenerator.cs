namespace KernelR.Domain.Abstractions;

/// <summary>
/// Defines a contract for generating unique <see cref="Guid"/> values.
/// </summary>
/// <remarks>
/// <para>
/// Using this abstraction instead of calling <see cref="Guid.NewGuid"/> directly
/// provides two important benefits:
/// </para>
/// <list type="number">
///   <item><description>
///     <b>Testability:</b> Unit tests can inject a deterministic implementation
///     that returns predictable IDs, making assertions reliable.
///   </description></item>
///   <item><description>
///     <b>Database performance:</b> The <c>useSequential</c> parameter lets
///     implementations generate <i>sequential</i> GUIDs (also known as Comb GUIDs),
///     which reduce page splits and index fragmentation in databases such as
///     SQL Server.
///   </description></item>
/// </list>
/// <para>
/// The implementation is expected to be provided by the infrastructure layer
/// (e.g., using <c>Guid.NewGuid()</c> or a Comb-GUID algorithm such as
/// <c>SequentialGuidValueGenerator</c> from EF Core). It is typically registered
/// as a singleton, because GUID generation is stateless (or carries state in a
/// thread-safe counter for sequential mode).
/// </para>
/// </remarks>
/// <example>
/// <code>
/// public sealed class Order
/// {
///     public Order(IGuidGenerator idGenerator)
///     {
///         Id = idGenerator.Create();
///     }
///     
///     public Guid Id { get; }
/// }
/// </code>
/// </example>
public interface IGuidGenerator
{
    /// <summary>
    /// Creates a new unique <see cref="Guid"/>.
    /// </summary>
    /// <param name="useSequential">
    /// When <see langword="true"/>, the implementation should generate a <i>sequential</i>
    /// GUID (e.g., Comb GUID) to reduce database index fragmentation. When
    /// <see langword="false"/>, a purely random GUID (v4) is generated.
    /// Defaults to <see langword="true"/>, which is the recommended value for
    /// persistent entities on most relational databases.
    /// </param>
    /// <returns>A new, unique <see cref="Guid"/> value.</returns>
    /// <remarks>
    /// Implementations must guarantee uniqueness. If a sequential algorithm is used,
    /// it is the implementation's responsibility to be thread-safe.
    /// </remarks>
    Guid Create(bool useSequential = true);
}
