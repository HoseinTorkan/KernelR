namespace KernelR.Domain.Auditing.Abstractions;

/// <summary>
/// Defines a contract for entities that track their creation time.
/// </summary>
/// <remarks>
/// <para>
/// This interface is typically implemented alongside <see cref="IHasCreator{TUserId}"/>
/// (via <see cref="ICreationAudited{TUserId}"/>) to form a complete creation audit trail.
/// </para>
/// <para>
/// <b>UTC Requirement:</b> Implementations should always persist this value in UTC.
/// Local time should be applied only at the presentation layer (UI), not in the domain or database.
/// </para>
/// </remarks>
public interface IHasCreationTime
{
    /// <summary>
    /// Gets the moment when this entity was created.
    /// </summary>
    /// <value>
    /// A <see cref="DateTimeOffset"/> representing the exact instant of creation.
    /// </value>
    /// <remarks>
    /// This value is usually set by the persistence layer (e.g., an EF Core interceptor)
    /// immediately before the entity is inserted into the database. Once set, it should
    /// never be modified.
    /// </remarks>
    DateTimeOffset CreatedAt { get; }
}
