using KernelR.Patterns.Results;
using KernelR.Patterns.Serialization;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text.Json;

namespace KernelR.Domain.ValueObjects;

/// <summary>
/// Represents a monetary amount with its associated currency.
/// </summary>
/// <remarks>
/// <para>
/// This value object guarantees the following invariants:
/// </para>
/// <list type="bullet">
///   <item><description>
///     The <see cref="Amount"/> is always rounded to two decimal places using
///     <see cref="MidpointRounding.AwayFromZero"/>.
///   </description></item>
///   <item><description>
///     The <see cref="Currency"/> is always a valid, uppercase ISO 4217 code
///     (see <see cref="CurrencyCode"/>).
///   </description></item>
///   <item><description>
///     The <see cref="Amount"/> may be negative. Negative amounts represent debts,
///     balances, reversals, or withdrawals.
///   </description></item>
/// </list>
/// <para>
/// <b>Error model:</b> The <see cref="Create(decimal, string)"/> factory returns
/// <see cref="Result{T}"/> for recoverable validation failures. Arithmetic operators,
/// by contrast, throw <see cref="InvalidOperationException"/> on currency mismatch,
/// because mixing currencies is a programming error rather than a recoverable condition.
/// </para>
/// </remarks>
public readonly record struct Money : IParsable<Money>
{
    /// <summary>
    /// Gets the monetary amount, rounded to two decimal places.
    /// </summary>
    /// <remarks>
    /// The value may be negative.
    /// </remarks>
    public decimal Amount { get; }

    /// <summary>
    /// Gets the validated ISO 4217 currency code.
    /// </summary>
    public CurrencyCode Currency { get; }

    private Money(decimal amount, CurrencyCode currency)
    {
        Amount = Math.Round(amount, 2, MidpointRounding.AwayFromZero);
        Currency = currency;
    }

    /// <summary>
    /// Creates a new <see cref="Money"/> from an amount and a currency code.
    /// </summary>
    /// <param name="amount">
    /// The monetary amount. May be negative. Values with more than two decimal places
    /// are rounded using <see cref="MidpointRounding.AwayFromZero"/>.
    /// </param>
    /// <param name="currency">
    /// A 3-letter ISO 4217 currency code (for example <c>IRR</c>, <c>USD</c>).
    /// Case-insensitive; validated through <see cref="CurrencyCode.Create(string)"/>.
    /// </param>
    /// <returns>
    /// A successful result carrying the new <see cref="Money"/>, or a failure if the
    /// currency code is invalid.
    /// </returns>
    public static Result<Money> Create(decimal amount, string currency)
    {
        Result<CurrencyCode> codeResult = CurrencyCode.Create(currency);

        return codeResult.IsFailure
            ? Result<Money>.Failure(codeResult.Error)
            : Result<Money>.Success(new(amount, codeResult.Value));
    }

    /// <summary>
    /// Creates a new <see cref="Money"/> from a string in the form
    /// <c>"&lt;amount&gt; &lt;currency&gt;"</c>.
    /// </summary>
    /// <param name="value">
    /// The string representation, for example <c>"100 IRR"</c>, <c>"-50.25 USD"</c>.
    /// The amount uses invariant culture; the currency is validated through
    /// <see cref="CurrencyCode.Create(string)"/>.
    /// </param>
    /// <returns>
    /// A successful result carrying the new <see cref="Money"/>, or a failure with one of:
    /// <list type="bullet">
    ///   <item><description><c>KernelR.ValueObjects.Money.EMPTY</c> when the input is null or whitespace.</description></item>
    ///   <item><description><c>KernelR.ValueObjects.Money.INVALID_FORMAT</c> when the input does not contain exactly two parts.</description></item>
    ///   <item><description><c>KernelR.ValueObjects.Money.INVALID_VALUE</c> when the numeric part is not a valid number.</description></item>
    ///   <item><description>Any error from <see cref="CurrencyCode.Create(string)"/> when the currency code is invalid.</description></item>
    /// </list>
    /// </returns>
    /// <example>
    /// <code>
    /// Money.Create("100 IRR");    // → Money(100, IRR)
    /// Money.Create("-50.25 USD"); // → Money(-50.25, USD)
    /// </code>
    /// </example>
    public static Result<Money> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Result<Money>.Failure(
                new Error(
                    "KernelR.ValueObjects.Money.EMPTY",
                    "Money cannot be empty.",
                    ErrorType.ValidationError));
        }

        string[] parts = value.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 2)
        {
            return Result<Money>.Failure(
                new Error(
                    "KernelR.ValueObjects.Money.INVALID_FORMAT",
                    "Money must be formatted as '<amount> <currency>', for example '100 IRR'.",
                    ErrorType.ValidationError));
        }

        if (!decimal.TryParse(parts[0], NumberStyles.Number, CultureInfo.InvariantCulture, out decimal amount))
        {
            return Result<Money>.Failure(
                new Error(
                    "KernelR.ValueObjects.Money.INVALID_VALUE",
                    "Money amount is not a valid number.",
                    ErrorType.ValidationError));
        }

        return Create(amount, parts[1]);
    }

    /// <inheritdoc />
    /// <exception cref="FormatException">
    /// <paramref name="s"/> is <see langword="null"/>, empty, whitespace, not in the
    /// form <c>"&lt;amount&gt; &lt;currency&gt;"</c>, or contains an invalid amount or
    /// currency. The exception message contains the JSON-serialized domain
    /// <see cref="Error"/>.
    /// </exception>
    public static Money Parse(string s, IFormatProvider? provider)
    {
        Result<Money> result = Create(s);
        if (result.IsSuccess)
        {
            return result.Value;
        }

        string errorJson = JsonSerializer.Serialize(result.Error, KernelRJsonOptions.Options);
        throw new FormatException(errorJson);
    }

    /// <inheritdoc />
    public static bool TryParse(string? s, IFormatProvider? provider, [MaybeNullWhen(false)] out Money result)
    {
        result = default;
        if (string.IsNullOrWhiteSpace(s))
        {
            return false;
        }

        Result<Money> createResult = Create(s);
        if (createResult.IsFailure)
        {
            return false;
        }

        result = createResult.Value;
        return true;
    }

    /// <summary>
    /// Creates a zero amount in the given currency.
    /// </summary>
    /// <param name="currency">A 3-letter ISO 4217 currency code.</param>
    /// <returns>
    /// A successful result carrying <c>0.00</c> in the given currency, or a failure
    /// when the currency code is invalid.
    /// </returns>
    public static Result<Money> Zero(string currency) => Create(0.00m, currency);

    #region Arithmetic Operators

    /// <summary>
    /// Adds two monetary amounts of the same currency.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when <paramref name="left"/> and <paramref name="right"/> have different currencies.
    /// </exception>
    public static Money operator +(Money left, Money right)
    {
        EnsureSameCurrency(left, right);
        return new Money(left.Amount + right.Amount, left.Currency);
    }

    /// <summary>
    /// Subtracts one monetary amount from another of the same currency.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when <paramref name="left"/> and <paramref name="right"/> have different currencies.
    /// </exception>
    public static Money operator -(Money left, Money right)
    {
        EnsureSameCurrency(left, right);
        return new Money(left.Amount - right.Amount, left.Currency);
    }

    /// <summary>
    /// Multiplies a monetary amount by a scalar factor.
    /// </summary>
    public static Money operator *(Money money, decimal multiplier)
        => new(money.Amount * multiplier, money.Currency);

    /// <summary>
    /// Divides a monetary amount by a scalar divisor.
    /// </summary>
    /// <exception cref="DivideByZeroException">
    /// Thrown when <paramref name="divisor"/> is zero.
    /// </exception>
    public static Money operator /(Money money, decimal divisor)
    {
        if (divisor is 0m)
        {
            throw new DivideByZeroException("Cannot divide money by zero.");
        }

        return new Money(money.Amount / divisor, money.Currency);
    }

    /// <summary>
    /// Negates a monetary amount.
    /// </summary>
    public static Money operator -(Money money)
        => new(-money.Amount, money.Currency);

    #endregion

    #region Comparison Operators (Money vs Money)

    /// <summary>Determines whether the left amount is greater than the right amount.</summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the operands have different currencies.
    /// </exception>
    public static bool operator >(Money left, Money right)
    {
        EnsureSameCurrency(left, right);
        return left.Amount > right.Amount;
    }

    /// <summary>Determines whether the left amount is less than the right amount.</summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the operands have different currencies.
    /// </exception>
    public static bool operator <(Money left, Money right)
    {
        EnsureSameCurrency(left, right);
        return left.Amount < right.Amount;
    }

    /// <summary>Determines whether the left amount is greater than or equal to the right amount.</summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the operands have different currencies.
    /// </exception>
    public static bool operator >=(Money left, Money right)
    {
        EnsureSameCurrency(left, right);
        return left.Amount >= right.Amount;
    }

    /// <summary>Determines whether the left amount is less than or equal to the right amount.</summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the operands have different currencies.
    /// </exception>
    public static bool operator <=(Money left, Money right)
    {
        EnsureSameCurrency(left, right);
        return left.Amount <= right.Amount;
    }

    #endregion

    #region Comparison Operators (Money vs decimal)

    /// <summary>Determines whether the amount is greater than the given scalar.</summary>
    public static bool operator >(Money left, decimal right) => left.Amount > right;

    /// <summary>Determines whether the amount is less than the given scalar.</summary>
    public static bool operator <(Money left, decimal right) => left.Amount < right;

    /// <summary>Determines whether the amount is greater than or equal to the given scalar.</summary>
    public static bool operator >=(Money left, decimal right) => left.Amount >= right;

    /// <summary>Determines whether the amount is less than or equal to the given scalar.</summary>
    public static bool operator <=(Money left, decimal right) => left.Amount <= right;

    #endregion

    private static void EnsureSameCurrency(Money left, Money right)
    {
        if (left.Currency != right.Currency)
        {
            throw new InvalidOperationException(
                $"Cannot operate on amounts with different currencies: " +
                $"'{left.Currency}' vs '{right.Currency}'.");
        }
    }

    /// <summary>
    /// Returns a human-readable representation such as <c>1,234.56 IRR</c>.
    /// </summary>
    public override string ToString() => $"{Amount:N2} {Currency}";
}
