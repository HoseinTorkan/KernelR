namespace KernelR.Domain.Auditing;

/// <summary>
/// Combines creation time and creator tracking into a single contract.
/// </summary>
/// <typeparam name="TUserId">The type of the user identifier.</typeparam>
public interface ICreationAudited<TUserId> : IHasCreationTime, IHasCreator<TUserId>
    where TUserId : struct;
