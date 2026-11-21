namespace KernelR.Domain.Abstractions;

/// <summary>
/// Defines a contract for entities that support optimistic concurrency control.
/// </summary>
/// <remarks>
/// <para>
/// Optimistic concurrency prevents lost updates when two users try to modify the
/// same record at the same time. The <see cref="ConcurrencyStamp"/> is read when the
/// entity is loaded, and checked against the database value when it is saved.
/// If they differ, the save fails with a concurrency exception.
/// </para>
/// <para>
/// In EF Core, this is usually mapped to a <c>rowversion</c> (SQL Server) or
/// <c>xmin</c> (PostgreSQL) column, and EF Core handles generating and validating
/// the stamp automatically. The property can also be a simple <see cref="string"/>
/// (e.g., a GUID) if the database does not support row versions natively.
/// </para>
/// <para>
/// <b>Why a separate interface?</b> Not every entity needs concurrency control.
/// By implementing this interface, an entity opts in explicitly, and the persistence
/// layer can configure the mapping only for those entities that require it.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// public sealed class Order : BaseEntity&lt;Guid&gt;, IConcurrencyStamp
/// {
///     public string ConcurrencyStamp { get; private set; } = Guid.NewGuid().ToString();
/// }
/// </code>
/// </example>
public interface IConcurrencyStamp
{
    /// <summary>
    /// Gets the concurrency stamp used to detect conflicting updates.
    /// </summary>
    /// <value>
    /// A string representation of the current concurrency token.
    /// </value>
    /// <remarks>
    /// The value must be updated by the persistence layer every time the entity is
    /// successfully saved. Application code should never set this property directly.
    /// </remarks>
    string ConcurrencyStamp { get; }
}
