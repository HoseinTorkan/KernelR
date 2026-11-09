using KernelR.Patterns.Results;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace KernelR.Domain.ValueObjects;

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
/// </remarks>
public readonly record struct Money :
    IValueObject,
    IParsable<Money>,
    IComparable<Money>,
    IComparable
{
    /// <summary>
    /// The number of decimal places used for rounding monetary amounts.
    /// </summary>
    private const int Precision = 2;

    /// <summary>
    /// Gets the monetary amount, rounded to two decimal places.
    /// </summary>
    public decimal Amount { get; }

    /// <summary>
    /// Gets the validated ISO 4217 currency code.
    /// </summary>
    public CurrencyCode Currency { get; }

    /// <summary>
    /// Gets a value indicating whether the amount is zero.
    /// </summary>
    public bool IsZero => Amount == 0m;

    /// <summary>
    /// Gets a value indicating whether the amount is strictly positive (greater than zero).
    /// </summary>
    public bool IsPositive => Amount > 0m;

    /// <summary>
    /// Gets a value indicating whether the amount is strictly negative (less than zero).
    /// </summary>
    public bool IsNegative => Amount < 0m;

    /// <summary>
    /// Initializes a new instance of the <see cref="Money"/> struct.
    /// </summary>
    private Money(decimal amount, CurrencyCode currency)
    {
        Amount = Math.Round(amount, Precision, MidpointRounding.AwayFromZero);
        Currency = currency;
    }

    /// <summary>
    /// Creates a new <see cref="Money"/> from an amount and a currency code.
    /// </summary>
    public static Result<Money> Create(decimal amount, string currency)
    {
        Result<CurrencyCode> codeResult = CurrencyCode.Create(currency);

        return codeResult.IsFailure
            ? Result<Money>.Failure(codeResult.Error)
            : Result<Money>.Success(new(amount, codeResult.Value));
    }

    /// <summary>
    /// Creates a new <see cref="Money"/> from a string in the form <c>"&lt;amount&gt; &lt;currency&gt;"</c>.
    /// </summary>
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

        string[] parts = value.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);

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

    /// <summary>
    /// Parses the specified string into a <see cref="Money"/> instance.
    /// </summary>
    public static Money Parse(string s, IFormatProvider? provider)
    {
        return Create(s).OrThrowFormatException();
    }

    /// <summary>
    /// Attempts to parse the specified string into a <see cref="Money"/> instance.
    /// </summary>
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
    public static Result<Money> Zero(string currency) => Create(0.00m, currency);

    /// <summary>
    /// Returns the absolute value of the current monetary amount.
    /// </summary>
    public Money Abs()
    {
        return new(Math.Abs(Amount), Currency);
    }

    #region ═══ Safe Functional Operations (Math & Comparison) ═══

    // ─── Math Operations (Returns Result) ───

    /// <summary>
    /// Adds the specified monetary amount to the current amount, returning a <see cref="Result{T}"/>.
    /// </summary>
    public Result<Money> Add(Money other)
    {
        if (Currency != other.Currency)
        {
            return Result<Money>.Failure(
                new Error(
                    "KernelR.ValueObjects.Money.CURRENCY_MISMATCH",
                    $"Cannot add amounts with different currencies: '{Currency}' vs '{other.Currency}'.",
                    ErrorType.ValidationError));
        }

        return Result<Money>.Success(new(Amount + other.Amount, Currency));
    }

    /// <summary>
    /// Subtracts the specified monetary amount from the current amount, returning a <see cref="Result{T}"/>.
    /// </summary>
    public Result<Money> Subtract(Money other)
    {
        if (Currency != other.Currency)
        {
            return Result<Money>.Failure(
                new Error(
                    "KernelR.ValueObjects.Money.CURRENCY_MISMATCH",
                    $"Cannot subtract amounts with different currencies: '{Currency}' vs '{other.Currency}'.",
                    ErrorType.ValidationError));
        }

        return Result<Money>.Success(new(Amount - other.Amount, Currency));
    }

    /// <summary>
    /// Multiplies the monetary amount by a scalar factor, returning a <see cref="Result{T}"/>.
    /// </summary>
    public Result<Money> Multiply(decimal multiplier)
    {
        try
        {
            decimal newAmount = Amount * multiplier;
            return Result<Money>.Success(new(newAmount, Currency));
        }
        catch (OverflowException)
        {
            return Result<Money>.Failure(
                new Error(
                    "KernelR.ValueObjects.Money.OVERFLOW",
                    "The multiplication resulted in a value that is too large.",
                    ErrorType.ValidationError));
        }
    }

    /// <summary>
    /// Divides the monetary amount by a scalar divisor, returning a <see cref="Result{T}"/>.
    /// </summary>
    public Result<Money> Divide(decimal divisor)
    {
        if (divisor is 0m)
        {
            return Result<Money>.Failure(
                new Error(
                    "KernelR.ValueObjects.Money.DIVIDE_BY_ZERO",
                    "Cannot divide money by zero.",
                    ErrorType.ValidationError));
        }

        try
        {
            decimal newAmount = Amount / divisor;
            return Result<Money>.Success(new(newAmount, Currency));
        }
        catch (OverflowException)
        {
            return Result<Money>.Failure(
                new Error(
                    "KernelR.ValueObjects.Money.OVERFLOW",
                    "The division resulted in a value that is too large.",
                    ErrorType.ValidationError));
        }
    }

    // ─── Comparison Operations (Returns Result) ───

    /// <summary>
    /// Determines whether the current amount is greater than the specified amount, returning a <see cref="Result{T}"/>.
    /// </summary>
    public Result<bool> GreaterThan(Money other)
    {
        if (Currency != other.Currency)
        {
            return Result<bool>.Failure(
                new Error(
                    "KernelR.ValueObjects.Money.CURRENCY_MISMATCH",
                    $"Cannot compare amounts with different currencies: '{Currency}' vs '{other.Currency}'.",
                    ErrorType.ValidationError));
        }

        return Result<bool>.Success(Amount > other.Amount);
    }

    /// <summary>
    /// Determines whether the current amount is less than the specified amount, returning a <see cref="Result{T}"/>.
    /// </summary>
    public Result<bool> LessThan(Money other)
    {
        if (Currency != other.Currency)
        {
            return Result<bool>.Failure(
                new Error(
                    "KernelR.ValueObjects.Money.CURRENCY_MISMATCH",
                    $"Cannot compare amounts with different currencies: '{Currency}' vs '{other.Currency}'.",
                    ErrorType.ValidationError));
        }

        return Result<bool>.Success(Amount < other.Amount);
    }

    /// <summary>
    /// Determines whether the current amount is greater than or equal to the specified amount, returning a <see cref="Result{T}"/>.
    /// </summary>
    public Result<bool> GreaterThanOrEqual(Money other)
    {
        if (Currency != other.Currency)
        {
            return Result<bool>.Failure(
                new Error(
                    "KernelR.ValueObjects.Money.CURRENCY_MISMATCH",
                    $"Cannot compare amounts with different currencies: '{Currency}' vs '{other.Currency}'.",
                    ErrorType.ValidationError));
        }

        return Result<bool>.Success(Amount >= other.Amount);
    }

    /// <summary>
    /// Determines whether the current amount is less than or equal to the specified amount, returning a <see cref="Result{T}"/>.
    /// </summary>
    public Result<bool> LessThanOrEqual(Money other)
    {
        if (Currency != other.Currency)
        {
            return Result<bool>.Failure(
                new Error(
                    "KernelR.ValueObjects.Money.CURRENCY_MISMATCH",
                    $"Cannot compare amounts with different currencies: '{Currency}' vs '{other.Currency}'.",
                    ErrorType.ValidationError));
        }

        return Result<bool>.Success(Amount <= other.Amount);
    }

    #endregion ═══ Safe Functional Operations (Math & Comparison) ═══

    #region ═══ Arithmetic Operators                           ═══

    /// <summary>
    /// Adds two monetary amounts of the same currency.
    /// </summary>
    public static Money operator +(Money left, Money right)
    {
        return left.Add(right).OrThrowInvalidOperationException();
    }

    /// <summary>
    /// Subtracts one monetary amount from another of the same currency.
    /// </summary>
    public static Money operator -(Money left, Money right)
    {
        return left.Subtract(right).OrThrowInvalidOperationException();
    }

    /// <summary>
    /// Multiplies a monetary amount by a scalar factor.
    /// </summary>
    public static Money operator *(Money money, decimal multiplier)
    {
        return money.Multiply(multiplier).OrThrowInvalidOperationException();
    }

    /// <summary>
    /// Multiplies a scalar factor by a monetary amount (commutative support).
    /// </summary>
    public static Money operator *(decimal multiplier, Money money)
    {
        return money.Multiply(multiplier).OrThrowInvalidOperationException();
    }

    /// <summary>
    /// Divides a monetary amount by a scalar divisor.
    /// </summary>
    public static Money operator /(Money money, decimal divisor)
    {
        return money.Divide(divisor).OrThrowInvalidOperationException();
    }

    /// <summary>
    /// Negates a monetary amount.
    /// </summary>
    public static Money operator -(Money money)
    {
        return new(-money.Amount, money.Currency);
    }

    #endregion ═══ Arithmetic Operators                           ═══

    #region ═══ Comparison Operators (Money vs Money)          ═══

    /// <summary>Determines whether the left amount is greater than the right amount.</summary>
    public static bool operator >(Money left, Money right)
    {
        return left.GreaterThan(right).OrThrowInvalidOperationException();
    }

    /// <summary>Determines whether the left amount is less than the right amount.</summary>
    public static bool operator <(Money left, Money right)
    {
        return left.LessThan(right).OrThrowInvalidOperationException();
    }

    /// <summary>Determines whether the left amount is greater than or equal to the right amount.</summary>
    public static bool operator >=(Money left, Money right)
    {
        return left.GreaterThanOrEqual(right).OrThrowInvalidOperationException();
    }

    /// <summary>Determines whether the left amount is less than or equal to the right amount.</summary>
    public static bool operator <=(Money left, Money right)
    {
        return left.LessThanOrEqual(right).OrThrowInvalidOperationException();
    }

    #endregion ═══ Comparison Operators (Money vs Money)          ═══

    #region ═══ Comparison Operators (Money vs decimal)        ═══

    /// <summary>Determines whether the amount is greater than the given scalar.</summary>
    public static bool operator >(Money left, decimal right) => left.Amount > right;

    /// <summary>Determines whether the amount is less than the given scalar.</summary>
    public static bool operator <(Money left, decimal right) => left.Amount < right;

    /// <summary>Determines whether the amount is greater than or equal to the given scalar.</summary>
    public static bool operator >=(Money left, decimal right) => left.Amount >= right;

    /// <summary>Determines whether the amount is less than or equal to the given scalar.</summary>
    public static bool operator <=(Money left, decimal right) => left.Amount <= right;

    #endregion ═══ Comparison Operators (Money vs decimal)        ═══

    #region ═══ IComparable Implementation                     ═══

    /// <inheritdoc />
    public int CompareTo(Money other)
    {
        if (Currency != other.Currency)
        {
            return Result<int>.Failure(
                new Error(
                    "KernelR.ValueObjects.Money.CURRENCY_MISMATCH",
                    $"Cannot compare amounts with different currencies: '{Currency}' vs '{other.Currency}'.",
                    ErrorType.ValidationError))
                .OrThrowInvalidOperationException();
        }

        return Amount.CompareTo(other.Amount);
    }

    /// <inheritdoc />
    int IComparable.CompareTo(object? obj)
    {
        if (obj is null)
        {
            return 1; // Any instance is greater than null.
        }

        if (obj is not Money other)
        {
            throw new ArgumentException($"Object must be of type {nameof(Money)}.", nameof(obj));
        }

        return CompareTo(other);
    }

    #endregion ═══ IComparable Implementation                     ═══

    /// <summary>
    /// Returns a human-readable representation of the monetary amount.
    /// </summary>
    public override string ToString() => $"{Amount:N2} {Currency}";

    /// <summary>
    /// Explicitly converts a <see cref="Money"/> to its string representation.
    /// </summary>
    public static explicit operator string(Money money) => money.ToString();
}
