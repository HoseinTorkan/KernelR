using KernelR.Primitives.Results;
using KernelR.Primitives.Results.Extensions;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace KernelR.Domain.ValueObjects.Financial;

/// <summary>
/// Represents a monetary amount with its associated currency.
/// </summary>
/// <remarks>
/// <para>
/// This value object guarantees the following invariants:
/// </para>
/// <list type="bullet">
///   <item><description>The <see cref="Amount"/> is always rounded to two decimal places using <see cref="MidpointRounding.AwayFromZero"/>.</description></item>
///   <item><description>The <see cref="Currency"/> is always a valid, uppercase ISO 4217 code (see <see cref="CurrencyCode"/>).</description></item>
///   <item><description>The <see cref="Amount"/> may be negative. Negative amounts represent debts, balances, reversals, or withdrawals.</description></item>
/// </list>
/// <para>
/// <b>Error model:</b> The <see cref="Create(decimal, string)"/> factory and all safe functional methods 
/// return <see cref="Result{T}"/> for recoverable failures. 
/// Arithmetic and comparison operators (like <c>+</c>, <c>-</c>, <c>&gt;</c>, etc.) throw 
/// <see cref="InvalidOperationException"/> on failure, as they are typically used in scenarios where 
/// such errors are programming mistakes. 
/// </para>
/// <para>
/// <b>Note on decimal comparisons:</b> This type intentionally does not provide comparison operators 
/// (like <c>&lt;</c>, <c>&gt;</c>, <c>&lt;=</c>, <c>&gt;=</c>) against raw <see cref="decimal"/> values. 
/// Such comparisons would silently ignore the currency and could lead to production bugs (e.g., comparing 
/// an IRR amount with a hardcoded USD threshold). Use the <see cref="Amount"/> property explicitly if you 
/// need to compare against a raw number.
/// </para>
/// </remarks>
/// <seealso cref="CurrencyCode"/>
/// <seealso cref="IValueObject"/>
public readonly record struct Money : IValueObject, IParsable<Money>, IComparable<Money>, IComparable
{
    /// <summary>
    /// The number of decimal places used for rounding monetary amounts.
    /// </summary>
    private const int Precision = 2;

    /// <summary>
    /// Gets the monetary amount, rounded to two decimal places.
    /// </summary>
    /// <value>The amount in the associated <see cref="Currency"/>.</value>
    public decimal Amount { get; }

    /// <summary>
    /// Gets the validated ISO 4217 currency code.
    /// </summary>
    /// <value>The <see cref="CurrencyCode"/> associated with the amount.</value>
    public CurrencyCode Currency { get; }

    /// <summary>
    /// Gets a value indicating whether the amount is zero.
    /// </summary>
    /// <value><see langword="true"/> if <see cref="Amount"/> is exactly zero; otherwise, <see langword="false"/>.</value>
    public bool IsZero => Amount == 0m;

    /// <summary>
    /// Gets a value indicating whether the amount is strictly positive (greater than zero).
    /// </summary>
    /// <value><see langword="true"/> if <see cref="Amount"/> is greater than zero; otherwise, <see langword="false"/>.</value>
    public bool IsPositive => Amount > 0m;

    /// <summary>
    /// Gets a value indicating whether the amount is strictly negative (less than zero).
    /// </summary>
    /// <value><see langword="true"/> if <see cref="Amount"/> is less than zero; otherwise, <see langword="false"/>.</value>
    public bool IsNegative => Amount < 0m;

    /// <summary>
    /// Initializes a new instance of the <see cref="Money"/> struct.
    /// </summary>
    /// <param name="amount">The monetary amount.</param>
    /// <param name="currency">The validated currency code.</param>
    private Money(decimal amount, CurrencyCode currency)
    {
        Amount = Math.Round(amount, Precision, MidpointRounding.AwayFromZero);
        Currency = currency;
    }

    /// <summary>
    /// Creates a new <see cref="Money"/> from an amount and a currency code.
    /// </summary>
    /// <param name="amount">The monetary amount.</param>
    /// <param name="currency">The currency code to validate.</param>
    /// <returns>
    /// A successful result carrying the new <see cref="Money"/>, or a failure with one of:
    /// <list type="bullet">
    ///   <item><description><c>KernelR.ValueObjects.CurrencyCode.EMPTY</c> when <paramref name="currency"/> is null or whitespace.</description></item>
    ///   <item><description><c>KernelR.ValueObjects.CurrencyCode.INVALID</c> when <paramref name="currency"/> is not a recognized ISO 4217 code.</description></item>
    /// </list>
    /// </returns>
    public static Result<Money> Create(decimal amount, string currency)
    {
        Result<CurrencyCode> codeResult = CurrencyCode.Create(currency);
        return codeResult.IsFailure
            ? codeResult.Error
            : new Money(amount, codeResult.Value);
    }

    /// <summary>
    /// Creates a new <see cref="Money"/> from a string in the form <c>"&lt;amount&gt; &lt;currency&gt;"</c>.
    /// </summary>
    /// <param name="value">The string representation, for example <c>"100 IRR"</c> or <c>"-25.50 USD"</c>.</param>
    /// <returns>
    /// A successful result carrying the new <see cref="Money"/>, or a failure with one of the following error codes:
    /// <list type="bullet">
    ///   <item><description><c>KernelR.ValueObjects.Money.EMPTY</c> when the input is null or whitespace.</description></item>
    ///   <item><description><c>KernelR.ValueObjects.Money.INVALID_FORMAT</c> when the input does not contain exactly two parts.</description></item>
    ///   <item><description><c>KernelR.ValueObjects.Money.INVALID_VALUE</c> when the numeric part is not a valid number.</description></item>
    ///   <item><description><c>KernelR.ValueObjects.CurrencyCode.INVALID</c> when the currency part is not recognized.</description></item>
    /// </list>
    /// </returns>
    public static Result<Money> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return new Error(
                "KernelR.ValueObjects.Money.EMPTY",
                "Money cannot be empty.",
                ErrorType.ValidationError);

        string[] parts = value.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length != 2)
            return new Error(
                "KernelR.ValueObjects.Money.INVALID_FORMAT",
                "Money must be formatted as '<amount> <currency>', for example '100 IRR'.",
                ErrorType.ValidationError);

        if (!decimal.TryParse(parts[0], NumberStyles.Number, CultureInfo.InvariantCulture, out decimal amount))
            return new Error(
                "KernelR.ValueObjects.Money.INVALID_VALUE",
                "Money amount is not a valid number.",
                ErrorType.ValidationError);

        return Create(amount, parts[1]);
    }

    /// <summary>
    /// Creates a zero amount in the given currency.
    /// </summary>
    /// <param name="currency">The currency code.</param>
    /// <returns>
    /// A successful result carrying a new <see cref="Money"/> with a value of zero, or a failure if the currency is invalid.
    /// </returns>
    public static Result<Money> Zero(string currency) => Create(0.00m, currency);

    /// <summary>
    /// Parses the specified string into a <see cref="Money"/> instance.
    /// </summary>
    /// <param name="s">The string to parse (e.g., "100 IRR").</param>
    /// <param name="provider">An object that provides culture-specific formatting information. This parameter is ignored.</param>
    /// <returns>A <see cref="Money"/> instance representing the parsed value.</returns>
    /// <exception cref="FormatException">
    /// Thrown when <paramref name="s"/> is not a valid money string. 
    /// The exception message contains the JSON-serialized domain <see cref="Error"/>.
    /// </exception>
    public static Money Parse(string s, IFormatProvider? provider) => Create(s).OrThrowFormatException();

    /// <summary>
    /// Attempts to parse the specified string into a <see cref="Money"/> instance.
    /// </summary>
    /// <param name="s">The string to parse.</param>
    /// <param name="provider">An object that provides culture-specific formatting information. This parameter is ignored.</param>
    /// <param name="result">When this method returns, contains the parsed <see cref="Money"/> instance, if the parsing succeeded, or the default value if it failed.</param>
    /// <returns><see langword="true"/> if the parsing succeeded; otherwise, <see langword="false"/>.</returns>
    public static bool TryParse(string? s, IFormatProvider? provider, [MaybeNullWhen(false)] out Money result)
        => Create(s ?? string.Empty).TryGetValue(out result);

    /// <summary>
    /// Ensures that the other monetary amount has the same currency.
    /// </summary>
    /// <param name="other">The other monetary amount.</param>
    /// <returns>A successful result if currencies match, or a failure with <c>KernelR.ValueObjects.Money.CURRENCY_MISMATCH</c>.</returns>
    private Result<bool> EnsureSameCurrency(Money other)
    {
        return Currency != other.Currency
            ? new Error(
                "KernelR.ValueObjects.Money.CURRENCY_MISMATCH",
                $"Cannot operate on different currencies: '{Currency}' vs '{other.Currency}'.",
                ErrorType.ValidationError)
            : Result<bool>.Success(true);
    }

    /// <summary>
    /// Adds the specified monetary amount to the current amount, returning a <see cref="Result{T}"/>.
    /// </summary>
    /// <param name="other">The monetary amount to add.</param>
    /// <returns>A successful result carrying the sum, or a failure with <c>KernelR.ValueObjects.Money.CURRENCY_MISMATCH</c>.</returns>
    public Result<Money> Add(Money other)
    {
        Result<bool> check = EnsureSameCurrency(other);
        return check.IsFailure
            ? check.Error
            : new Money(Amount + other.Amount, Currency);
    }

    /// <summary>
    /// Subtracts the specified monetary amount from the current amount, returning a <see cref="Result{T}"/>.
    /// </summary>
    /// <param name="other">The monetary amount to subtract.</param>
    /// <returns>A successful result carrying the difference, or a failure with <c>KernelR.ValueObjects.Money.CURRENCY_MISMATCH</c>.</returns>
    public Result<Money> Subtract(Money other)
    {
        Result<bool> check = EnsureSameCurrency(other);
        return check.IsFailure
            ? check.Error
            : new Money(Amount - other.Amount, Currency);
    }

    /// <summary>
    /// Multiplies the monetary amount by a scalar factor, returning a <see cref="Result{T}"/>.
    /// </summary>
    /// <param name="multiplier">The scalar factor.</param>
    /// <returns>A successful result carrying the product.</returns>
    public Result<Money> Multiply(decimal multiplier) => new Money(Amount * multiplier, Currency);

    /// <summary>
    /// Divides the monetary amount by a scalar divisor, returning a <see cref="Result{T}"/>.
    /// </summary>
    /// <param name="divisor">The scalar divisor.</param>
    /// <returns>A successful result carrying the quotient, or a failure with <c>KernelR.ValueObjects.Money.DIVIDE_BY_ZERO</c>.</returns>
    public Result<Money> Divide(decimal divisor)
    {
        return divisor == 0m
            ? new Error(
                "KernelR.ValueObjects.Money.DIVIDE_BY_ZERO",
                "Cannot divide money by zero.",
                ErrorType.ValidationError)
            : new Money(Amount / divisor, Currency);
    }

    /// <summary>
    /// Determines whether the current amount is greater than the specified amount, returning a <see cref="Result{T}"/>.
    /// </summary>
    /// <param name="other">The monetary amount to compare.</param>
    /// <returns>A successful result carrying the comparison result, or a failure with <c>KernelR.ValueObjects.Money.CURRENCY_MISMATCH</c>.</returns>
    public Result<bool> GreaterThan(Money other)
    {
        Result<bool> check = EnsureSameCurrency(other);
        return check.IsFailure ? check.Error : Result<bool>.Success(Amount > other.Amount);
    }

    /// <summary>
    /// Determines whether the current amount is less than the specified amount, returning a <see cref="Result{T}"/>.
    /// </summary>
    /// <param name="other">The monetary amount to compare.</param>
    /// <returns>A successful result carrying the comparison result, or a failure with <c>KernelR.ValueObjects.Money.CURRENCY_MISMATCH</c>.</returns>
    public Result<bool> LessThan(Money other)
    {
        Result<bool> check = EnsureSameCurrency(other);
        return check.IsFailure ? check.Error : Result<bool>.Success(Amount < other.Amount);
    }

    /// <summary>
    /// Determines whether the current amount is greater than or equal to the specified amount, returning a <see cref="Result{T}"/>.
    /// </summary>
    /// <param name="other">The monetary amount to compare.</param>
    /// <returns>A successful result carrying the comparison result, or a failure with <c>KernelR.ValueObjects.Money.CURRENCY_MISMATCH</c>.</returns>
    public Result<bool> GreaterThanOrEqual(Money other)
    {
        Result<bool> check = EnsureSameCurrency(other);
        return check.IsFailure ? check.Error : Result<bool>.Success(Amount >= other.Amount);
    }

    /// <summary>
    /// Determines whether the current amount is less than or equal to the specified amount, returning a <see cref="Result{T}"/>.
    /// </summary>
    /// <param name="other">The monetary amount to compare.</param>
    /// <returns>A successful result carrying the comparison result, or a failure with <c>KernelR.ValueObjects.Money.CURRENCY_MISMATCH</c>.</returns>
    public Result<bool> LessThanOrEqual(Money other)
    {
        Result<bool> check = EnsureSameCurrency(other);
        return check.IsFailure ? check.Error : Result<bool>.Success(Amount <= other.Amount);
    }

    /// <summary>
    /// Adds two monetary amounts of the same currency.
    /// </summary>
    /// <param name="left">The first amount.</param>
    /// <param name="right">The second amount.</param>
    /// <returns>The sum of the two amounts.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the currencies do not match.</exception>
    public static Money operator +(Money left, Money right) => left.Add(right).OrThrow();

    /// <summary>
    /// Subtracts one monetary amount from another of the same currency.
    /// </summary>
    /// <param name="left">The first amount.</param>
    /// <param name="right">The second amount.</param>
    /// <returns>The difference between the two amounts.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the currencies do not match.</exception>
    public static Money operator -(Money left, Money right) => left.Subtract(right).OrThrow();

    /// <summary>
    /// Multiplies a monetary amount by a scalar factor.
    /// </summary>
    /// <param name="money">The monetary amount.</param>
    /// <param name="multiplier">The scalar factor.</param>
    /// <returns>The product of the amount and the factor.</returns>
    public static Money operator *(Money money, decimal multiplier) => money.Multiply(multiplier).OrThrow();

    /// <summary>
    /// Multiplies a scalar factor by a monetary amount (commutative support).
    /// </summary>
    /// <param name="multiplier">The scalar factor.</param>
    /// <param name="money">The monetary amount.</param>
    /// <returns>The product of the amount and the factor.</returns>
    public static Money operator *(decimal multiplier, Money money) => money.Multiply(multiplier).OrThrow();

    /// <summary>
    /// Divides a monetary amount by a scalar divisor.
    /// </summary>
    /// <param name="money">The monetary amount.</param>
    /// <param name="divisor">The scalar divisor.</param>
    /// <returns>The quotient of the amount and the divisor.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the divisor is zero.</exception>
    public static Money operator /(Money money, decimal divisor) => money.Divide(divisor).OrThrow();

    /// <summary>
    /// Negates a monetary amount.
    /// </summary>
    /// <param name="money">The monetary amount to negate.</param>
    /// <returns>The negated monetary amount.</returns>
    public static Money operator -(Money money) => new(-money.Amount, money.Currency);

    /// <summary>
    /// Determines whether the left amount is greater than the right amount.
    /// </summary>
    /// <param name="left">The left amount.</param>
    /// <param name="right">The right amount.</param>
    /// <returns><see langword="true"/> if left is greater; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the currencies do not match.</exception>
    public static bool operator >(Money left, Money right) => left.GreaterThan(right).OrThrow();

    /// <summary>
    /// Determines whether the left amount is less than the right amount.
    /// </summary>
    /// <param name="left">The left amount.</param>
    /// <param name="right">The right amount.</param>
    /// <returns><see langword="true"/> if left is less; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the currencies do not match.</exception>
    public static bool operator <(Money left, Money right) => left.LessThan(right).OrThrow();

    /// <summary>
    /// Determines whether the left amount is greater than or equal to the right amount.
    /// </summary>
    /// <param name="left">The left amount.</param>
    /// <param name="right">The right amount.</param>
    /// <returns><see langword="true"/> if left is greater or equal; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the currencies do not match.</exception>
    public static bool operator >=(Money left, Money right) => left.GreaterThanOrEqual(right).OrThrow();

    /// <summary>
    /// Determines whether the left amount is less than or equal to the right amount.
    /// </summary>
    /// <param name="left">The left amount.</param>
    /// <param name="right">The right amount.</param>
    /// <returns><see langword="true"/> if left is less or equal; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the currencies do not match.</exception>
    public static bool operator <=(Money left, Money right) => left.LessThanOrEqual(right).OrThrow();

    /// <inheritdoc />
    public int CompareTo(Money other)
    {
        if (Currency != other.Currency)
            new Error(
                "KernelR.ValueObjects.Money.CURRENCY_MISMATCH",
                "Cannot compare amounts with different currencies.",
                ErrorType.ValidationError).Throw();

        return Amount.CompareTo(other.Amount);
    }

    /// <inheritdoc />
    int IComparable.CompareTo(object? obj)
    {
        return obj switch
        {
            null => 1,
            Money other => CompareTo(other),
            _ => throw new ArgumentException($"Object must be of type {nameof(Money)}.", nameof(obj))
        };
    }

    /// <summary>
    /// Returns the absolute value of the current monetary amount.
    /// </summary>
    /// <returns>A new <see cref="Money"/> with the absolute amount and the same currency.</returns>
    public Money Abs() => new(Math.Abs(Amount), Currency);

    /// <summary>
    /// Returns a human-readable representation of the monetary amount.
    /// </summary>
    /// <returns>A string formatted as "&lt;amount&gt; &lt;currency&gt;", for example "100.00 IRR".</returns>
    public override string ToString() => $"{Amount.ToString("N2", CultureInfo.InvariantCulture)} {Currency}";

    /// <summary>
    /// Explicitly converts a <see cref="Money"/> to its string representation.
    /// </summary>
    /// <param name="money">The money instance to convert.</param>
    /// <returns>The string representation of the money.</returns>
    public static explicit operator string(Money money) => money.ToString();
}
