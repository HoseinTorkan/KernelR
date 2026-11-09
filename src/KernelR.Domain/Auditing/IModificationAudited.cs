namespace KernelR.Domain.Auditing;

/// <summary>
/// Combines modification time and modifier tracking into a single contract.
/// </summary>
/// <typeparam name="TUserId">The type of the user identifier.</typeparam>
/// <remarks>
/// This interface is meant for entities that can be updated after creation.
/// </remarks>
public interface IModificationAudited<TUserId> : IHasModificationTime, IHasModifier<TUserId>
    where TUserId : struct;
