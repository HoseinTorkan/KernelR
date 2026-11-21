namespace KernelR.Domain.Auditing.Abstractions;

/// <summary>
/// Combines creation time and creator tracking into a single contract.
/// </summary>
/// <typeparam name="TUserId">The type of the user identifier.</typeparam>
/// <remarks>
/// <para>
/// This interface is meant for entities that are immutable after creation,
/// such as audit logs or immutable snapshots.
/// </para>
/// <para>
/// For entities that can be updated after creation, use
/// <see cref="IModificationAudited{TUserId}"/> or <see cref="IFullAudited{TUserId}"/>.
/// </para>
/// </remarks>
/// <seealso cref="IHasCreationTime"/>
/// <seealso cref="IHasCreator{TUserId}"/>
public interface ICreationAudited<TUserId> :
    IHasCreationTime,
    IHasCreator<TUserId>
    where TUserId : struct;
