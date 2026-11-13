using KernelR.Primitives.Results;
using KernelR.Primitives.Results.Extensions;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Sockets;

namespace KernelR.Domain.ValueObjects.Network;

/// <summary>
/// Represents a valid IPv4 or IPv6 address as an immutable value object.
/// </summary>
/// <remarks>
/// <para>
/// This value object encapsulates the standard <see cref="IPAddress"/> to keep the
/// domain model free from infrastructure and framework-specific dependencies.
/// </para>
/// </remarks>
public readonly record struct IpAddress : IValueObject, IParsable<IpAddress>, IComparable<IpAddress>, IComparable
{
    private readonly IPAddress _ipAddress;

    /// <summary>
    /// Gets the normalized string representation of the IP address.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Gets a value indicating whether this is an IPv4 address.
    /// </summary>
    public bool IsIPv4 => _ipAddress.AddressFamily == AddressFamily.InterNetwork;

    /// <summary>
    /// Gets a value indicating whether this is an IPv6 address.
    /// </summary>
    public bool IsIPv6 => _ipAddress.AddressFamily == AddressFamily.InterNetworkV6;

    /// <summary>
    /// Gets a value indicating whether this is a loopback address (e.g., 127.0.0.1 or ::1).
    /// </summary>
    public bool IsLoopback => IPAddress.IsLoopback(_ipAddress);

    /// <summary>
    /// Gets a value indicating whether this is a private network address (RFC 1918 for IPv4).
    /// </summary>
    public bool IsPrivate
    {
        get
        {
            if (!IsIPv4) return false;

            byte[] bytes = _ipAddress.GetAddressBytes();
            return bytes[0] == 10
                || (bytes[0] == 172 && bytes[1] >= 16 && bytes[1] <= 31)
                || (bytes[0] == 192 && bytes[1] == 168);
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="IpAddress"/> struct.
    /// </summary>
    /// <param name="value">The normalized string representation.</param>
    /// <param name="ipAddress">The parsed IP address.</param>
    private IpAddress(string value, IPAddress ipAddress)
    {
        Value = value;
        _ipAddress = ipAddress;
    }

    /// <summary>
    /// Creates a new <see cref="IpAddress"/> from the given string.
    /// </summary>
    /// <param name="value">The raw IP address string.</param>
    /// <returns>A successful result, or a failure if the string is empty or invalid.</returns>
    public static Result<IpAddress> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return new Error("KernelR.ValueObjects.IpAddress.EMPTY", "IP address cannot be empty.", ErrorType.ValidationError);

        if (!IPAddress.TryParse(value.Trim(), out IPAddress? parsedIp))
            return new Error("KernelR.ValueObjects.IpAddress.INVALID_FORMAT", "IP address is not a valid IPv4 or IPv6 format.", ErrorType.ValidationError);

        return Create(parsedIp);
    }

    /// <summary>
    /// Creates a new <see cref="IpAddress"/> from a <see cref="System.Net.IPAddress"/> instance.
    /// </summary>
    /// <param name="ipAddress">The system IP address to wrap.</param>
    /// <returns>A successful result, or a failure if the input is null.</returns>
    public static Result<IpAddress> Create(IPAddress ipAddress)
    {
        return ipAddress is null
            ? new Error("KernelR.ValueObjects.IpAddress.NULL", "IP address cannot be null.", ErrorType.ValidationError)
            : new IpAddress(ipAddress.ToString(), ipAddress);
    }

    /// <inheritdoc />
    public static IpAddress Parse(string s, IFormatProvider? provider) => Create(s).OrThrowFormatException();

    /// <inheritdoc />
    public static bool TryParse(string? s, IFormatProvider? provider, [MaybeNullWhen(false)] out IpAddress result)
        => Create(s ?? string.Empty).TryGetValue(out result);

    /// <summary>
    /// Converts an IPv4 address to its 32-bit unsigned integer representation.
    /// </summary>
    /// <returns>A successful result carrying the integer representation of the IPv4 address, or a failure for IPv6.</returns>
    public Result<uint> ToInteger()
    {
        if (!IsIPv4)
            return new Error("KernelR.ValueObjects.IpAddress.IPV6_TO_INTEGER", "Only IPv4 addresses can be converted to a 32-bit integer.", ErrorType.ValidationError);

        byte[] bytes = _ipAddress.GetAddressBytes();
        return Result<uint>.Success((uint)(bytes[0] << 24 | bytes[1] << 16 | bytes[2] << 8 | bytes[3]));
    }

    /// <inheritdoc />
    public int CompareTo(IpAddress other)
    {
        byte[] bytes1 = _ipAddress.GetAddressBytes();
        byte[] bytes2 = other._ipAddress.GetAddressBytes();

        if (bytes1.Length != bytes2.Length) return bytes1.Length.CompareTo(bytes2.Length);

        for (int i = 0; i < bytes1.Length; i++)
            if (bytes1[i] != bytes2[i]) return bytes1[i].CompareTo(bytes2[i]);

        return 0;
    }

    /// <inheritdoc />
    int IComparable.CompareTo(object? obj)
    {
        return obj switch
        {
            null => 1,
            IpAddress other => CompareTo(other),
            _ => throw new ArgumentException($"Object must be of type {nameof(IpAddress)}.", nameof(obj))
        };
    }

    /// <summary>Determines whether the left IP address is less than the right IP address.</summary>
    /// <param name="left">The left IP address.</param>
    /// <param name="right">The right IP address.</param>
    /// <returns><see langword="true"/> if left is less; otherwise, <see langword="false"/>.</returns>
    public static bool operator <(IpAddress left, IpAddress right) => left.CompareTo(right) < 0;

    /// <summary>Determines whether the left IP address is greater than the right IP address.</summary>
    /// <param name="left">The left IP address.</param>
    /// <param name="right">The right IP address.</param>
    /// <returns><see langword="true"/> if left is greater; otherwise, <see langword="false"/>.</returns>
    public static bool operator >(IpAddress left, IpAddress right) => left.CompareTo(right) > 0;

    /// <summary>Determines whether the left IP address is less than or equal to the right IP address.</summary>
    /// <param name="left">The left IP address.</param>
    /// <param name="right">The right IP address.</param>
    /// <returns><see langword="true"/> if left is less or equal; otherwise, <see langword="false"/>.</returns>
    public static bool operator <=(IpAddress left, IpAddress right) => left.CompareTo(right) <= 0;

    /// <summary>Determines whether the left IP address is greater than or equal to the right IP address.</summary>
    /// <param name="left">The left IP address.</param>
    /// <param name="right">The right IP address.</param>
    /// <returns><see langword="true"/> if left is greater or equal; otherwise, <see langword="false"/>.</returns>
    public static bool operator >=(IpAddress left, IpAddress right) => left.CompareTo(right) >= 0;

    /// <inheritdoc />
    public override string ToString() => Value;

    /// <summary>Explicitly converts an <see cref="IpAddress"/> to its normalized string value.</summary>
    /// <param name="ip">The IP address instance to convert.</param>
    /// <returns>The normalized string representation of the IP address.</returns>
    public static explicit operator string(IpAddress ip) => ip.Value;

    /// <summary>
    /// Explicitly converts an <see cref="IpAddress"/> to a <see cref="System.Net.IPAddress"/> instance.
    /// </summary>
    /// <param name="ip">The IP address value object to convert.</param>
    /// <returns>The underlying <see cref="System.Net.IPAddress"/>.</returns>
    public static explicit operator IPAddress(IpAddress ip) => ip._ipAddress;
}
