namespace KernelR.Domain.Auditing.Internal;

internal static class FullAuditedValidator
{
    internal static void Validate<TUserId>(DateTimeOffset deletedAt, TUserId deletedBy)
    {
        if (deletedAt == default)
        {
            throw new ArgumentException(
                "The deletion date cannot be the default value.",
                nameof(deletedAt));
        }

        if (deletedAt.Offset != TimeSpan.Zero)
        {
            throw new ArgumentException(
                "The deletion date must be in UTC.",
                nameof(deletedAt));
        }

        if (EqualityComparer<TUserId>.Default.Equals(deletedBy, default))
        {
            throw new ArgumentException(
                "The deleter identifier cannot be the default value.",
                nameof(deletedBy));
        }
    }
}
