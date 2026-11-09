using KernelR.Domain.ValueObjects.Contact;
using KernelR.Patterns.Results;

namespace KernelR.Domain.Tests.ValueObjects.Contact;

public class EmailTests
{
    [Theory]
    [InlineData("test@example.com")]
    [InlineData("user.name@example.com")]
    [InlineData("user+tag@example.com")]
    [InlineData("user_name@sub.example.co.uk")]
    [InlineData("1234567890@example.com")]
    [InlineData("test-user@example-domain.com")]
    [InlineData("a@b.co")]
    public void Create_ValidEmail_ReturnsSuccess(string validEmail)
    {
        Result<Email> result = Email.Create(validEmail);

        Assert.True(result.IsSuccess);
        Assert.Equal(validEmail, result.Value.Value);
    }

    [Theory]
    [InlineData("TEST@EXAMPLE.COM", "test@example.com")]
    [InlineData("  User@Example.Com  ", "user@example.com")]
    [InlineData("User.Name+Tag@Sub.Domain.com", "user.name+tag@sub.domain.com")]
    public void Create_ValidEmail_NormalizesToLowercaseAndTrims(string input, string expectedValue)
    {
        Result<Email> result = Email.Create(input);

        Assert.True(result.IsSuccess);
        Assert.Equal(expectedValue, result.Value.Value);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_NullOrWhitespace_ReturnsEmptyError(string? input)
    {
        Result<Email> result = Email.Create(input!);

        Assert.True(result.IsFailure);
        Assert.Equal("KernelR.ValueObjects.Email.EMPTY", result.Error.Code);
        Assert.Equal("Email cannot be empty.", result.Error.Text);
    }

    [Fact]
    public void Create_TooLong_ReturnsTooLongError()
    {
        string localPart = new('a', 243);
        string tooLongEmail = $"{localPart}@example.com";

        Result<Email> result = Email.Create(tooLongEmail);

        Assert.True(result.IsFailure);
        Assert.Equal("KernelR.ValueObjects.Email.TOO_LONG", result.Error.Code);
        Assert.Equal("Email is too long.", result.Error.Text);
    }

    [Theory]
    [InlineData("plainaddress")]
    [InlineData("@missinglocal.com")]
    [InlineData("missingdomain@")]
    [InlineData("missingatsign.com")]
    [InlineData("user@domain")]
    [InlineData("user@domain.")]
    [InlineData("user@.domain.com")]
    [InlineData("user@domain..com")]
    [InlineData("user name@domain.com")]
    [InlineData("user@domain,com")]
    public void Create_InvalidFormat_ReturnsInvalidFormatError(string invalidEmail)
    {
        Result<Email> result = Email.Create(invalidEmail);

        Assert.True(result.IsFailure);
        Assert.Equal("KernelR.ValueObjects.Email.INVALID_FORMAT", result.Error.Code);
        Assert.Equal("Email format is invalid.", result.Error.Text);
    }

    [Fact]
    public void Parse_ValidEmail_ReturnsEmail()
    {
        string input = "Test@Example.com";

        Email email = Email.Parse(input, null);

        Assert.Equal("test@example.com", email.Value);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("invalid-email")]
    public void Parse_InvalidEmail_ThrowsFormatException(string? input)
    {
        Assert.Throws<FormatException>(() => Email.Parse(input!, null));
    }

    [Fact]
    public void TryParse_ValidEmail_ReturnsTrueAndOutValue()
    {
        string input = "Test@Example.com";

        bool success = Email.TryParse(input, null, out Email result);

        Assert.True(success);
        Assert.Equal("test@example.com", result.Value);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("invalid-email")]
    [InlineData("   ")]
    public void TryParse_InvalidEmail_ReturnsFalseAndDefaultValue(string? input)
    {
        bool success = Email.TryParse(input, null, out Email result);

        Assert.False(success);
        Assert.Equal(default, result);
    }

    [Fact]
    public void ToString_ReturnsValue()
    {
        Email email = Email.Create("Test@Example.com").Value;

        string result = email.ToString();

        Assert.Equal("test@example.com", result);
    }

    [Fact]
    public void ExplicitOperator_ReturnsValue()
    {
        Email email = Email.Create("Test@Example.com").Value;

        string result = (string)email;

        Assert.Equal("test@example.com", result);
    }

    [Fact]
    public void Equality_SameEmail_ReturnsTrue()
    {
        Email email1 = Email.Create("Test@Example.com").Value;
        Email email2 = Email.Create("test@example.com").Value;

        Assert.Equal(email1, email2);
        Assert.True(email1 == email2);
        Assert.False(email1 != email2);
    }

    [Fact]
    public void Equality_DifferentEmail_ReturnsFalse()
    {
        Email email1 = Email.Create("test1@example.com").Value;
        Email email2 = Email.Create("test2@example.com").Value;

        Assert.NotEqual(email1, email2);
        Assert.False(email1 == email2);
        Assert.True(email1 != email2);
    }
}
