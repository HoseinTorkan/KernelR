using KernelR.Domain.Abstractions;

namespace KernelR.Domain.Entities;

/// <summary>
/// Provides a base implementation for entities with identity-based equality.
/// </summary>
/// <typeparam name="TId">
/// The type of the entity identifier.
/// </typeparam>
/// <remarks>
/// <para>
/// This class provides entity identity, identifier validation, and equality
/// semantics based on the entity's runtime type and identifier.
/// </para>
/// <para>
/// The identifier is assigned during construction and cannot be changed
/// afterward through the public API. A default identifier value is not allowed.
/// </para>
/// <para>
/// Two entities are considered equal when they have the same runtime type
/// and identifier.
/// </para>
/// </remarks>
public abstract class BaseEntity<TId> :
    IEntity<TId>,
    IEquatable<BaseEntity<TId>>
    where TId : struct
{
    /// <inheritdoc/>
    public TId Id { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="BaseEntity{TId}"/> class.
    /// </summary>
    /// <param name="id">
    /// The identifier of the entity.
    /// </param>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="id"/> is equal to the default value
    /// of <typeparamref name="TId"/>.
    /// </exception>
    protected BaseEntity(TId id)
    {
        if (EqualityComparer<TId>.Default.Equals(id, default))
        {
            throw new ArgumentException(
                "The entity identifier cannot be the default value.",
                nameof(id));
        }

        Id = id;
    }

    /// <summary>
    /// Determines whether the current entity is equal to another entity
    /// of the same base entity type.
    /// </summary>
    /// <param name="other">
    /// The entity to compare with the current entity.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the specified entity has the same runtime type
    /// and identifier as the current entity; otherwise,
    /// <see langword="false"/>.
    /// </returns>
    public bool Equals(BaseEntity<TId>? other)
    {
        if (other is null)
            return false;

        if (ReferenceEquals(this, other))
            return true;

        return GetType() == other.GetType() &&
               EqualityComparer<TId>.Default.Equals(Id, other.Id);
    }

    /// <summary>
    /// Determines whether the specified object is equal to the current entity.
    /// </summary>
    /// <param name="obj">
    /// The object to compare with the current entity.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the specified object is an entity with the same
    /// runtime type and identifier as the current entity; otherwise,
    /// <see langword="false"/>.
    /// </returns>
    public override bool Equals(object? obj) =>
        obj is BaseEntity<TId> entity && Equals(entity);

    /// <summary>
    /// Returns a hash code for the current entity based on its runtime type
    /// and identifier.
    /// </summary>
    /// <returns>
    /// A hash code for the current entity.
    /// </returns>
    public override int GetHashCode() =>
        HashCode.Combine(GetType(), Id);

    /// <summary>
    /// Determines whether two entities are equal.
    /// </summary>
    /// <param name="left">
    /// The first entity to compare.
    /// </param>
    /// <param name="right">
    /// The second entity to compare.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if both entities are equal; otherwise,
    /// <see langword="false"/>.
    /// </returns>
    public static bool operator ==(BaseEntity<TId>? left, BaseEntity<TId>? right) =>
        object.Equals(left, right);

    /// <summary>
    /// Determines whether two entities are not equal.
    /// </summary>
    /// <param name="left">
    /// The first entity to compare.
    /// </param>
    /// <param name="right">
    /// The second entity to compare.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if both entities are not equal; otherwise,
    /// <see langword="false"/>.
    /// </returns>
    public static bool operator !=(BaseEntity<TId>? left, BaseEntity<TId>? right) =>
        !object.Equals(left, right);
}
