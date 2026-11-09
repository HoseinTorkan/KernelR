using KernelR.Domain.Enums;
using KernelR.Domain.ValueObjects.Apparel;
using KernelR.Patterns.Results;

namespace KernelR.Domain.Tests.ValueObjects.Apparel;

public class ClothSizeTests
{
    [Theory]
    [InlineData(ClothSizeType.ExtraSmall)]
    [InlineData(ClothSizeType.Small)]
    [InlineData(ClothSizeType.Medium)]
    [InlineData(ClothSizeType.Large)]
    [InlineData(ClothSizeType.ExtraLarge)]
    [InlineData(ClothSizeType.DoubleExtraLarge)]
    [InlineData(ClothSizeType.TripleExtraLarge)]
    public void CreateFromEnum_ValidValue_ReturnsSuccess(ClothSizeType validType)
    {
        Result<ClothSize> result = ClothSize.Create(validType);

        Assert.True(result.IsSuccess);
        Assert.Equal(validType, result.Value.Value);
    }

    [Fact]
    public void CreateFromEnum_InvalidValue_ReturnsFailure()
    {
        ClothSizeType invalidType = (ClothSizeType)99;

        Result<ClothSize> result = ClothSize.Create(invalidType);

        Assert.True(result.IsFailure);
        Assert.Equal("KernelR.ValueObjects.ClothSize.INVALID", result.Error.Code);
    }

    [Theory]
    [InlineData("XS", ClothSizeType.ExtraSmall)]
    [InlineData("ExtraSmall", ClothSizeType.ExtraSmall)]
    [InlineData("S", ClothSizeType.Small)]
    [InlineData("small", ClothSizeType.Small)]
    [InlineData("M", ClothSizeType.Medium)]
    [InlineData(" m ", ClothSizeType.Medium)]
    [InlineData("L", ClothSizeType.Large)]
    [InlineData("Large", ClothSizeType.Large)]
    [InlineData("XL", ClothSizeType.ExtraLarge)]
    [InlineData("ExtraLarge", ClothSizeType.ExtraLarge)]
    [InlineData("XXL", ClothSizeType.DoubleExtraLarge)]
    [InlineData("2XL", ClothSizeType.DoubleExtraLarge)]
    [InlineData("DoubleExtraLarge", ClothSizeType.DoubleExtraLarge)]
    [InlineData("XXXL", ClothSizeType.TripleExtraLarge)]
    [InlineData("3XL", ClothSizeType.TripleExtraLarge)]
    [InlineData("tripleextralarge", ClothSizeType.TripleExtraLarge)]
    public void CreateFromString_ValidString_ReturnsSuccess(string input, ClothSizeType expectedType)
    {
        Result<ClothSize> result = ClothSize.Create(input);

        Assert.True(result.IsSuccess);
        Assert.Equal(expectedType, result.Value.Value);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("InvalidSize")]
    [InlineData("4XL")]
    [InlineData("MediumLarge")]
    public void CreateFromString_InvalidString_ReturnsFailure(string? input)
    {
        Result<ClothSize> result = ClothSize.Create(input!);

        Assert.True(result.IsFailure);
        Assert.Contains("EMPTY", result.Error.Code);
    }

    [Theory]
    [InlineData("4XL")]
    [InlineData("Invalid")]
    [InlineData("123")]
    public void CreateFromString_UnrecognizedString_ReturnsInvalidError(string input)
    {
        Result<ClothSize> result = ClothSize.Create(input);

        Assert.True(result.IsFailure);
        Assert.Equal("KernelR.ValueObjects.ClothSize.INVALID", result.Error.Code);
    }

    [Fact]
    public void Parse_ValidString_ReturnsClothSize()
    {
        ClothSize size = ClothSize.Parse("XL", null);

        Assert.Equal(ClothSizeType.ExtraLarge, size.Value);
    }

    [Fact]
    public void Parse_InvalidString_ThrowsFormatException()
    {
        Assert.Throws<FormatException>(() => ClothSize.Parse("Invalid", null));
    }

