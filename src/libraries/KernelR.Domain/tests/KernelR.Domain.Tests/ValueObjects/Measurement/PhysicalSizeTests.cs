using KernelR.Domain.Enums;
using KernelR.Domain.ValueObjects.Measurement;
using KernelR.Patterns.Results;

namespace KernelR.Domain.Tests.ValueObjects.Measurement;

public class PhysicalSizeTests
{
    [Theory]
    [InlineData(0.01, PhysicalSizeUnit.Millimeter, 0.01)]
    [InlineData(10, PhysicalSizeUnit.Centimeter, 10)]
    [InlineData(1.55, PhysicalSizeUnit.Meter, 1.55)]
    [InlineData(9999.99, PhysicalSizeUnit.Inch, 9999.99)]
    public void Create_ValidAmountAndUnit_ReturnsSuccess(decimal value, PhysicalSizeUnit unit, decimal expectedValue)
    {
        Result<PhysicalSize> result = PhysicalSize.Create(value, unit);

        Assert.True(result.IsSuccess);
        Assert.Equal(expectedValue, result.Value.Value);
        Assert.Equal(unit, result.Value.Unit);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100.5)]
    public void Create_ZeroOrNegativeAmount_ReturnsInvalidValueError(decimal value)
    {
        Result<PhysicalSize> result = PhysicalSize.Create(value, PhysicalSizeUnit.Meter);

        Assert.True(result.IsFailure);
        Assert.Equal("KernelR.ValueObjects.PhysicalSize.INVALID_VALUE", result.Error.Code);
        Assert.Equal("Physical size value must be greater than zero.", result.Error.Message);
    }

    [Fact]
    public void Create_ExceedsMaxValue_ReturnsTooLargeError()
    {
        Result<PhysicalSize> result = PhysicalSize.Create(10000.01m, PhysicalSizeUnit.Meter);

        Assert.True(result.IsFailure);
        Assert.Equal("KernelR.ValueObjects.PhysicalSize.TOO_LARGE", result.Error.Code);
    }

    [Fact]
    public void Create_InvalidUnit_ReturnsInvalidUnitError()
    {
        Result<PhysicalSize> result = PhysicalSize.Create(10, (PhysicalSizeUnit)99);

        Assert.True(result.IsFailure);
        Assert.Equal("KernelR.ValueObjects.PhysicalSize.INVALID_UNIT", result.Error.Code);
    }

    [Theory]
    [InlineData("5.5 cm", 5.5, PhysicalSizeUnit.Centimeter)]
    [InlineData("10 mm", 10, PhysicalSizeUnit.Millimeter)]
    [InlineData("1 m", 1, PhysicalSizeUnit.Meter)]
    [InlineData("2.5 in", 2.5, PhysicalSizeUnit.Inch)]
    [InlineData("3 ft", 3, PhysicalSizeUnit.Foot)]
    [InlineData("  1.5   m  ", 1.5, PhysicalSizeUnit.Meter)]
    [InlineData("5 CENTIMETER", 5, PhysicalSizeUnit.Centimeter)]
    public void Create_ValidString_ReturnsSuccess(string input, decimal expectedValue, PhysicalSizeUnit expectedUnit)
    {
        Result<PhysicalSize> result = PhysicalSize.Create(input);

        Assert.True(result.IsSuccess);
        Assert.Equal(expectedValue, result.Value.Value);
        Assert.Equal(expectedUnit, result.Value.Unit);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_NullOrWhitespaceString_ReturnsEmptyError(string? input)
    {
        Result<PhysicalSize> result = PhysicalSize.Create(input!);

        Assert.True(result.IsFailure);
        Assert.Equal("KernelR.ValueObjects.PhysicalSize.EMPTY", result.Error.Code);
    }

    [Theory]
    [InlineData("5.5")]
    [InlineData("5.5 cm extra")]
    [InlineData("5.5cm")]
    public void Create_InvalidStringFormat_ReturnsInvalidFormatError(string input)
    {
        Result<PhysicalSize> result = PhysicalSize.Create(input);

        Assert.True(result.IsFailure);
        Assert.Equal("KernelR.ValueObjects.PhysicalSize.INVALID_FORMAT", result.Error.Code);
    }

    [Fact]
    public void Create_InvalidStringAmount_ReturnsInvalidValueError()
    {
        Result<PhysicalSize> result = PhysicalSize.Create("abc cm");

        Assert.True(result.IsFailure);
        Assert.Equal("KernelR.ValueObjects.PhysicalSize.INVALID_VALUE", result.Error.Code);
    }

    [Fact]
    public void Create_InvalidStringUnit_ReturnsInvalidUnitError()
    {
        Result<PhysicalSize> result = PhysicalSize.Create("5.5 XYZ");

        Assert.True(result.IsFailure);
        Assert.Equal("KernelR.ValueObjects.PhysicalSize.INVALID_UNIT", result.Error.Code);
    }

    [Fact]
    public void Parse_ValidString_ReturnsPhysicalSize()
    {
        PhysicalSize size = PhysicalSize.Parse("5.5 cm", null);

        Assert.Equal(5.5m, size.Value);
        Assert.Equal(PhysicalSizeUnit.Centimeter, size.Unit);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("invalid")]
    public void Parse_InvalidString_ThrowsFormatException(string? input)
    {
        Assert.Throws<FormatException>(() => PhysicalSize.Parse(input!, null));
    }

    [Theory]
    [InlineData("5.5 cm", true)]
    [InlineData("invalid", false)]
    [InlineData(null, false)]
    public void TryParse_VariousInputs_ReturnsExpectedResult(string? input, bool expectedSuccess)
    {
        bool success = PhysicalSize.TryParse(input, null, out PhysicalSize result);

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
    public void Add_SameUnit_ReturnsSum()
    {
        PhysicalSize s1 = PhysicalSize.Create(100, PhysicalSizeUnit.Centimeter).Value;
        PhysicalSize s2 = PhysicalSize.Create(50, PhysicalSizeUnit.Centimeter).Value;

        Result<PhysicalSize> result = s1.Add(s2);

        Assert.True(result.IsSuccess);
        Assert.Equal(150, result.Value.Value);
        Assert.Equal(PhysicalSizeUnit.Centimeter, result.Value.Unit);
    }

    [Fact]
    public void Add_DifferentUnits_ConvertsAndReturnsSum()
    {
        PhysicalSize s1 = PhysicalSize.Create(1, PhysicalSizeUnit.Meter).Value;
        PhysicalSize s2 = PhysicalSize.Create(50, PhysicalSizeUnit.Centimeter).Value;

        Result<PhysicalSize> result = s1.Add(s2);

        Assert.True(result.IsSuccess);
        Assert.Equal(1.5m, result.Value.Value);
        Assert.Equal(PhysicalSizeUnit.Meter, result.Value.Unit);
    }

    [Fact]
    public void Subtract_SameUnit_ReturnsDifference()
    {
        PhysicalSize s1 = PhysicalSize.Create(100, PhysicalSizeUnit.Centimeter).Value;
        PhysicalSize s2 = PhysicalSize.Create(40, PhysicalSizeUnit.Centimeter).Value;

        Result<PhysicalSize> result = s1.Subtract(s2);

        Assert.True(result.IsSuccess);
        Assert.Equal(60, result.Value.Value);
    }

    [Fact]
    public void Subtract_DifferentUnits_ConvertsAndReturnsDifference()
    {
        PhysicalSize s1 = PhysicalSize.Create(1, PhysicalSizeUnit.Meter).Value;
        PhysicalSize s2 = PhysicalSize.Create(20, PhysicalSizeUnit.Centimeter).Value;

        Result<PhysicalSize> result = s1.Subtract(s2);

        Assert.True(result.IsSuccess);
        Assert.Equal(0.8m, result.Value.Value);
        Assert.Equal(PhysicalSizeUnit.Meter, result.Value.Unit);
    }

    [Fact]
    public void Subtract_ResultZeroOrNegative_ReturnsNegativeOrZeroResultError()
    {
        PhysicalSize s1 = PhysicalSize.Create(50, PhysicalSizeUnit.Centimeter).Value;
        PhysicalSize s2 = PhysicalSize.Create(100, PhysicalSizeUnit.Centimeter).Value;

        Result<PhysicalSize> result = s1.Subtract(s2);

        Assert.True(result.IsFailure);
        Assert.Equal("KernelR.ValueObjects.PhysicalSize.NEGATIVE_OR_ZERO_RESULT", result.Error.Code);
    }

    [Fact]
    public void ComparisonMethods_ReturnCorrectBool()
    {
        PhysicalSize s1 = PhysicalSize.Create(1, PhysicalSizeUnit.Meter).Value;
        PhysicalSize s2 = PhysicalSize.Create(50, PhysicalSizeUnit.Centimeter).Value;
        PhysicalSize s3 = PhysicalSize.Create(100, PhysicalSizeUnit.Centimeter).Value;

        Assert.True(s1.GreaterThan(s2).Value);
        Assert.False(s1.LessThan(s2).Value);
        Assert.True(s1.GreaterThanOrEqual(s3).Value);
        Assert.True(s1.LessThanOrEqual(s3).Value);
    }

    [Fact]
    public void Operator_Plus_ReturnsSum()
    {
        PhysicalSize s1 = PhysicalSize.Create(1, PhysicalSizeUnit.Meter).Value;
        PhysicalSize s2 = PhysicalSize.Create(50, PhysicalSizeUnit.Centimeter).Value;

        PhysicalSize result = s1 + s2;

        Assert.Equal(1.5m, result.Value);
        Assert.Equal(PhysicalSizeUnit.Meter, result.Unit);
    }

    [Fact]
    public void Operator_Minus_ReturnsDifference()
    {
        PhysicalSize s1 = PhysicalSize.Create(1, PhysicalSizeUnit.Meter).Value;
        PhysicalSize s2 = PhysicalSize.Create(50, PhysicalSizeUnit.Centimeter).Value;

        PhysicalSize result = s1 - s2;

        Assert.Equal(0.5m, result.Value);
    }

    [Fact]
    public void Operator_Minus_ZeroOrNegativeResult_ThrowsInvalidOperationException()
    {
        PhysicalSize s1 = PhysicalSize.Create(50, PhysicalSizeUnit.Centimeter).Value;
        PhysicalSize s2 = PhysicalSize.Create(100, PhysicalSizeUnit.Centimeter).Value;

        Assert.Throws<InvalidOperationException>(() => s1 - s2);
    }

    [Fact]
    public void ComparisonOperators_ReturnCorrectBool()
    {
        PhysicalSize s1 = PhysicalSize.Create(1, PhysicalSizeUnit.Meter).Value;
        PhysicalSize s2 = PhysicalSize.Create(50, PhysicalSizeUnit.Centimeter).Value;

        Assert.True(s1 > s2);
        Assert.False(s1 < s2);
        Assert.True(s1 >= s2);
        Assert.False(s1 <= s2);
    }

    [Fact]
    public void CompareTo_SameUnit_ReturnsIntegerComparison()
    {
        PhysicalSize s1 = PhysicalSize.Create(2, PhysicalSizeUnit.Meter).Value;
        PhysicalSize s2 = PhysicalSize.Create(1, PhysicalSizeUnit.Meter).Value;
        PhysicalSize s3 = PhysicalSize.Create(2, PhysicalSizeUnit.Meter).Value;

        Assert.True(s1.CompareTo(s2) > 0);
        Assert.True(s2.CompareTo(s1) < 0);
        Assert.Equal(0, s1.CompareTo(s3));
    }

    [Fact]
    public void CompareTo_DifferentUnits_ReturnsIntegerComparison()
    {
        PhysicalSize s1 = PhysicalSize.Create(1, PhysicalSizeUnit.Meter).Value;
        PhysicalSize s2 = PhysicalSize.Create(100, PhysicalSizeUnit.Centimeter).Value;

        Assert.Equal(0, s1.CompareTo(s2));
    }

    [Fact]
    public void IComparableCompareTo_WithNull_ReturnsOne()
    {
        PhysicalSize size = PhysicalSize.Create(1, PhysicalSizeUnit.Meter).Value;
        IComparable comparable = size;

        Assert.Equal(1, comparable.CompareTo(null));
    }

    [Fact]
    public void IComparableCompareTo_WithInvalidType_ThrowsArgumentException()
    {
        PhysicalSize size = PhysicalSize.Create(1, PhysicalSizeUnit.Meter).Value;
        IComparable comparable = size;

        Assert.Throws<ArgumentException>(() => comparable.CompareTo("NotPhysicalSize"));
    }

    [Fact]
    public void Conversions_ReturnCorrectValues()
    {
        PhysicalSize oneMeter = PhysicalSize.Create(1, PhysicalSizeUnit.Meter).Value;

        Assert.Equal(100, oneMeter.ToCentimeter().Value);
        Assert.Equal(1000, oneMeter.ToMillimeter().Value);
        Assert.Equal(39.37m, oneMeter.ToInch().Value);
        Assert.Equal(3.28m, oneMeter.ToFoot().Value);
    }

    [Fact]
    public void Conversion_SameUnit_ReturnsSelf()
    {
        PhysicalSize size = PhysicalSize.Create(5, PhysicalSizeUnit.Centimeter).Value;

        PhysicalSize result = size.ToCentimeter();

        Assert.Equal(size, result);
    }

    [Fact]
    public void ToString_ReturnsFormattedString()
    {
        PhysicalSize size = PhysicalSize.Create(5.5m, PhysicalSizeUnit.Centimeter).Value;

        string result = size.ToString();

        Assert.Equal("5.5 Centimeter", result);
    }

    [Fact]
    public void ExplicitOperator_ReturnsString()
    {
        PhysicalSize size = PhysicalSize.Create(5.5m, PhysicalSizeUnit.Meter).Value;

        string result = (string)size;

        Assert.Equal("5.5 Meter", result);
    }

    [Fact]
    public void Equality_SameValueAndUnit_ReturnsTrue()
    {
        PhysicalSize s1 = PhysicalSize.Create(1, PhysicalSizeUnit.Meter).Value;
        PhysicalSize s2 = PhysicalSize.Create(1, PhysicalSizeUnit.Meter).Value;

        Assert.Equal(s1, s2);
        Assert.True(s1 == s2);
        Assert.False(s1 != s2);
    }

    [Fact]
    public void Equality_DifferentValuesOrUnits_ReturnsFalse()
    {
        PhysicalSize s1 = PhysicalSize.Create(1, PhysicalSizeUnit.Meter).Value;
        PhysicalSize s2 = PhysicalSize.Create(100, PhysicalSizeUnit.Centimeter).Value;
        PhysicalSize s3 = PhysicalSize.Create(2, PhysicalSizeUnit.Meter).Value;

        Assert.NotEqual(s1, s2);
        Assert.NotEqual(s1, s3);
    }
}
