namespace KernelR.Domain.Auditing;

/// <summary>
/// Combines creation time and creator tracking into a single contract.
/// </summary>
/// <typeparam name="TUserId">The type of the user identifier.</typeparam>
/// <remarks>
/// This interface is meant for entities that are immutable after creation, 
/// such as audit logs or immutable snapshots.
/// </remarks>
public interface ICreationAudited<TUserId> : IHasCreationTime, IHasCreator<TUserId>
    where TUserId : struct;
