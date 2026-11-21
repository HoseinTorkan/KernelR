namespace KernelR.Domain.Auditing.Abstractions;

/// <summary>
/// Defines a contract for entities that track their last modification time.
/// </summary>
/// <remarks>
/// <para>
/// This interface is typically implemented alongside <see cref="IHasModifier{TUserId}"/>
/// (via <see cref="IModificationAudited{TUserId}"/>) to form a complete modification audit trail.
/// </para>
/// <para>
/// <b>UTC Requirement:</b> Implementations should always persist this value in UTC.
/// Local time should be applied only at the presentation layer (UI), not in the domain or database.
/// </para>
/// </remarks>
public interface IHasModificationTime
{
    /// <summary>
    /// Gets the moment when this entity was last updated.
    /// </summary>
    /// <value>
    /// A <see cref="DateTimeOffset"/> representing the exact instant of the last modification,
    /// or <see langword="null"/> if the entity has never been modified since its creation.
    /// </value>
    /// <remarks>
    /// This value is usually set by the persistence layer (e.g., an EF Core interceptor)
    /// immediately before an update statement is executed.
    /// </remarks>
    DateTimeOffset? UpdatedAt { get; }
}
