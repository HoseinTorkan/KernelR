using KernelR.Domain.ValueObjects;
using KernelR.Patterns.Results;

namespace KernelR.Domain.Tests.ValueObjects;

public class PhoneNumberTests
{
    // ─────────── Create ───────────

    [Theory]
    [InlineData("09121234567")]
    [InlineData("+989121234567")]
    [InlineData("00989121234567")]
    [InlineData("989121234567")]
    [InlineData("0912-123-4567")]
    [InlineData("0912 123 4567")]
    [InlineData("(0912) 123-4567")]
    [InlineData("  09121234567  ")]
    public void Create_WithValidInput_ReturnsSuccess(string input)
    {
        Result<PhoneNumber> result = PhoneNumber.Create(input);

        Assert.True(result.IsSuccess);
        Assert.Equal("09121234567", result.Value.Value);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_WithEmptyInput_ReturnsEmpty(string? input)
    {
        Result<PhoneNumber> result = PhoneNumber.Create(input!);

        Assert.True(result.IsFailure);
        Assert.Equal("KernelR.ValueObjects.PhoneNumber.EMPTY", result.Error.Code);
    }

    [Theory]
    [InlineData("123")]
    [InlineData("0912123456")]     // 10 digits
    [InlineData("091212345678")]   // 12 digits
    [InlineData("08121234567")]    // doesn't start with 09
    [InlineData("abcdefghijk")]
    public void Create_WithInvalidFormat_ReturnsInvalidFormat(string input)
    {
        Result<PhoneNumber> result = PhoneNumber.Create(input);

        Assert.True(result.IsFailure);
        Assert.Equal("KernelR.ValueObjects.PhoneNumber.INVALID_FORMAT", result.Error.Code);
    }

    // ─────────── IParsable ───────────

    [Fact]
    public void TryParse_WithValidInput_ReturnsTrue()
    {
        bool success = PhoneNumber.TryParse("09121234567", null, out PhoneNumber result);

        Assert.True(success);
        Assert.Equal("09121234567", result.Value);
    }

    [Fact]
    public void TryParse_WithInvalidInput_ReturnsFalse()
    {
        bool success = PhoneNumber.TryParse("123", null, out PhoneNumber result);

        Assert.False(success);
        Assert.Equal(default, result);
    }

    [Fact]
    public void Parse_WithValidInput_ReturnsPhoneNumber()
    {
        PhoneNumber result = PhoneNumber.Parse("09121234567", null);

        Assert.Equal("09121234567", result.Value);
    }

    [Fact]
    public void Parse_WithInvalidInput_ThrowsFormatException()
    {
        Assert.Throws<FormatException>(() => PhoneNumber.Parse("123", null));
    }

    // ─────────── ToString & implicit operator ───────────

    [Fact]
    public void ToString_ReturnsNormalizedValue()
    {
        PhoneNumber phone = PhoneNumber.Create("+989121234567").Value;

        Assert.Equal("09121234567", phone.ToString());
    }

    [Fact]
    public void ImplicitOperator_ConvertsToString()
    {
        PhoneNumber phone = PhoneNumber.Create("09121234567").Value;

        string result = phone;

        Assert.Equal("09121234567", result);
    }
}
