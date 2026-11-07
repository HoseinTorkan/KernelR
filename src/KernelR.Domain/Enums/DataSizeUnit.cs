namespace KernelR.Domain.Enums;

/// <summary>
/// Units of digital information supported by <see cref="ValueObjects.DataSize"/>.
/// </summary>
/// <remarks>
/// <para>
/// This enum uses <b>binary</b> (base-1024) prefixes: <c>1 KB = 1024 B</c>,
/// <c>1 MB = 1024 KB</c>, and so on. These are the units commonly displayed by
/// operating systems and storage tools, even though the SI standard would call them
/// kibibyte (KiB), mebibyte (MiB), etc.
/// </para>
/// <para>
/// <b>Why binary, not decimal?</b> Storage capacity reported by Windows, Linux, and
/// most file managers uses base-1024 for KB/MB/GB. Using base-1000 would produce
/// values that differ from what users see in their file explorer, causing confusion
/// (for example, a "500 GB" drive showing as "465 GB"). This type deliberately matches
/// the convention users actually observe.
/// </para>
/// <para>
/// Additional units (for example <c>TB</c>, <c>PB</c>) can be added later, but each
/// addition requires extending the conversion factor in <see cref="ValueObjects.DataSize"/>.
/// </para>
/// </remarks>
public enum DataSizeUnit
{
    /// <summary>Bytes (B).</summary>
    Byte = 0,

    /// <summary>Kilobytes (KB). 1 KB = 1024 bytes.</summary>
    Kilobyte = 1,

    /// <summary>Megabytes (MB). 1 MB = 1024 KB.</summary>
    Megabyte = 2,

    /// <summary>Gigabytes (GB). 1 GB = 1024 MB.</summary>
    Gigabyte = 3,

    /// <summary>Terabytes (TB). 1 TB = 1024 GB.</summary>
    Terabyte = 4
}
