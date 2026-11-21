namespace KernelR.Domain.Auditing.Abstractions;

/// <summary>
/// Defines a contract for entities that track their soft-deletion time.
/// </summary>
/// <remarks>
/// <para>
/// This interface is typically implemented alongside <see cref="IHasDeleter{TUserId}"/>
/// and <see cref="ISoftDeletable"/> (via <see cref="IDeletionAudited{TUserId}"/>)
/// to form a complete soft-deletion audit trail.
/// </para>
/// <para>
/// <b>UTC Requirement:</b> Implementations should always persist this value in UTC.
/// Local time should be applied only at the presentation layer (UI), not in the domain or database.
/// </para>
/// </remarks>
public interface IHasDeletionTime
{
    /// <summary>
    /// Gets the moment when this entity was soft-deleted.
    /// </summary>
    /// <value>
    /// A <see cref="DateTimeOffset"/> representing the exact instant of soft-deletion,
    /// or <see langword="null"/> if the entity has not been deleted.
    /// </value>
    /// <remarks>
    /// This value is usually set by the persistence layer (e.g., an EF Core interceptor)
    /// when a delete operation is intercepted and converted into an update.
    /// </remarks>
    DateTimeOffset? DeletedAt { get; }
}
