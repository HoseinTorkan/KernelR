namespace KernelR.Domain.Auditing.Abstractions;

/// <summary>
/// Defines a contract for entities that track creation time and creator identity.
/// </summary>
/// <typeparam name="TUserId">
/// The type of the creator's identifier.
/// </typeparam>
/// <remarks>
/// <para>
/// This interface combines <see cref="IHasCreationTime"/> and
/// <see cref="IHasCreator{TUserId}"/> to provide a single contract
/// for creation auditing.
/// </para>
/// <para>
/// Implementations expose the creation timestamp and the identifier
/// of the user who created the entity.
/// </para>
/// </remarks>
public interface ICreationAudited<TUserId> :
    IHasCreationTime,
    IHasCreator<TUserId>
    where TUserId : struct;
