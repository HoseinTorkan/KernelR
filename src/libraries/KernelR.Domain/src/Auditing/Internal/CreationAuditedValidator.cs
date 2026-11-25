namespace KernelR.Domain.Auditing.Internal;

internal static class CreationAuditedValidator
{
    internal static void Validate<TUserId>(DateTimeOffset createdAt, TUserId createdBy)
    {
        if (createdAt == default)
        {
            throw new ArgumentException(
                "The creation date cannot be the default value.",
                nameof(createdAt));
        }

        if (createdAt.Offset != TimeSpan.Zero)
        {
            throw new ArgumentException(
                "The creation date must be in UTC.",
                nameof(createdAt));
        }

        if (EqualityComparer<TUserId>.Default.Equals(createdBy, default))
        {
            throw new ArgumentException(
                "The creator identifier cannot be the default value.",
                nameof(createdBy));
        }
    }
}
