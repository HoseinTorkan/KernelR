using KernelR.Domain.ValueObjects;
using KernelR.Patterns.Results;

namespace KernelR.Domain.Tests.ValueObjects;

public class PersonNameTests
{
    // ─────────── Create ───────────

    [Theory]
    [InlineData("علی", "علی")]
    [InlineData("فاطمه‌زهرا", "فاطمه‌زهرا")]  // ZWNJ
    [InlineData("John Doe", "John Doe")]
    [InlineData("O'Brien", "O'Brien")]
    [InlineData("Jean-Luc", "Jean-Luc")]
    [InlineData("  علی  ", "علی")]
    public void Create_WithValidName_ReturnsTrimmedSuccess(string input, string expected)
    {
        Result<PersonName> result = PersonName.Create(input);

        Assert.True(result.IsSuccess);
        Assert.Equal(expected, result.Value.Value);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_WithEmptyInput_ReturnsEmpty(string? input)
    {
        Result<PersonName> result = PersonName.Create(input!);

        Assert.True(result.IsFailure);
        Assert.Equal("KernelR.ValueObjects.PersonName.EMPTY", result.Error.Code);
    }

    [Fact]
    public void Create_WithSingleCharacter_ReturnsTooShort()
    {
        // 👇 "A" (یک حرف) → TOO_SHORT (نه EMPTY)
        Result<PersonName> result = PersonName.Create("A");

        Assert.True(result.IsFailure);
        Assert.Equal("KernelR.ValueObjects.PersonName.TOO_SHORT", result.Error.Code);
    }

    [Fact]
    public void Create_WithTooLongInput_ReturnsTooLong()
    {
        string input = new('a', 101);

        Result<PersonName> result = PersonName.Create(input);

        Assert.True(result.IsFailure);
        Assert.Equal("KernelR.ValueObjects.PersonName.TOO_LONG", result.Error.Code);
    }

    [Theory]
    [InlineData("User123")]
    [InlineData("John@Doe")]
    [InlineData("Name!")]
    public void Create_WithInvalidCharacters_ReturnsInvalidCharacters(string input)
    {
        Result<PersonName> result = PersonName.Create(input);

        Assert.True(result.IsFailure);
        Assert.Equal("KernelR.ValueObjects.PersonName.INVALID_CHARACTERS", result.Error.Code);
    }

    // ─────────── IParsable ───────────

    [Fact]
    public void TryParse_WithValidInput_ReturnsTrue()
    {
        bool success = PersonName.TryParse("علی", null, out PersonName result);

        Assert.True(success);
        Assert.Equal("علی", result.Value);
    }

    [Fact]
    public void TryParse_WithInvalidInput_ReturnsFalse()
    {
        bool success = PersonName.TryParse("A", null, out PersonName result);

        Assert.False(success);
        Assert.Equal(default, result);
    }

    [Fact]
    public void Parse_WithValidInput_ReturnsPersonName()
    {
        PersonName result = PersonName.Parse("John Doe", null);

        Assert.Equal("John Doe", result.Value);
    }

    [Fact]
    public void Parse_WithInvalidInput_ThrowsFormatException()
    {
        Assert.Throws<FormatException>(() => PersonName.Parse("A", null));
    }

    // ─────────── ToString & implicit operator ───────────

    [Fact]
    public void ToString_ReturnsValue()
    {
        PersonName name = PersonName.Create("علی").Value;

        Assert.Equal("علی", name.ToString());
    }

    [Fact]
    public void ImplicitOperator_ConvertsToString()
    {
        PersonName name = PersonName.Create("John Doe").Value;

        string result = name;

        Assert.Equal("John Doe", result);
    }
}
