using KernelR.Domain.ValueObjects;
using KernelR.Patterns.Results;

namespace KernelR.Domain.Tests.ValueObjects;

public class CurrencyCodeTests
{
    // ─────────── Create ───────────

    [Theory]
    [InlineData("IRR", "IRR")]
    [InlineData("irr", "IRR")]
    [InlineData("  USD  ", "USD")]
    [InlineData("eur", "EUR")]
    [InlineData("IRT", "IRT")]  // Non-ISO extension
    public void Create_WithValidCode_ReturnsNormalizedSuccess(string input, string expected)
    {
        Result<CurrencyCode> result = CurrencyCode.Create(input);

        Assert.True(result.IsSuccess);
        Assert.Equal(expected, result.Value.Value);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_WithEmptyInput_ReturnsEmpty(string? input)
    {
        Result<CurrencyCode> result = CurrencyCode.Create(input!);

        Assert.True(result.IsFailure);
        Assert.Equal("KernelR.ValueObjects.CurrencyCode.EMPTY", result.Error.Code);
    }

    [Theory]
    [InlineData("XYZ")]
    [InlineData("ABC")]
    [InlineData("US")]
    [InlineData("USDX")]
    public void Create_WithInvalidCode_ReturnsInvalid(string input)
    {
        Result<CurrencyCode> result = CurrencyCode.Create(input);

        Assert.True(result.IsFailure);
        Assert.Equal("KernelR.ValueObjects.CurrencyCode.INVALID", result.Error.Code);
    }

    // ─────────── IParsable ───────────

    [Fact]
    public void TryParse_WithValidInput_ReturnsTrue()
    {
        bool success = CurrencyCode.TryParse("IRR", null, out CurrencyCode result);

        Assert.True(success);
        Assert.Equal("IRR", result.Value);
    }

    [Fact]
    public void TryParse_WithInvalidInput_ReturnsFalse()
    {
        bool success = CurrencyCode.TryParse("XYZ", null, out CurrencyCode result);

        Assert.False(success);
        Assert.Equal(default, result);
    }

    [Fact]
    public void Parse_WithValidInput_ReturnsCurrencyCode()
    {
        CurrencyCode result = CurrencyCode.Parse("USD", null);

        Assert.Equal("USD", result.Value);
    }

    [Fact]
    public void Parse_WithInvalidInput_ThrowsFormatException()
    {
        Assert.Throws<FormatException>(() => CurrencyCode.Parse("XYZ", null));
    }

    // ─────────── ToString & implicit operator ───────────

    [Fact]
    public void ToString_ReturnsValue()
    {
        CurrencyCode code = CurrencyCode.Create("IRR").Value;

        Assert.Equal("IRR", code.ToString());
    }

    [Fact]
    public void ImplicitOperator_ConvertsToString()
    {
        CurrencyCode code = CurrencyCode.Create("USD").Value;

        string result = code;

        Assert.Equal("USD", result);
    }
}
