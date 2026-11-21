using KernelR.Domain.ValueObjects;
using KernelR.Patterns.Results;

namespace KernelR.Domain.Tests.ValueObjects.Contact;

public class NationalCodeTests
{
    [Theory]
    [InlineData("0499370899")]
    [InlineData("0084575948")]
    [InlineData("0013542419")]
    [InlineData("0963696991")]
    public void Create_ValidCode_ReturnsSuccess(string validCode)
    {
        Result<NationalCode> result = NationalCode.Create(validCode);

        Assert.True(result.IsSuccess);
        Assert.Equal(validCode, result.Value.Value);
    }

    [Theory]
    [InlineData("049-937-0899", "0499370899")]
    [InlineData("049 937 0899", "0499370899")]
    [InlineData(" 0499370899 ", "0499370899")]
    [InlineData("0-0-8-4-5-7-5-9-4-8", "0084575948")]
    public void Create_WithSeparators_RemovesSeparatorsAndReturnsSuccess(string input, string expectedNormalized)
    {
        Result<NationalCode> result = NationalCode.Create(input);

        Assert.True(result.IsSuccess);
        Assert.Equal(expectedNormalized, result.Value.Value);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_NullOrWhitespace_ReturnsEmptyError(string? input)
    {
        Result<NationalCode> result = NationalCode.Create(input!);

        Assert.True(result.IsFailure);
        Assert.Equal("KernelR.ValueObjects.NationalCode.EMPTY", result.Error.Code);
        Assert.Equal("National code cannot be empty.", result.Error.Message);
    }

    [Theory]
    [InlineData("123456789")]
    [InlineData("12345678901")]
    [InlineData("049937089a")]
    [InlineData("04993708!9")]
    [InlineData("abc")]
    public void Create_InvalidLengthOrCharacters_ReturnsInvalidFormatError(string invalidCode)
    {
        Result<NationalCode> result = NationalCode.Create(invalidCode);

        Assert.True(result.IsFailure);
        Assert.Equal("KernelR.ValueObjects.NationalCode.INVALID_FORMAT", result.Error.Code);
        Assert.Equal("National code must be exactly 10 digits.", result.Error.Message);
    }

    [Theory]
    [InlineData("0000000000")]
    [InlineData("1111111111")]
    [InlineData("2222222222")]
    [InlineData("9999999999")]
    public void Create_AllIdenticalDigits_ReturnsInvalidChecksumError(string invalidCode)
    {
        Result<NationalCode> result = NationalCode.Create(invalidCode);

        Assert.True(result.IsFailure);
        Assert.Equal("KernelR.ValueObjects.NationalCode.INVALID_CHECKSUM", result.Error.Code);
        Assert.Equal("National code is invalid.", result.Error.Message);
    }

    [Theory]
    [InlineData("0499370898")]
    [InlineData("0084575949")]
    [InlineData("0012345678")]
    public void Create_InvalidChecksum_ReturnsInvalidChecksumError(string invalidCode)
    {
        Result<NationalCode> result = NationalCode.Create(invalidCode);

        Assert.True(result.IsFailure);
        Assert.Equal("KernelR.ValueObjects.NationalCode.INVALID_CHECKSUM", result.Error.Code);
        Assert.Equal("National code is invalid.", result.Error.Message);
    }

    [Fact]
    public void Parse_ValidCode_ReturnsNationalCode()
    {
        string input = "049-937-0899";

        NationalCode code = NationalCode.Parse(input, null);

        Assert.Equal("0499370899", code.Value);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("1111111111")]
    [InlineData("123")]
    [InlineData("invalid-code")]
    public void Parse_InvalidCode_ThrowsFormatException(string? input)
    {
        Assert.Throws<FormatException>(() => NationalCode.Parse(input!, null));
    }

    [Theory]
    [InlineData("0499370899", true)]
    [InlineData("049-937-0899", true)]
    [InlineData(null, false)]
    [InlineData("", false)]
    [InlineData("123", false)]
    [InlineData("1111111111", false)]
    [InlineData("0499370898", false)]
    public void TryParse_VariousInputs_ReturnsExpectedResult(string? input, bool expectedSuccess)
    {
        bool success = NationalCode.TryParse(input, null, out NationalCode result);

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
    public void ToString_ReturnsNormalizedValue()
    {
        NationalCode code = NationalCode.Create("049-937-0899").Value;

        string result = code.ToString();

        Assert.Equal("0499370899", result);
    }

    [Fact]
    public void ExplicitOperator_ReturnsNormalizedValue()
    {
        NationalCode code = NationalCode.Create("008-457-5948").Value;

        string result = (string)code;

        Assert.Equal("0084575948", result);
    }

    [Fact]
    public void Equality_SameNormalizedCode_ReturnsTrue()
    {
        NationalCode code1 = NationalCode.Create("0499370899").Value;
        NationalCode code2 = NationalCode.Create("049-937-0899").Value;

        Assert.Equal(code1, code2);
        Assert.True(code1 == code2);
        Assert.False(code1 != code2);
    }

    [Fact]
    public void Equality_DifferentCodes_ReturnsFalse()
    {
        NationalCode code1 = NationalCode.Create("0499370899").Value;
        NationalCode code2 = NationalCode.Create("0084575948").Value;

        Assert.NotEqual(code1, code2);
        Assert.False(code1 == code2);
        Assert.True(code1 != code2);
    }
}
