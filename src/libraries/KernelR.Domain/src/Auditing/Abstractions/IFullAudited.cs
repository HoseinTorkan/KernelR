namespace KernelR.Domain.Auditing.Abstractions;

/// <summary>
/// Combines creation, modification, and deletion auditing into a single comprehensive contract.
/// </summary>
/// <typeparam name="TUserId">The type of the user identifier.</typeparam>
/// <remarks>
/// <para>
/// Use this interface for entities that require full tracking of their lifecycle
/// (who created, who modified, and who deleted them).
/// </para>
/// <para>
/// This interface aggregates:
/// </para>
/// <list type="bullet">
///   <item><description><see cref="ICreationAudited{TUserId}"/> (created at, created by).</description></item>
///   <item><description><see cref="IModificationAudited{TUserId}"/> (updated at, updated by).</description></item>
///   <item><description><see cref="IDeletionAudited{TUserId}"/> (deleted at, deleted by, is deleted).</description></item>
/// </list>
/// <para>
/// Implementing this interface is usually done by deriving from the corresponding
/// base class (e.g., <c>FullAuditedEntity&lt;TId, TUserId&gt;</c> or
/// <c>FullAuditedAggregateRoot&lt;TId, TUserId&gt;</c>).
/// </para>
/// </remarks>
public interface IFullAudited<TUserId> :
    ICreationAudited<TUserId>,
    IModificationAudited<TUserId>,
    IDeletionAudited<TUserId>
    where TUserId : struct;
