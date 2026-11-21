using KernelR.Domain.ValueObjects.Financial;
using KernelR.Patterns.Results;

namespace KernelR.Domain.Tests.ValueObjects.Financial;

public class MoneyTests
{
    [Theory]
    [InlineData(100, "USD", 100)]
    [InlineData(-50.25, "EUR", -50.25)]
    [InlineData(10.555, "IRR", 10.56)]
    [InlineData(10.554, "IRR", 10.55)]
    [InlineData(0, "IRT", 0)]
    public void Create_ValidAmountAndCurrency_ReturnsSuccess(decimal amount, string currency, decimal expectedAmount)
    {
        Result<Money> result = Money.Create(amount, currency);

        Assert.True(result.IsSuccess);
        Assert.Equal(expectedAmount, result.Value.Amount);
        Assert.Equal(currency, result.Value.Currency.Value);
    }

    [Fact]
    public void Create_InvalidCurrency_ReturnsFailure()
    {
        Result<Money> result = Money.Create(100, "XYZ");

        Assert.True(result.IsFailure);
        Assert.Equal("KernelR.ValueObjects.CurrencyCode.INVALID", result.Error.Code);
    }

    [Theory]
    [InlineData("100 USD", 100, "USD")]
    [InlineData(" -50.25   EUR ", -50.25, "EUR")]
    [InlineData("0 IRR", 0, "IRR")]
    public void Create_FromString_ValidFormat_ReturnsSuccess(string input, decimal expectedAmount, string expectedCurrency)
    {
        Result<Money> result = Money.Create(input);

        Assert.True(result.IsSuccess);
        Assert.Equal(expectedAmount, result.Value.Amount);
        Assert.Equal(expectedCurrency, result.Value.Currency.Value);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_FromString_NullOrWhitespace_ReturnsEmptyError(string? input)
    {
        Result<Money> result = Money.Create(input!);

        Assert.True(result.IsFailure);
        Assert.Equal("KernelR.ValueObjects.Money.EMPTY", result.Error.Code);
    }

    [Theory]
    [InlineData("100")]
    [InlineData("100 USD extra")]
    [InlineData("abc USD")]
    [InlineData("100 XYZ")]
    public void Create_FromString_InvalidFormatOrValue_ReturnsFailure(string input)
    {
        Result<Money> result = Money.Create(input);

        Assert.True(result.IsFailure);
        Assert.Contains("INVALID", result.Error.Code);
    }

    [Fact]
    public void Zero_ValidCurrency_ReturnsZeroAmount()
    {
        Result<Money> result = Money.Zero("USD");

        Assert.True(result.IsSuccess);
        Assert.Equal(0, result.Value.Amount);
        Assert.Equal("USD", result.Value.Currency.Value);
    }

    [Fact]
    public void Properties_IsZeroIsPositiveIsNegative_ReturnCorrectValues()
    {
        Money zero = Money.Create(0, "USD").Value;
        Money positive = Money.Create(10, "USD").Value;
        Money negative = Money.Create(-10, "USD").Value;

        Assert.True(zero.IsZero);
        Assert.False(zero.IsPositive);
        Assert.False(zero.IsNegative);

        Assert.False(positive.IsZero);
        Assert.True(positive.IsPositive);
        Assert.False(positive.IsNegative);

        Assert.False(negative.IsZero);
        Assert.False(negative.IsPositive);
        Assert.True(negative.IsNegative);
    }

    [Theory]
    [InlineData("100 USD", 100, "USD")]
    [InlineData("-50.25 EUR", -50.25, "EUR")]
    public void Parse_ValidString_ReturnsMoney(string input, decimal expectedAmount, string expectedCurrency)
    {
        Money money = Money.Parse(input, null);

        Assert.Equal(expectedAmount, money.Amount);
        Assert.Equal(expectedCurrency, money.Currency.Value);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("invalid")]
    public void Parse_InvalidString_ThrowsFormatException(string? input)
    {
        Assert.Throws<FormatException>(() => Money.Parse(input!, null));
    }

    [Theory]
    [InlineData("100 USD", true)]
    [InlineData(" -50.25 EUR ", true)]
    [InlineData(null, false)]
    [InlineData("", false)]
    [InlineData("invalid", false)]
    public void TryParse_VariousInputs_ReturnsExpectedResult(string? input, bool expectedSuccess)
    {
        bool success = Money.TryParse(input, null, out Money result);

        Assert.Equal(expectedSuccess, success);
        if (expectedSuccess)
        {
            Assert.NotEqual(default, result);
        }
        else
        {
            Assert.Equal(default, result);
        }
    }

    [Fact]
    public void Add_SameCurrency_ReturnsSum()
    {
        Money m1 = Money.Create(100, "USD").Value;
        Money m2 = Money.Create(50.50m, "USD").Value;

        Result<Money> result = m1.Add(m2);

        Assert.True(result.IsSuccess);
        Assert.Equal(150.50m, result.Value.Amount);
        Assert.Equal("USD", result.Value.Currency.Value);
    }

    [Fact]
    public void Add_DifferentCurrencies_ReturnsCurrencyMismatchError()
    {
        Money usd = Money.Create(100, "USD").Value;
        Money eur = Money.Create(100, "EUR").Value;

        Result<Money> result = usd.Add(eur);

        Assert.True(result.IsFailure);
        Assert.Equal("KernelR.ValueObjects.Money.CURRENCY_MISMATCH", result.Error.Code);
    }

    [Fact]
    public void Subtract_SameCurrency_ReturnsDifference()
    {
        Money m1 = Money.Create(100, "USD").Value;
        Money m2 = Money.Create(30, "USD").Value;

        Result<Money> result = m1.Subtract(m2);

        Assert.True(result.IsSuccess);
        Assert.Equal(70, result.Value.Amount);
    }

    [Fact]
    public void Subtract_DifferentCurrencies_ReturnsCurrencyMismatchError()
    {
        Money usd = Money.Create(100, "USD").Value;
        Money eur = Money.Create(100, "EUR").Value;

        Result<Money> result = usd.Subtract(eur);

        Assert.True(result.IsFailure);
        Assert.Equal("KernelR.ValueObjects.Money.CURRENCY_MISMATCH", result.Error.Code);
    }

    [Fact]
    public void Multiply_ValidMultiplier_ReturnsProduct()
    {
        Money money = Money.Create(100, "USD").Value;

        Result<Money> result = money.Multiply(1.5m);

        Assert.True(result.IsSuccess);
        Assert.Equal(150, result.Value.Amount);
    }

    [Fact]
    public void Multiply_Overflow_ReturnsOverflowError()
    {
        Money money = Money.Create(decimal.MaxValue, "USD").Value;

        Result<Money> result = money.Multiply(2m);

        Assert.True(result.IsFailure);
        Assert.Equal("KernelR.ValueObjects.Money.OVERFLOW", result.Error.Code);
    }

    [Fact]
    public void Divide_ValidDivisor_ReturnsQuotient()
    {
        Money money = Money.Create(100, "USD").Value;

        Result<Money> result = money.Divide(4m);

        Assert.True(result.IsSuccess);
        Assert.Equal(25, result.Value.Amount);
    }

    [Fact]
    public void Divide_ByZero_ReturnsDivideByZeroError()
    {
        Money money = Money.Create(100, "USD").Value;

        Result<Money> result = money.Divide(0);

        Assert.True(result.IsFailure);
        Assert.Equal("KernelR.ValueObjects.Money.DIVIDE_BY_ZERO", result.Error.Code);
    }

    [Fact]
    public void GreaterThan_ValidComparisons_ReturnsCorrectBool()
    {
        Money m1 = Money.Create(100, "USD").Value;
        Money m2 = Money.Create(50, "USD").Value;

        Assert.True(m1.GreaterThan(m2).Value);
        Assert.False(m2.GreaterThan(m1).Value);
    }

    [Fact]
    public void GreaterThan_DifferentCurrencies_ReturnsCurrencyMismatchError()
    {
        Money usd = Money.Create(100, "USD").Value;
        Money eur = Money.Create(50, "EUR").Value;

        Result<bool> result = usd.GreaterThan(eur);

        Assert.True(result.IsFailure);
        Assert.Equal("KernelR.ValueObjects.Money.CURRENCY_MISMATCH", result.Error.Code);
    }

    [Fact]
    public void LessThan_ValidComparisons_ReturnsCorrectBool()
    {
        Money m1 = Money.Create(100, "USD").Value;
        Money m2 = Money.Create(50, "USD").Value;

        Assert.False(m1.LessThan(m2).Value);
        Assert.True(m2.LessThan(m1).Value);
    }

    [Fact]
    public void LessThanOrEqual_And_GreaterThanOrEqual_ReturnCorrectBool()
    {
        Money m1 = Money.Create(100, "USD").Value;
        Money m2 = Money.Create(100, "USD").Value;
        Money m3 = Money.Create(50, "USD").Value;

        Assert.True(m1.GreaterThanOrEqual(m2).Value);
        Assert.True(m1.GreaterThanOrEqual(m3).Value);
        Assert.False(m3.GreaterThanOrEqual(m1).Value);

        Assert.True(m1.LessThanOrEqual(m2).Value);
        Assert.True(m3.LessThanOrEqual(m1).Value);
        Assert.False(m1.LessThanOrEqual(m3).Value);
    }

    [Fact]
    public void Operator_Plus_ReturnsSum()
    {
        Money m1 = Money.Create(100, "USD").Value;
        Money m2 = Money.Create(50, "USD").Value;

        Money result = m1 + m2;

        Assert.Equal(150, result.Amount);
    }

    [Fact]
    public void Operator_Plus_DifferentCurrencies_ThrowsInvalidOperationException()
    {
        Money usd = Money.Create(100, "USD").Value;
        Money eur = Money.Create(50, "EUR").Value;

        Assert.Throws<InvalidOperationException>(() => usd + eur);
    }

    [Fact]
    public void Operator_Minus_ReturnsDifference()
    {
        Money m1 = Money.Create(100, "USD").Value;
        Money m2 = Money.Create(50, "USD").Value;

        Money result = m1 - m2;

        Assert.Equal(50, result.Amount);
    }

    [Fact]
    public void Operator_Multiply_ReturnsProduct()
    {
        Money money = Money.Create(100, "USD").Value;

        Money result1 = money * 2;
        Money result2 = 2 * money;

        Assert.Equal(200, result1.Amount);
        Assert.Equal(200, result2.Amount);
    }

    [Fact]
    public void Operator_Divide_ReturnsQuotient()
    {
        Money money = Money.Create(100, "USD").Value;

        Money result = money / 4;

        Assert.Equal(25, result.Amount);
    }

    [Fact]
    public void Operator_Divide_ByZero_ThrowsInvalidOperationException()
    {
        Money money = Money.Create(100, "USD").Value;

        Assert.Throws<InvalidOperationException>(() => money / 0);
    }

    [Fact]
    public void Operator_UnaryMinus_NegatesAmount()
    {
        Money money = Money.Create(100, "USD").Value;

        Money result = -money;

        Assert.Equal(-100, result.Amount);
    }

    [Fact]
    public void ComparisonOperators_MoneyVsMoney_ReturnCorrectBool()
    {
        Money m1 = Money.Create(100, "USD").Value;
        Money m2 = Money.Create(50, "USD").Value;

        Assert.True(m1 > m2);
        Assert.False(m1 < m2);
        Assert.True(m1 >= m2);
        Assert.False(m1 <= m2);
    }

    [Fact]
    public void ComparisonOperators_MoneyVsDecimal_ReturnCorrectBool()
    {
        Money money = Money.Create(100, "USD").Value;

        Assert.True(money > 50m);
        Assert.False(money < 50m);
        Assert.True(money >= 100m);
        Assert.True(money <= 100m);
    }

    [Fact]
    public void CompareTo_SameCurrency_ReturnsIntegerComparison()
    {
        Money m1 = Money.Create(100, "USD").Value;
        Money m2 = Money.Create(50, "USD").Value;
        Money m3 = Money.Create(100, "USD").Value;

        Assert.True(m1.CompareTo(m2) > 0);
        Assert.True(m2.CompareTo(m1) < 0);
        Assert.Equal(0, m1.CompareTo(m3));
    }

    [Fact]
    public void CompareTo_DifferentCurrencies_ThrowsInvalidOperationException()
    {
        Money usd = Money.Create(100, "USD").Value;
        Money eur = Money.Create(100, "EUR").Value;

        Assert.Throws<InvalidOperationException>(() => usd.CompareTo(eur));
    }

    [Fact]
    public void IComparableCompareTo_WithNull_ReturnsOne()
    {
        Money money = Money.Create(100, "USD").Value;
        IComparable comparable = money;

        Assert.Equal(1, comparable.CompareTo(null));
    }

    [Fact]
    public void IComparableCompareTo_WithInvalidType_ThrowsArgumentException()
    {
        Money money = Money.Create(100, "USD").Value;
        IComparable comparable = money;

        Assert.Throws<ArgumentException>(() => comparable.CompareTo("NotMoney"));
    }

    [Fact]
    public void Abs_ReturnsAbsoluteValue()
    {
        Money negative = Money.Create(-100, "USD").Value;
        Money positive = Money.Create(100, "USD").Value;

        Assert.Equal(100, negative.Abs().Amount);
        Assert.Equal(100, positive.Abs().Amount);
    }

    [Fact]
    public void ToString_ReturnsFormattedString()
    {
        Money money = Money.Create(1234.56m, "USD").Value;

        string result = money.ToString();

        Assert.Equal("1,234.56 USD", result);
    }

    [Fact]
    public void ExplicitOperator_ReturnsString()
    {
        Money money = Money.Create(100, "USD").Value;

        string result = (string)money;

        Assert.Equal("100.00 USD", result);
    }

    [Fact]
    public void Equality_SameAmountAndCurrency_ReturnsTrue()
    {
        Money m1 = Money.Create(100, "USD").Value;
        Money m2 = Money.Create(100, "USD").Value;

        Assert.Equal(m1, m2);
        Assert.True(m1 == m2);
        Assert.False(m1 != m2);
    }

    [Fact]
    public void Equality_DifferentAmountOrCurrency_ReturnsFalse()
    {
        Money m1 = Money.Create(100, "USD").Value;
        Money m2 = Money.Create(100, "EUR").Value;
        Money m3 = Money.Create(50, "USD").Value;

        Assert.NotEqual(m1, m2);
        Assert.NotEqual(m1, m3);
    }
}
