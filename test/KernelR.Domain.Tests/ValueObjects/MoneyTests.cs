using KernelR.Domain.ValueObjects;
using KernelR.Patterns.Results;

namespace KernelR.Domain.Tests.ValueObjects;

public class MoneyTests
{
    // ─────────── Create(decimal, string) ───────────

    [Theory]
    [InlineData(100, "IRR", 100, "IRR")]
    [InlineData(-50.25, "usd", -50.25, "USD")]
    [InlineData(0, "EUR", 0, "EUR")]
    public void Create_WithValidInput_ReturnsSuccess(decimal amount, string currency, decimal expectedAmount, string expectedCurrency)
    {
        Result<Money> result = Money.Create(amount, currency);

        Assert.True(result.IsSuccess);
        Assert.Equal(expectedAmount, result.Value.Amount);
        Assert.Equal(expectedCurrency, result.Value.Currency.Value);
    }

    [Fact]
    public void Create_RoundsToTwoDecimals()
    {
        Result<Money> result = Money.Create(5.555m, "IRR");

        Assert.True(result.IsSuccess);
        Assert.Equal(5.56m, result.Value.Amount);
    }

    [Fact]
    public void Create_WithInvalidCurrency_ReturnsFailure()
    {
        Result<Money> result = Money.Create(100, "XYZ");

        Assert.True(result.IsFailure);
        Assert.Equal("KernelR.ValueObjects.CurrencyCode.INVALID", result.Error.Code);
    }

    // ─────────── Create(string) ───────────

    [Theory]
    [InlineData("100 IRR", 100, "IRR")]
    [InlineData("-50.25 USD", -50.25, "USD")]
    [InlineData("  0 EUR  ", 0, "EUR")]
    public void Create_WithValidString_ReturnsSuccess(string input, decimal expectedAmount, string expectedCurrency)
    {
        Result<Money> result = Money.Create(input);

        Assert.True(result.IsSuccess);
        Assert.Equal(expectedAmount, result.Value.Amount);
        Assert.Equal(expectedCurrency, result.Value.Currency.Value);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_WithEmptyString_ReturnsEmpty(string? input)
    {
        Result<Money> result = Money.Create(input!);

        Assert.True(result.IsFailure);
        Assert.Equal("KernelR.ValueObjects.Money.EMPTY", result.Error.Code);
    }

    [Theory]
    [InlineData("100")]
    [InlineData("100 IRR extra")]
    public void Create_WithInvalidFormat_ReturnsInvalidFormat(string input)
    {
        Result<Money> result = Money.Create(input);

        Assert.True(result.IsFailure);
        Assert.Equal("KernelR.ValueObjects.Money.INVALID_FORMAT", result.Error.Code);
    }

    [Fact]
    public void Create_WithInvalidNumber_ReturnsInvalidValue()
    {
        Result<Money> result = Money.Create("abc IRR");

        Assert.True(result.IsFailure);
        Assert.Equal("KernelR.ValueObjects.Money.INVALID_VALUE", result.Error.Code);
    }

    // ─────────── IParsable ───────────

    [Fact]
    public void TryParse_WithValidInput_ReturnsTrue()
    {
        bool success = Money.TryParse("100 IRR", null, out Money result);

        Assert.True(success);
        Assert.Equal(100, result.Amount);
    }

    [Fact]
    public void TryParse_WithInvalidInput_ReturnsFalse()
    {
        bool success = Money.TryParse("invalid", null, out Money result);

        Assert.False(success);
        Assert.Equal(default, result);
    }

    [Fact]
    public void Parse_WithValidInput_ReturnsMoney()
    {
        Money result = Money.Parse("100 IRR", null);

        Assert.Equal(100, result.Amount);
    }

    [Fact]
    public void Parse_WithInvalidInput_ThrowsFormatException()
    {
        Assert.Throws<FormatException>(() => Money.Parse("invalid", null));
    }

    // ─────────── Zero ───────────

    [Fact]
    public void Zero_ReturnsZeroAmountInCurrency()
    {
        Result<Money> result = Money.Zero("IRR");

        Assert.True(result.IsSuccess);
        Assert.Equal(0, result.Value.Amount);
        Assert.Equal("IRR", result.Value.Currency.Value);
    }

    // ─────────── Arithmetic Operators ───────────

    [Fact]
    public void Add_SameCurrency_ReturnsSum()
    {
        Money left = Money.Create(100, "IRR").Value;
        Money right = Money.Create(50, "IRR").Value;

        Money result = left + right;

        Assert.Equal(150, result.Amount);
    }

    [Fact]
    public void Add_DifferentCurrency_ThrowsInvalidOperationException()
    {
        Money left = Money.Create(100, "IRR").Value;
        Money right = Money.Create(50, "USD").Value;

        Assert.Throws<InvalidOperationException>(() => left + right);
    }

    [Fact]
    public void Subtract_SameCurrency_ReturnsDifference()
    {
        Money left = Money.Create(100, "IRR").Value;
        Money right = Money.Create(30, "IRR").Value;

        Money result = left - right;

        Assert.Equal(70, result.Amount);
    }

    [Fact]
    public void Multiply_ReturnsProduct()
    {
        Money money = Money.Create(100, "IRR").Value;

        Money result = money * 2.5m;

        Assert.Equal(250, result.Amount);
    }

    [Fact]
    public void Divide_ReturnsQuotient()
    {
        Money money = Money.Create(100, "IRR").Value;

        Money result = money / 4m;

        Assert.Equal(25, result.Amount);
    }

    [Fact]
    public void Divide_ByZero_ThrowsDivideByZeroException()
    {
        Money money = Money.Create(100, "IRR").Value;

        Assert.Throws<DivideByZeroException>(() => money / 0m);
    }

    [Fact]
    public void Negate_ReturnsNegatedAmount()
    {
        Money money = Money.Create(100, "IRR").Value;

        Money result = -money;

        Assert.Equal(-100, result.Amount);
    }

    // ─────────── Comparison Operators ───────────

    [Fact]
    public void Comparison_SameCurrency_ComparesAmounts()
    {
        Money a = Money.Create(100, "IRR").Value;
        Money b = Money.Create(50, "IRR").Value;

        Assert.True(a > b);
        Assert.False(a < b);
        Assert.True(a >= b);
        Assert.False(a <= b);
    }

    [Fact]
    public void Comparison_DifferentCurrency_ThrowsInvalidOperationException()
    {
        Money a = Money.Create(100, "IRR").Value;
        Money b = Money.Create(50, "USD").Value;

        Assert.Throws<InvalidOperationException>(() => a > b);
    }

    // ─────────── ToString ───────────

    [Fact]
    public void ToString_ReturnsFormattedValue()
    {
        Money money = Money.Create(1234.56m, "IRR").Value;

        // Format depends on culture; we just check it contains both parts
        Assert.Contains("IRR", money.ToString());
        Assert.Contains("1", money.ToString());
    }
}
