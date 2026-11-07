using KernelR.Domain.ValueObjects;
using KernelR.Patterns.Results;

namespace KernelR.Domain.Tests.ValueObjects;

public class NationalCodeTests
{
    // ─────────── Create ───────────

    [Theory]
    [InlineData("0013542419", "0013542419")]
    [InlineData("001-3542419", "0013542419")]
    [InlineData("001 3542419", "0013542419")]
    public void Create_WithValidCode_ReturnsNormalizedSuccess(string input, string expected)
    {
        Result<NationalCode> result = NationalCode.Create(input);

        Assert.True(result.IsSuccess);
        Assert.Equal(expected, result.Value.Value);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_WithEmptyInput_ReturnsEmpty(string? input)
    {
        Result<NationalCode> result = NationalCode.Create(input!);

        Assert.True(result.IsFailure);
        Assert.Equal("KernelR.ValueObjects.NationalCode.EMPTY", result.Error.Code);
    }

    [Theory]
    [InlineData("123")]
    [InlineData("12345678901")]
    [InlineData("abcdefghij")]
    public void Create_WithInvalidFormat_ReturnsInvalidFormat(string input)
    {
        Result<NationalCode> result = NationalCode.Create(input);

        Assert.True(result.IsFailure);
        Assert.Equal("KernelR.ValueObjects.NationalCode.INVALID_FORMAT", result.Error.Code);
    }

    [Theory]
    [InlineData("1111111111")]
    [InlineData("0000000000")]
    [InlineData("2222222222")]
    public void Create_WithAllIdenticalDigits_ReturnsInvalidChecksum(string input)
    {
        Result<NationalCode> result = NationalCode.Create(input);

        Assert.True(result.IsFailure);
        Assert.Equal("KernelR.ValueObjects.NationalCode.INVALID_CHECKSUM", result.Error.Code);
    }

    [Theory]
    [InlineData("1234567890")]  // checksum fails
    public void Create_WithInvalidChecksum_ReturnsInvalidChecksum(string input)
    {
        Result<NationalCode> result = NationalCode.Create(input);

        Assert.True(result.IsFailure);
        Assert.Equal("KernelR.ValueObjects.NationalCode.INVALID_CHECKSUM", result.Error.Code);
    }

    // ─────────── IParsable ───────────

    [Fact]
    public void TryParse_WithValidInput_ReturnsTrue()
    {
        bool success = NationalCode.TryParse("0013542419", null, out NationalCode result);

        Assert.True(success);
        Assert.Equal("0013542419", result.Value);
    }

    [Fact]
    public void TryParse_WithInvalidInput_ReturnsFalse()
    {
        bool success = NationalCode.TryParse("123", null, out NationalCode result);

        Assert.False(success);
        Assert.Equal(default, result);
    }

    [Fact]
    public void Parse_WithValidInput_ReturnsNationalCode()
    {
        NationalCode result = NationalCode.Parse("0013542419", null);

        Assert.Equal("0013542419", result.Value);
    }

    [Fact]
    public void Parse_WithInvalidInput_ThrowsFormatException()
    {
        Assert.Throws<FormatException>(() => NationalCode.Parse("123", null));
    }

    // ─────────── ToString & implicit operator ───────────

    [Fact]
    public void ToString_ReturnsValue()
    {
        NationalCode code = NationalCode.Create("0013542419").Value;

        Assert.Equal("0013542419", code.ToString());
    }

    [Fact]
    public void ImplicitOperator_ConvertsToString()
    {
        NationalCode code = NationalCode.Create("0013542419").Value;

        string result = code;

        Assert.Equal("0013542419", result);
    }
}
