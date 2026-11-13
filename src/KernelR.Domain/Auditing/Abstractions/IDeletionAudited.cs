namespace KernelR.Domain.Auditing.Abstractions;

/// <summary>
/// Combines deletion time, deleter tracking, and the IsDeleted flag into a single contract.
/// </summary>
/// <typeparam name="TUserId">The type of the user identifier.</typeparam>
public interface IDeletionAudited<TUserId> : IHasDeletionTime, IHasDeleter<TUserId>, ISoftDeletable
    where TUserId : struct;
