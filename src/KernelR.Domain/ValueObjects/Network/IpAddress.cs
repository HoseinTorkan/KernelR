using KernelR.Patterns.Results;
using KernelR.Patterns.Results.Extensions;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Sockets;

namespace KernelR.Domain.ValueObjects.Network;

/// <summary>
/// Represents a valid IPv4 or IPv6 address as an immutable value object.
/// </summary>
public readonly record struct IpAddress :
    IValueObject,
    IParsable<IpAddress>,
    IComparable<IpAddress>,
    IComparable
{
    #region ═══ Constants & Fields                                       ═══

    private readonly IPAddress _ipAddress;

    #endregion ═══ Constants & Fields                                       ═══

    #region ═══ Properties                                                 ═══

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
    /// <remarks>
    /// Checks for 10.0.0.0/8, 172.16.0.0/12, and 192.168.0.0/16. Always returns false for IPv6.
    /// </remarks>
    public bool IsPrivate
    {
        get
        {
            if (!IsIPv4)
            {
                return false;
            }

            byte[] bytes = _ipAddress.GetAddressBytes();
            return bytes[0] == 10
                || (bytes[0] == 172 && bytes[1] >= 16 && bytes[1] <= 31)
                || (bytes[0] == 192 && bytes[1] == 168);
        }
    }

    #endregion ═══ Properties                                                 ═══

    #region ═══ Constructors & Factories                                   ═══

    /// <summary>
    /// Initializes a new instance of the <see cref="IpAddress"/> struct.
    /// </summary>
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
        {
            return Result<IpAddress>.Failure(
                new Error(
                    "KernelR.ValueObjects.IpAddress.EMPTY",
                    "IP address cannot be empty.",
                    ErrorType.ValidationError));
        }

        if (!IPAddress.TryParse(value.Trim(), out IPAddress? parsedIp))
        {
            return Result<IpAddress>.Failure(
                new Error(
                    "KernelR.ValueObjects.IpAddress.INVALID_FORMAT",
                    "IP address is not a valid IPv4 or IPv6 format.",
                    ErrorType.ValidationError));
        }

        return Result<IpAddress>.Success(new IpAddress(parsedIp.ToString(), parsedIp));
    }

    /// <summary>
    /// Creates a new <see cref="IpAddress"/> from a <see cref="System.Net.IPAddress"/> instance.
    /// </summary>
    /// <param name="ipAddress">The system IP address to wrap.</param>
    /// <returns>
    /// A successful result carrying the new <see cref="IpAddress"/>, or a failure with 
    /// <c>KernelR.ValueObjects.IpAddress.NULL</c> if the input is <see langword="null"/>.
    /// </returns>
    public static Result<IpAddress> Create(IPAddress ipAddress)
    {
        if (ipAddress is null)
        {
            return Result<IpAddress>.Failure(
                new Error(
                    "KernelR.ValueObjects.IpAddress.NULL",
                    "IP address cannot be null.",
                    ErrorType.ValidationError));
        }

        return Result<IpAddress>.Success(new IpAddress(ipAddress.ToString(), ipAddress));
    }

    #endregion ═══ Constructors & Factories                                   ═══

    #region ═══ IParsable Implementation                                   ═══

    /// <inheritdoc />
    public static IpAddress Parse(string s, IFormatProvider? provider)
    {
        return Create(s).OrThrowFormatException();
    }

    /// <inheritdoc />
    public static bool TryParse(string? s, IFormatProvider? provider, [MaybeNullWhen(false)] out IpAddress result)
    {
        result = default;
        if (string.IsNullOrWhiteSpace(s))
        {
            return false;
        }

        Result<IpAddress> createResult = Create(s);
        if (createResult.IsFailure)
        {
            return false;
        }

        result = createResult.Value;
        return true;
    }

    #endregion ═══ IParsable Implementation                                   ═══

    #region ═══ Functional Operations                                      ═══

    /// <summary>
    /// Converts this value object back to a <see cref="System.Net.IPAddress"/> instance.
    /// </summary>
    /// <returns>The underlying <see cref="System.Net.IPAddress"/>.</returns>
    public IPAddress ToSystem()
    {
        return _ipAddress;
    }

    /// <summary>
    /// Converts an IPv4 address to its 32-bit unsigned integer representation.
    /// </summary>
    /// <returns>
    /// A successful result carrying the integer representation of the IPv4 address, 
    /// or a failure with <c>KernelR.ValueObjects.IpAddress.IPV6_TO_INTEGER</c> 
    /// if the address is IPv6.
    /// </returns>
    public Result<uint> ToInteger()
    {
        if (!IsIPv4)
        {
            return Result<uint>.Failure(
                new Error(
                    "KernelR.ValueObjects.IpAddress.IPV6_TO_INTEGER",
                    "Only IPv4 addresses can be converted to a 32-bit integer.",
                    ErrorType.ValidationError));
        }

        byte[] bytes = _ipAddress.GetAddressBytes();

        // Big-endian conversion (network byte order)
        uint result = (uint)(bytes[0] << 24 | bytes[1] << 16 | bytes[2] << 8 | bytes[3]);

        return Result<uint>.Success(result);
    }

    #endregion ═══ Functional Operations                                      ═══

    #region ═══ IComparable Implementation                                 ═══

    /// <inheritdoc />
    public int CompareTo(IpAddress other)
    {
        byte[] bytes1 = _ipAddress.GetAddressBytes();
        byte[] bytes2 = other._ipAddress.GetAddressBytes();

        if (bytes1.Length != bytes2.Length)
        {
            return bytes1.Length.CompareTo(bytes2.Length);
        }

        for (int i = 0; i < bytes1.Length; i++)
        {
            if (bytes1[i] != bytes2[i])
            {
                return bytes1[i].CompareTo(bytes2[i]);
            }
        }

        return 0;
    }

    /// <inheritdoc />
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="obj"/> is not of type <see cref="IpAddress"/>. 
    /// The exception message contains the JSON-serialized domain <see cref="Error"/>.
    /// </exception>
    int IComparable.CompareTo(object? obj)
    {
        if (obj is null)
        {
            return 1;
        }

        if (obj is not IpAddress other)
        {
            new Error(
                "KernelR.ValueObjects.IpAddress.INVALID_TYPE",
                $"Object must be of type {nameof(IpAddress)}.",
                ErrorType.ValidationError)
                .ThrowArgumentException(nameof(obj));
        }

        return CompareTo(other);
    }

    #endregion ═══ IComparable Implementation                                 ═══

    #region ═══ Comparison Operators                                       ═══

    /// <summary>Determines whether the left IP address is less than the right IP address.</summary>
    public static bool operator <(IpAddress left, IpAddress right) => left.CompareTo(right) < 0;

    /// <summary>Determines whether the left IP address is greater than the right IP address.</summary>
    public static bool operator >(IpAddress left, IpAddress right) => left.CompareTo(right) > 0;

    /// <summary>Determines whether the left IP address is less than or equal to the right IP address.</summary>
    public static bool operator <=(IpAddress left, IpAddress right) => left.CompareTo(right) <= 0;

    /// <summary>Determines whether the left IP address is greater than or equal to the right IP address.</summary>
    public static bool operator >=(IpAddress left, IpAddress right) => left.CompareTo(right) >= 0;

    #endregion ═══ Comparison Operators                                       ═══

    #region ═══ Helpers & Overrides                                        ═══

    /// <inheritdoc />
    public override string ToString() => Value;

    /// <summary>
    /// Explicitly converts an <see cref="IpAddress"/> to its normalized string value.
    /// </summary>
    public static explicit operator string(IpAddress ip) => ip.Value;

    #endregion ═══ Helpers & Overrides                                        ═══
}
