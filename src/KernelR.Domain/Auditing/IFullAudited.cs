namespace KernelR.Domain.Auditing;

/// <summary>
/// Combines creation, modification, and deletion auditing into a single comprehensive contract.
/// </summary>
/// <typeparam name="TUserId">The type of the user identifier.</typeparam>
/// <remarks>
/// Use this interface for entities that require full tracking of their lifecycle (who created, 
/// who modified, and who deleted them).
/// </remarks>
public interface IFullAudited<TUserId> :
    ICreationAudited<TUserId>,
    IModificationAudited<TUserId>,
    IDeletionAudited<TUserId>
    where TUserId : struct;
