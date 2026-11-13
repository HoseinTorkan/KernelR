namespace KernelR.Domain.Auditing.Abstractions;

/// <summary>
/// Defines a contract for entities that support soft-deletion.
/// </summary>
/// <remarks>
/// This is the minimal contract required by EF Core global query filters
/// (e.g., <c>modelBuilder.Entity&lt;T&gt;().HasQueryFilter(e => !e.IsDeleted)</c>).
/// For tracking <i>when</i> and <i>by whom</i> the entity was deleted, use
/// <see cref="IDeletionAudited{TUserId}"/>, which inherits from this interface.
/// </remarks>
public interface ISoftDeletable
{
    /// <summary>
    /// Gets a value indicating whether this entity has been soft-deleted.
    /// </summary>
    bool IsDeleted { get; }
}
