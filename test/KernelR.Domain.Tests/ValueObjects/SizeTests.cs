using KernelR.Domain.Enums;
using KernelR.Domain.ValueObjects;
using KernelR.Patterns.Results;

namespace KernelR.Domain.Tests.ValueObjects;

public class SizeTests
{
    // ─────────── Create(decimal, SizeUnit) ───────────

    [Theory]
    [InlineData(5, SizeUnit.Centimeter, 5, SizeUnit.Centimeter)]
    [InlineData(2.5, SizeUnit.Meter, 2.5, SizeUnit.Meter)]
    [InlineData(10, SizeUnit.Inch, 10, SizeUnit.Inch)]
    public void Create_WithValidInput_ReturnsSuccess(decimal value, SizeUnit unit, decimal expectedValue, SizeUnit expectedUnit)
    {
        Result<Size> result = Size.Create(value, unit);

        Assert.True(result.IsSuccess);
        Assert.Equal(expectedValue, result.Value.Value);
        Assert.Equal(expectedUnit, result.Value.Unit);
    }

    [Fact]
    public void Create_RoundsToTwoDecimals()
    {
        Result<Size> result = Size.Create(5.555m, SizeUnit.Centimeter);

        Assert.True(result.IsSuccess);
        Assert.Equal(5.56m, result.Value.Value);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_WithNonPositiveValue_ReturnsInvalidValue(decimal value)
    {
        Result<Size> result = Size.Create(value, SizeUnit.Centimeter);

        Assert.True(result.IsFailure);
        Assert.Equal("KernelR.ValueObjects.Size.INVALID_VALUE", result.Error.Code);
    }

    [Fact]
    public void Create_WithTooLargeValue_ReturnsTooLarge()
    {
        Result<Size> result = Size.Create(10_001, SizeUnit.Centimeter);

        Assert.True(result.IsFailure);
        Assert.Equal("KernelR.ValueObjects.Size.TOO_LARGE", result.Error.Code);
    }

    [Fact]
    public void Create_WithUndefinedUnit_ReturnsInvalidUnit()
    {
        Result<Size> result = Size.Create(5, (SizeUnit)99);

        Assert.True(result.IsFailure);
        Assert.Equal("KernelR.ValueObjects.Size.INVALID_UNIT", result.Error.Code);
    }

    // ─────────── Create(string) ───────────

    [Theory]
    [InlineData("5 cm", 5, SizeUnit.Centimeter)]
    [InlineData("2.5 Meter", 2.5, SizeUnit.Meter)]
    [InlineData("10 in", 10, SizeUnit.Inch)]
    [InlineData("1 ft", 1, SizeUnit.Foot)]
    [InlineData("100 mm", 100, SizeUnit.Millimeter)]
    public void Create_WithValidString_ReturnsSuccess(string input, decimal expectedValue, SizeUnit expectedUnit)
    {
        Result<Size> result = Size.Create(input);

        Assert.True(result.IsSuccess);
        Assert.Equal(expectedValue, result.Value.Value);
        Assert.Equal(expectedUnit, result.Value.Unit);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_WithEmptyString_ReturnsEmpty(string? input)
    {
        Result<Size> result = Size.Create(input!);

        Assert.True(result.IsFailure);
        Assert.Equal("KernelR.ValueObjects.Size.EMPTY", result.Error.Code);
    }

    [Theory]
    [InlineData("5")]
    [InlineData("cm")]
    [InlineData("5 cm extra")]
    public void Create_WithInvalidFormat_ReturnsInvalidFormat(string input)
    {
        Result<Size> result = Size.Create(input);

        Assert.True(result.IsFailure);
        Assert.Equal("KernelR.ValueObjects.Size.INVALID_FORMAT", result.Error.Code);
    }

    [Fact]
    public void Create_WithInvalidNumber_ReturnsInvalidValue()
    {
        Result<Size> result = Size.Create("abc cm");

        Assert.True(result.IsFailure);
        Assert.Equal("KernelR.ValueObjects.Size.INVALID_VALUE", result.Error.Code);
    }

    [Fact]
    public void Create_WithInvalidUnit_ReturnsInvalidUnit()
    {
        Result<Size> result = Size.Create("5 xyz");

        Assert.True(result.IsFailure);
        Assert.Equal("KernelR.ValueObjects.Size.INVALID_UNIT", result.Error.Code);
    }

    // ─────────── IParsable ───────────

    [Fact]
    public void TryParse_WithValidInput_ReturnsTrue()
    {
        bool success = Size.TryParse("5 cm", null, out Size result);

        Assert.True(success);
        Assert.Equal(5, result.Value);
    }

    [Fact]
    public void TryParse_WithInvalidInput_ReturnsFalse()
    {
        bool success = Size.TryParse("invalid", null, out Size result);

        Assert.False(success);
        Assert.Equal(default, result);
    }

    [Fact]
    public void Parse_WithValidInput_ReturnsSize()
    {
        Size result = Size.Parse("5 cm", null);

        Assert.Equal(5, result.Value);
    }

    [Fact]
    public void Parse_WithInvalidInput_ThrowsFormatException()
    {
        Assert.Throws<FormatException>(() => Size.Parse("invalid", null));
    }

    // ─────────── Conversions ───────────

    [Fact]
    public void ToCentimeter_FromMeter_ReturnsCorrectValue()
    {
        Size meter = Size.Create(1, SizeUnit.Meter).Value;

        Size cm = meter.ToCentimeter();

        Assert.Equal(100, cm.Value);
        Assert.Equal(SizeUnit.Centimeter, cm.Unit);
    }

    [Fact]
    public void ToInch_FromCentimeter_ReturnsCorrectValue()
    {
        Size cm = Size.Create(2.54m, SizeUnit.Centimeter).Value;

        Size inch = cm.ToInch();

        Assert.Equal(1, inch.Value);
        Assert.Equal(SizeUnit.Inch, inch.Unit);
    }

    [Fact]
    public void Convert_ToSameUnit_ReturnsSameValue()
    {
        Size cm = Size.Create(5, SizeUnit.Centimeter).Value;

        Size same = cm.ToCentimeter();

        Assert.Equal(cm, same);
    }

    // ─────────── ToString & implicit operator ───────────

    [Fact]
    public void ToString_ReturnsValueAndUnit()
    {
        Size size = Size.Create(5.5m, SizeUnit.Centimeter).Value;

        Assert.Equal("5.5 Centimeter", size.ToString());
    }

    [Fact]
    public void ImplicitOperator_ConvertsToString()
    {
        Size size = Size.Create(5, SizeUnit.Centimeter).Value;

        string result = size;

        Assert.Equal("5 Centimeter", result);
    }
}
