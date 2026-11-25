namespace KernelR.Domain.Auditing.Internal;

internal static class ModificationAuditedValidator
{
    internal static void Validate<TUserId>(DateTimeOffset updatedAt, TUserId updatedBy)
    {
        if (updatedAt == default)
        {
            throw new ArgumentException(
                "The modification date cannot be the default value.",
                nameof(updatedAt));
        }

        if (updatedAt.Offset != TimeSpan.Zero)
        {
            throw new ArgumentException(
                "The modification date must be in UTC.",
                nameof(updatedAt));
        }

        if (EqualityComparer<TUserId>.Default.Equals(updatedBy, default))
        {
            throw new ArgumentException(
                "The modifier identifier cannot be the default value.",
                nameof(updatedBy));
        }
    }
}
