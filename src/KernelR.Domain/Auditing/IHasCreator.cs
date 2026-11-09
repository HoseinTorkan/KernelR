namespace KernelR.Domain.Auditing;

/// <summary>
/// Defines a contract for entities that track the user who created them.
/// </summary>
/// <typeparam name="TUserId">The type of the user identifier.</typeparam>
public interface IHasCreator<TUserId>
    where TUserId : struct
{
    #region ═══ Properties                                                 ═══

    /// <summary>
    /// Gets the identifier of the user who created this entity.
    /// </summary>
    TUserId CreatedBy { get; }

    #endregion ═══ Properties                                                 ═══
}
