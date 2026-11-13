namespace KernelR.Domain.Abstractions;

/// <summary>
/// Defines a contract for entities that support optimistic concurrency control.
/// </summary>
/// <remarks>
/// <para>
/// Optimistic concurrency prevents lost updates when two users try to modify the
/// same record at the same time. The <see cref="ConcurrencyStamp"/> is read when the
/// entity is loaded, and checked against the database value when it is saved.
/// If they differ, the save fails with a concurrency exception.
/// </para>
/// <para>
/// In EF Core, this is usually mapped to a <c>rowversion</c> (SQL Server) or
/// <c>xmin</c> (PostgreSQL) column, and EF Core handles generating and validating
/// the stamp automatically.
/// </para>
/// </remarks>
public interface IConcurrencyStamp
{
    /// <summary>
    /// Gets the concurrency stamp used to detect conflicting updates.
    /// </summary>
    string ConcurrencyStamp { get; }
}
