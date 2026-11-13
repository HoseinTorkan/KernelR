namespace KernelR.Domain.StronglyTypedIds;

/// <summary>
/// Represents the unique identifier of a user.
/// </summary>
[StronglyTypedId<Guid>]
public readonly partial record struct UserId(Guid Value);

