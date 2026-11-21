namespace KernelR.Domain.Auditing.Abstractions;

/// <summary>
/// Combines deletion time, deleter tracking, and the soft-delete flag into a single contract.
/// </summary>
/// <typeparam name="TUserId">The type of the user identifier.</typeparam>
/// <remarks>
/// <para>
/// This interface is intended for entities that support soft-deletion. It inherits
/// <see cref="ISoftDeletable"/> to provide the <c>IsDeleted</c> flag required by
/// EF Core global query filters.
/// </para>
/// <para>
/// <b>Global Query Filter:</b> Implementations must ensure that soft-deleted entities
/// are excluded from normal queries. In EF Core, this is typically achieved via a global
/// query filter configured in <c>DbContext.OnModelCreating</c>:
/// </para>
/// <code>
/// modelBuilder.Entity&lt;MyEntity&gt;().HasQueryFilter(e =&gt; !e.IsDeleted);
/// </code>
/// </remarks>
/// <seealso cref="IHasDeletionTime"/>
/// <seealso cref="IHasDeleter{TUserId}"/>
/// <seealso cref="ISoftDeletable"/>
public interface IDeletionAudited<TUserId> :
    IHasDeletionTime,
    IHasDeleter<TUserId>,
    ISoftDeletable
    where TUserId : struct;
