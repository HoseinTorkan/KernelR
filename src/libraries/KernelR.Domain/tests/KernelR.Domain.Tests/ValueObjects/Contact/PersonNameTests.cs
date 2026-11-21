using KernelR.Domain.ValueObjects;
using KernelR.Patterns.Results;

namespace KernelR.Domain.Tests.ValueObjects.Contact;

public class PersonNameTests
{
    [Theory]
    [InlineData("John Doe")]
    [InlineData("علی")]
    [InlineData("فاطمه\u200Cزهرا")]
    [InlineData("O'Connor")]
    [InlineData("Jean-Luc")]
    [InlineData("Mary-Jane Watson")]
    [InlineData("علی رضا")]
    [InlineData("Ali Reza")]
    [InlineData("Ab")]
    public void Create_ValidName_ReturnsSuccess(string validName)
    {
        Result<PersonName> result = PersonName.Create(validName);

        Assert.True(result.IsSuccess);
        Assert.Equal(validName, result.Value.Value);
    }

    [Theory]
    [InlineData("  John Doe  ", "John Doe")]
    [InlineData(" علی ", "علی")]
    [InlineData("\tJohn\t", "John")]
    public void Create_ValidName_TrimsWhitespace(string input, string expectedValue)
    {
        Result<PersonName> result = PersonName.Create(input);

        Assert.True(result.IsSuccess);
        Assert.Equal(expectedValue, result.Value.Value);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t")]
    public void Create_NullOrWhitespace_ReturnsEmptyError(string? input)
    {
        Result<PersonName> result = PersonName.Create(input!);

        Assert.True(result.IsFailure);
        Assert.Equal("KernelR.ValueObjects.PersonName.EMPTY", result.Error.Code);
        Assert.Equal("Person name cannot be empty.", result.Error.Message);
    }

    [Theory]
    [InlineData("A")]
    [InlineData(" A ")]
    public void Create_TooShort_ReturnsTooShortError(string input)
    {
        Result<PersonName> result = PersonName.Create(input);

        Assert.True(result.IsFailure);
        Assert.Equal("KernelR.ValueObjects.PersonName.TOO_SHORT", result.Error.Code);
        Assert.Equal("Person name is too short.", result.Error.Message);
    }

    [Fact]
    public void Create_TooLong_ReturnsTooLongError()
    {
        string tooLongName = new('A', 101);

        Result<PersonName> result = PersonName.Create(tooLongName);

        Assert.True(result.IsFailure);
        Assert.Equal("KernelR.ValueObjects.PersonName.TOO_LONG", result.Error.Code);
        Assert.Equal("Person name is too long.", result.Error.Message);
    }

    [Theory]
    [InlineData("User123")]
    [InlineData("John@Doe")]
    [InlineData("Name!")]
    [InlineData("12345")]
    [InlineData("!@#$%")]
    [InlineData("John  Doe")]
    [InlineData("John\tDoe")]
    [InlineData("John_Doe")]
    public void Create_InvalidCharacters_ReturnsInvalidCharactersError(string invalidName)
    {
        Result<PersonName> result = PersonName.Create(invalidName);

        Assert.True(result.IsFailure);
        Assert.Equal("KernelR.ValueObjects.PersonName.INVALID_CHARACTERS", result.Error.Code);
        Assert.Equal("Person name contains invalid characters.", result.Error.Message);
    }

    [Fact]
    public void Parse_ValidName_ReturnsPersonName()
    {
        string input = "  John Doe  ";

        PersonName name = PersonName.Parse(input, null);

        Assert.Equal("John Doe", name.Value);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("A")]
    [InlineData("User123")]
    [InlineData("John  Doe")]
    public void Parse_InvalidName_ThrowsFormatException(string? input)
    {
        Assert.Throws<FormatException>(() => PersonName.Parse(input!, null));
    }

    [Theory]
    [InlineData("John Doe", true)]
    [InlineData(" فاطمه\u200Cزهرا ", true)]
    [InlineData(null, false)]
    [InlineData("", false)]
    [InlineData("A", false)]
    [InlineData("User123", false)]
    public void TryParse_VariousInputs_ReturnsExpectedResult(string? input, bool expectedSuccess)
    {
        bool success = PersonName.TryParse(input, null, out PersonName result);

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
    public void ToString_ReturnsValue()
    {
        PersonName name = PersonName.Create("Ali").Value;

        string result = name.ToString();

        Assert.Equal("Ali", result);
    }

    [Fact]
    public void ExplicitOperator_ReturnsValue()
    {
        PersonName name = PersonName.Create("Ali").Value;

        string result = (string)name;

        Assert.Equal("Ali", result);
    }

    [Fact]
    public void Equality_SameName_ReturnsTrue()
    {
        PersonName name1 = PersonName.Create("John Doe").Value;
        PersonName name2 = PersonName.Create("  John Doe  ").Value;

        Assert.Equal(name1, name2);
        Assert.True(name1 == name2);
        Assert.False(name1 != name2);
    }

    [Fact]
    public void Equality_DifferentName_ReturnsFalse()
    {
        PersonName name1 = PersonName.Create("John Doe").Value;
        PersonName name2 = PersonName.Create("Ali Reza").Value;

        Assert.NotEqual(name1, name2);
        Assert.False(name1 == name2);
        Assert.True(name1 != name2);
    }
}
