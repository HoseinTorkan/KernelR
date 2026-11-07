using KernelR.Domain.ValueObjects;
using KernelR.Patterns.Results;

namespace KernelR.Domain.Tests.ValueObjects;

public class EmailTests
{
    // ─────────── Create ───────────

    [Theory]
    [InlineData("user@example.com", "user@example.com")]
    [InlineData("User@Example.com", "user@example.com")]
    [InlineData("USER@EXAMPLE.COM", "user@example.com")]
    [InlineData("  user@example.com  ", "user@example.com")]
    [InlineData("user.name+tag@example.co.uk", "user.name+tag@example.co.uk")]
    [InlineData("a@b.co", "a@b.co")]
    public void Create_WithValidEmail_ReturnsNormalizedSuccess(string input, string expected)
    {
        Result<Email> result = Email.Create(input);

        Assert.True(result.IsSuccess);
        Assert.Equal(expected, result.Value.Value);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_WithEmptyInput_ReturnsEmpty(string? input)
    {
        Result<Email> result = Email.Create(input!);

        Assert.True(result.IsFailure);
        Assert.Equal("KernelR.ValueObjects.Email.EMPTY", result.Error.Code);
    }

    [Fact]
    public void Create_WithTooLongInput_ReturnsTooLong()
    {
        string localPart = new string('a', 250);
        string input = $"{localPart}@example.com";

        Result<Email> result = Email.Create(input);

        Assert.True(result.IsFailure);
        Assert.Equal("KernelR.ValueObjects.Email.TOO_LONG", result.Error.Code);
    }

    [Theory]
    [InlineData("invalid")]
    [InlineData("@example.com")]
    [InlineData("user@")]
    [InlineData("user@.com")]
    [InlineData("user@example")]
    [InlineData("user @example.com")]
    [InlineData("user@exam ple.com")]
    public void Create_WithInvalidFormat_ReturnsInvalidFormat(string input)
    {
        Result<Email> result = Email.Create(input);

        Assert.True(result.IsFailure);
        Assert.Equal("KernelR.ValueObjects.Email.INVALID_FORMAT", result.Error.Code);
    }

    // ─────────── IParsable ───────────

    [Fact]
    public void TryParse_WithValidInput_ReturnsTrue()
    {
        bool success = Email.TryParse("user@example.com", null, out Email result);

        Assert.True(success);
        Assert.Equal("user@example.com", result.Value);
    }

    [Fact]
    public void TryParse_WithInvalidInput_ReturnsFalse()
    {
        bool success = Email.TryParse("invalid", null, out Email result);

        Assert.False(success);
        Assert.Equal(default, result);
    }

    [Fact]
    public void Parse_WithValidInput_ReturnsEmail()
    {
        Email result = Email.Parse("user@example.com", null);

        Assert.Equal("user@example.com", result.Value);
    }

    [Fact]
    public void Parse_WithInvalidInput_ThrowsFormatException()
    {
        Assert.Throws<FormatException>(() => Email.Parse("invalid", null));
    }

    // ─────────── ToString & implicit operator ───────────

    [Fact]
    public void ToString_ReturnsNormalizedValue()
    {
        Email email = Email.Create("User@Example.com").Value;

        Assert.Equal("user@example.com", email.ToString());
    }

    [Fact]
    public void ImplicitOperator_ConvertsToString()
    {
        Email email = Email.Create("user@example.com").Value;

        string result = email;

        Assert.Equal("user@example.com", result);
    }
}