    [Fact]
    public void TryParse_ValidString_ReturnsTrueAndOutValue()
    {
        bool success = ClothSize.TryParse("M", null, out ClothSize result);

        Assert.True(success);
        Assert.Equal(ClothSizeType.Medium, result.Value);
    }

    [Fact]
    public void TryParse_InvalidString_ReturnsFalseAndDefaultValue()
    {
        bool success = ClothSize.TryParse("Invalid", null, out ClothSize result);

        Assert.False(success);
        Assert.Equal(default, result);
    }

    [Theory]
    [InlineData(ClothSizeType.ExtraSmall, "XS")]
    [InlineData(ClothSizeType.Small, "S")]
    [InlineData(ClothSizeType.Medium, "M")]
    [InlineData(ClothSizeType.Large, "L")]
    [InlineData(ClothSizeType.ExtraLarge, "XL")]
    [InlineData(ClothSizeType.DoubleExtraLarge, "XXL")]
    [InlineData(ClothSizeType.TripleExtraLarge, "XXXL")]
    public void ToString_ReturnsStandardAbbreviation(ClothSizeType type, string expectedString)
    {
        ClothSize size = ClothSize.Create(type).Value;

        Assert.Equal(expectedString, size.ToString());
    }

    [Fact]
    public void ExplicitOperator_ReturnsString()
    {
        ClothSize size = ClothSize.Create(ClothSizeType.Large).Value;

        string result = (string)size;

        Assert.Equal("L", result);
    }

    [Fact]
    public void ComparisonOperators_CompareCorrectly()
    {
        ClothSize small = ClothSize.Create(ClothSizeType.Small).Value;
        ClothSize medium = ClothSize.Create(ClothSizeType.Medium).Value;
        ClothSize large = ClothSize.Create(ClothSizeType.Large).Value;

        Assert.True(small < medium);
        Assert.True(large > medium);
        Assert.True(small <= medium);
        Assert.True(medium >= medium);
        Assert.False(small > medium);
        Assert.False(large < medium);
    }

    [Fact]
    public void CompareTo_ReturnsCorrectInteger()
    {
        ClothSize small = ClothSize.Create(ClothSizeType.Small).Value;
        ClothSize medium = ClothSize.Create(ClothSizeType.Medium).Value;
        ClothSize anotherMedium = ClothSize.Create(ClothSizeType.Medium).Value;

        Assert.True(small.CompareTo(medium) < 0);
        Assert.True(medium.CompareTo(small) > 0);
        Assert.Equal(0, medium.CompareTo(anotherMedium));
    }

    [Fact]
    public void IComparableCompareTo_WithNull_ReturnsOne()
    {
        ClothSize size = ClothSize.Create(ClothSizeType.Medium).Value;
        IComparable comparable = size;

        int result = comparable.CompareTo(null);

        Assert.Equal(1, result);
    }

    [Fact]
    public void IComparableCompareTo_WithInvalidType_ThrowsArgumentException()
    {
        ClothSize size = ClothSize.Create(ClothSizeType.Medium).Value;
        IComparable comparable = size;

        Assert.Throws<ArgumentException>(() => comparable.CompareTo("NotAClothSize"));
    }

    [Fact]
    public void Equality_SameSize_ReturnsTrue()
    {
        ClothSize size1 = ClothSize.Create(ClothSizeType.Medium).Value;
        ClothSize size2 = ClothSize.Create("M").Value;

        Assert.Equal(size1, size2);
        Assert.True(size1 == size2);
        Assert.False(size1 != size2);
    }

    [Fact]
    public void Equality_DifferentSize_ReturnsFalse()
    {
        ClothSize size1 = ClothSize.Create(ClothSizeType.Medium).Value;
        ClothSize size2 = ClothSize.Create(ClothSizeType.Large).Value;

        Assert.NotEqual(size1, size2);
        Assert.False(size1 == size2);
        Assert.True(size1 != size2);
    }
}
