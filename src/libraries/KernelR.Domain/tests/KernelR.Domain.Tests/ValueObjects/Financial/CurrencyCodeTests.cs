using KernelR.Domain.ValueObjects.Financial;
using KernelR.Patterns.Results;

namespace KernelR.Domain.Tests.ValueObjects.Financial;

public class CurrencyCodeTests
{
    [Theory]
    [InlineData("USD")]
    [InlineData("EUR")]
    [InlineData("IRR")]
    [InlineData("GBP")]
    [InlineData("IRT")]
    public void Create_ValidCode_ReturnsSuccess(string validCode)
    {
        Result<CurrencyCode> result = CurrencyCode.Create(validCode);

        Assert.True(result.IsSuccess);
        Assert.Equal(validCode, result.Value.Value);
    }

    [Theory]
    [InlineData("usd", "USD")]
    [InlineData("  eur  ", "EUR")]
    [InlineData("irt", "IRT")]
    [InlineData(" gBp ", "GBP")]
    public void Create_ValidCode_NormalizesToUppercaseAndTrims(string input, string expectedValue)
    {
        Result<CurrencyCode> result = CurrencyCode.Create(input);

        Assert.True(result.IsSuccess);
        Assert.Equal(expectedValue, result.Value.Value);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_NullOrWhitespace_ReturnsEmptyError(string? input)
    {
        Result<CurrencyCode> result = CurrencyCode.Create(input!);

        Assert.True(result.IsFailure);
        Assert.Equal("KernelR.ValueObjects.CurrencyCode.EMPTY", result.Error.Code);
        Assert.Equal("Currency code cannot be empty.", result.Error.Message);
    }

    [Theory]
    [InlineData("XYZ")]
    [InlineData("AAA")]
    [InlineData("US")]
    [InlineData("USDD")]
    [InlineData("123")]
    [InlineData("U$D")]
    [InlineData("US D")]
    public void Create_InvalidCode_ReturnsInvalidError(string invalidCode)
    {
        Result<CurrencyCode> result = CurrencyCode.Create(invalidCode);

        Assert.True(result.IsFailure);
        Assert.Equal("KernelR.ValueObjects.CurrencyCode.INVALID", result.Error.Code);
        Assert.Equal("Currency code is not a valid ISO 4217 code.", result.Error.Message);
    }

    [Fact]
    public void Parse_ValidCode_ReturnsCurrencyCode()
    {
        string input = "  usd  ";

        CurrencyCode code = CurrencyCode.Parse(input, null);

        Assert.Equal("USD", code.Value);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("XYZ")]
    [InlineData("US")]
    public void Parse_InvalidCode_ThrowsFormatException(string? input)
    {
        Assert.Throws<FormatException>(() => CurrencyCode.Parse(input!, null));
    }

    [Theory]
    [InlineData("USD", true, "USD")]
    [InlineData("  irt  ", true, "IRT")]
    [InlineData(null, false, null)]
    [InlineData("", false, null)]
    [InlineData("XYZ", false, null)]
    [InlineData("US", false, null)]
    public void TryParse_VariousInputs_ReturnsExpectedResult(string? input, bool expectedSuccess, string? expectedValue)
    {
        bool success = CurrencyCode.TryParse(input, null, out CurrencyCode result);

        Assert.Equal(expectedSuccess, success);
        if (expectedSuccess)
        {
            Assert.NotNull(result.Value);
            Assert.Equal(expectedValue, result.Value);
        }
        else
        {
            Assert.Equal(default, result);
        }
    }

    [Fact]
    public void ToString_ReturnsValue()
    {
        CurrencyCode code = CurrencyCode.Create("USD").Value;

        string result = code.ToString();

        Assert.Equal("USD", result);
    }

    [Fact]
    public void ExplicitOperator_ReturnsValue()
    {
        CurrencyCode code = CurrencyCode.Create("EUR").Value;

        string result = (string)code;

        Assert.Equal("EUR", result);
    }

    [Fact]
    public void Equality_SameNormalizedCode_ReturnsTrue()
    {
        CurrencyCode code1 = CurrencyCode.Create("USD").Value;
        CurrencyCode code2 = CurrencyCode.Create("  usd  ").Value;

        Assert.Equal(code1, code2);
        Assert.True(code1 == code2);
        Assert.False(code1 != code2);
    }

    [Fact]
    public void Equality_DifferentCodes_ReturnsFalse()
    {
        CurrencyCode code1 = CurrencyCode.Create("USD").Value;
        CurrencyCode code2 = CurrencyCode.Create("EUR").Value;

        Assert.NotEqual(code1, code2);
        Assert.False(code1 == code2);
        Assert.True(code1 != code2);
    }
}
