using KernelR.Domain.ValueObjects;
using KernelR.Patterns.Results;

namespace KernelR.Domain.Tests.ValueObjects.Contact;

public class PhoneNumberTests
{
    [Theory]
    [InlineData("09121234567")]
    [InlineData("+989121234567")]
    [InlineData("00989121234567")]
    [InlineData("989121234567")]
    [InlineData("0912-123-4567")]
    [InlineData("0912 123 4567")]
    [InlineData("(0912) 123-4567")]
    [InlineData(" 09121234567 ")]
    public void Create_ValidPhoneNumber_NormalizesAndReturnsSuccess(string input)
    {
        Result<PhoneNumber> result = PhoneNumber.Create(input);

        Assert.True(result.IsSuccess);
        Assert.Equal("09121234567", result.Value.Value);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_NullOrWhitespace_ReturnsEmptyError(string? input)
    {
        Result<PhoneNumber> result = PhoneNumber.Create(input!);

        Assert.True(result.IsFailure);
        Assert.Equal("KernelR.ValueObjects.PhoneNumber.EMPTY", result.Error.Code);
        Assert.Equal("Phone number cannot be empty.", result.Error.Message);
    }

    [Theory]
    [InlineData("123")]
    [InlineData("0912123456")]
    [InlineData("091212345678")]
    [InlineData("08121234567")]
    [InlineData("0912123456a")]
    [InlineData("+98912123456")]
    public void Create_InvalidFormat_ReturnsInvalidFormatError(string invalidInput)
    {
        Result<PhoneNumber> result = PhoneNumber.Create(invalidInput);

        Assert.True(result.IsFailure);
        Assert.Equal("KernelR.ValueObjects.PhoneNumber.INVALID_FORMAT", result.Error.Code);
        Assert.Equal("Phone number format is invalid.", result.Error.Message);
    }

    [Fact]
    public void Parse_ValidPhoneNumber_ReturnsPhoneNumber()
    {
        string input = "+98912-123-4567";

        PhoneNumber phoneNumber = PhoneNumber.Parse(input, null);

        Assert.Equal("09121234567", phoneNumber.Value);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("123")]
    [InlineData("invalid-phone")]
    public void Parse_InvalidPhoneNumber_ThrowsFormatException(string? input)
    {
        Assert.Throws<FormatException>(() => PhoneNumber.Parse(input!, null));
    }

    [Theory]
    [InlineData("+989121234567", true)]
    [InlineData("0912-123-4567", true)]
    [InlineData(null, false)]
    [InlineData("", false)]
    [InlineData("123", false)]
    public void TryParse_VariousInputs_ReturnsExpectedResult(string? input, bool expectedSuccess)
    {
        bool success = PhoneNumber.TryParse(input, null, out PhoneNumber result);

        Assert.Equal(expectedSuccess, success);
        if (expectedSuccess)
        {
            Assert.NotEqual(default, result);
            Assert.Equal("09121234567", result.Value);
        }
        else
        {
            Assert.Equal(default, result);
        }
    }

    [Fact]
    public void ToString_ReturnsNormalizedValue()
    {
        PhoneNumber phoneNumber = PhoneNumber.Create("+98912-123-4567").Value;

        string result = phoneNumber.ToString();

        Assert.Equal("09121234567", result);
    }

    [Fact]
    public void ExplicitOperator_ReturnsNormalizedValue()
    {
        PhoneNumber phoneNumber = PhoneNumber.Create("0912 123 4567").Value;

        string result = (string)phoneNumber;

        Assert.Equal("09121234567", result);
    }

    [Fact]
    public void Equality_DifferentInputsSameNormalizedValue_ReturnsTrue()
    {
        PhoneNumber phone1 = PhoneNumber.Create("+989121234567").Value;
        PhoneNumber phone2 = PhoneNumber.Create("0912-123-4567").Value;

        Assert.Equal(phone1, phone2);
        Assert.True(phone1 == phone2);
        Assert.False(phone1 != phone2);
    }

    [Fact]
    public void Equality_DifferentPhoneNumbers_ReturnsFalse()
    {
        PhoneNumber phone1 = PhoneNumber.Create("09121234567").Value;
        PhoneNumber phone2 = PhoneNumber.Create("09121234568").Value;

        Assert.NotEqual(phone1, phone2);
        Assert.False(phone1 == phone2);
        Assert.True(phone1 != phone2);
    }
}
