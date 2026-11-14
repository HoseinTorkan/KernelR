namespace KernelR.Domain.Auditing.Abstractions;

/// <summary>
/// Combines modification time and modifier tracking into a single contract.
/// </summary>
/// <typeparam name="TUserId">The type of the user identifier.</typeparam>
/// <remarks>
/// <para>
/// This interface is meant for entities that can be updated after creation.
/// </para>
/// <para>
/// Note that this interface does <b>not</b> include creation tracking.
/// If you need both creation and modification, use <see cref="IFullAudited{TUserId}"/>.
/// </para>
/// </remarks>
/// <seealso cref="IHasModificationTime"/>
/// <seealso cref="IHasModifier{TUserId}"/>
public interface IModificationAudited<TUserId> : IHasModificationTime, IHasModifier<TUserId>
    where TUserId : struct;
