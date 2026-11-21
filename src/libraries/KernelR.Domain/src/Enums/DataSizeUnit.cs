using KernelR.Domain.ValueObjects.Measurement;

namespace KernelR.Domain.Enums;

/// <summary>
/// Specifies the units of digital information supported by <see cref="DataSize"/>.
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
/// <b>Canonical representation:</b> The enum is only used for <i>input</i> to the
/// <see cref="DataSize"/> value object (via <c>FromXxx</c> or <c>Create(decimal, DataSizeUnit)</c>).
/// Internally, every <see cref="DataSize"/> is stored as a <see cref="long"/> number of bytes,
/// so this enum is never used for storage or comparison.
/// </para>
/// </remarks>
/// <seealso cref="DataSize"/>
public enum DataSizeUnit
{
    /// <summary>
    /// Bytes (B). The canonical unit used internally by <see cref="DataSize"/>.
    /// </summary>
    /// <remarks>
    /// 1 byte = 8 bits. This is the smallest unit of digital information.
    /// </remarks>
    Byte = 0,

    /// <summary>
    /// Kilobytes (KB). 1 KB = 1024 bytes.
    /// </summary>
    /// <remarks>
    /// This is the binary kilobyte (1024 bytes), not the decimal kilobyte (1000 bytes).
    /// It matches the value displayed by Windows Explorer, Linux file managers, and
    /// most programming languages' size-formatting functions.
    /// </remarks>
    Kilobyte = 1,

    /// <summary>
    /// Megabytes (MB). 1 MB = 1024 KB = 1,048,576 bytes.
    /// </summary>
    /// <remarks>
    /// This is the binary megabyte (1024² bytes), not the decimal megabyte (10⁶ bytes).
    /// </remarks>
    Megabyte = 2,

    /// <summary>
    /// Gigabytes (GB). 1 GB = 1024 MB = 1,073,741,824 bytes.
    /// </summary>
    /// <remarks>
    /// This is the binary gigabyte (1024³ bytes), not the decimal gigabyte (10⁹ bytes).
    /// </remarks>
    Gigabyte = 3,

    /// <summary>
    /// Terabytes (TB). 1 TB = 1024 GB = 1,099,511,627,776 bytes.
    /// </summary>
    /// <remarks>
    /// This is the binary terabyte (1024⁴ bytes), not the decimal terabyte (10¹² bytes).
    /// </remarks>
    Terabyte = 4,
}
