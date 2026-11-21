namespace KernelR.Domain.Auditing.Abstractions;

/// <summary>
/// Defines a contract for entities that support soft-deletion.
/// </summary>
/// <remarks>
/// <para>
/// This is the minimal contract required by EF Core global query filters
/// (e.g., <c>modelBuilder.Entity&lt;T&gt;().HasQueryFilter(e =&gt; !e.IsDeleted)</c>).
/// For tracking <i>when</i> and <i>by whom</i> the entity was deleted, use
/// <see cref="IDeletionAudited{TUserId}"/>, which inherits from this interface.
/// </para>
/// <para>
/// <b>Why a marker interface?</b> By implementing this interface, an entity opts in
/// to soft-deletion behavior. The persistence layer can then automatically apply the
/// global query filter without the application code needing to check a boolean on
/// every query manually.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// public sealed class Invoice : BaseEntity&lt;Guid&gt;, ISoftDeletable
/// {
///     public bool IsDeleted { get; private set; }
/// }
/// </code>
/// </example>
public interface ISoftDeletable
{
    /// <summary>
    /// Gets a value indicating whether this entity has been soft-deleted.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if the entity is considered deleted; otherwise, <see langword="false"/>.
    /// </value>
    /// <remarks>
    /// Implementations typically define this as a computed property:
    /// <c>bool IsDeleted =&gt; DeletedAt is not null;</c>. This ensures that
    /// <see cref="IsDeleted"/> and <see cref="IHasDeletionTime.DeletedAt"/>
    /// can never be inconsistent with each other.
    /// </remarks>
    bool IsDeleted { get; }
}
