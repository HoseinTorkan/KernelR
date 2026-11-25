namespace KernelR.Domain.Auditing.Abstractions;

/// <summary>
/// Defines a contract for types that track modification time
/// and modifier identity.
/// </summary>
/// <typeparam name="TUserId">
/// The type of the modifier's identifier.
/// </typeparam>
/// <remarks>
/// <para>
/// This interface combines <see cref="IHasModificationTime"/> and
/// <see cref="IHasModifier{TUserId}"/> to provide a single contract
/// for modification auditing.
/// </para>
/// <para>
/// Implementations provide a method for updating modification audit
/// information, allowing the modification timestamp and modifier identity
/// to be updated together.
/// </para>
/// </remarks>
public interface IModificationAudited<TUserId> :
    IHasModificationTime,
    IHasModifier<TUserId>
    where TUserId : struct
{
    /// <summary>
    /// Updates the modification audit information.
    /// </summary>
    /// <param name="updatedAt">
    /// The timestamp of the modification.
    /// </param>
    /// <param name="updatedBy">
    /// The identifier of the user who performed the modification.
    /// </param>
    void UpdateModificationAudit(DateTimeOffset updatedAt, TUserId updatedBy);
}
