using KernelR.Domain.Enums;
using KernelR.Domain.ValueObjects.Measurement;
using KernelR.Patterns.Results;

namespace KernelR.Domain.Tests.ValueObjects.Measurement;

public class PhysicalWeightTests
{
    [Theory]
    [InlineData(0.01, PhysicalWeightUnit.Milligram, 0.01)]
    [InlineData(10, PhysicalWeightUnit.Gram, 10)]
    [InlineData(1.55, PhysicalWeightUnit.Kilogram, 1.55)]
    [InlineData(999999.99, PhysicalWeightUnit.Pound, 999999.99)]
    [InlineData(1.555, PhysicalWeightUnit.Kilogram, 1.56)]
    [InlineData(1.554, PhysicalWeightUnit.Kilogram, 1.55)]
    public void Create_ValidAmountAndUnit_ReturnsSuccess(decimal value, PhysicalWeightUnit unit, decimal expectedValue)
    {
        Result<PhysicalWeight> result = PhysicalWeight.Create(value, unit);

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
        Result<PhysicalWeight> result = PhysicalWeight.Create(value, PhysicalWeightUnit.Kilogram);

        Assert.True(result.IsFailure);
        Assert.Equal("KernelR.ValueObjects.PhysicalWeight.INVALID_VALUE", result.Error.Code);
        Assert.Equal("Physical weight value must be greater than zero.", result.Error.Message);
    }

    [Fact]
    public void Create_ExceedsMaxValue_ReturnsTooLargeError()
    {
        Result<PhysicalWeight> result = PhysicalWeight.Create(1000000.01m, PhysicalWeightUnit.Kilogram);

        Assert.True(result.IsFailure);
        Assert.Equal("KernelR.ValueObjects.PhysicalWeight.TOO_LARGE", result.Error.Code);
    }

    [Fact]
    public void Create_InvalidUnit_ReturnsInvalidUnitError()
    {
        Result<PhysicalWeight> result = PhysicalWeight.Create(10, (PhysicalWeightUnit)99);

        Assert.True(result.IsFailure);
        Assert.Equal("KernelR.ValueObjects.PhysicalWeight.INVALID_UNIT", result.Error.Code);
    }

    [Theory]
    [InlineData("2.5 kg", 2.5, PhysicalWeightUnit.Kilogram)]
    [InlineData("500 g", 500, PhysicalWeightUnit.Gram)]
    [InlineData("10 mg", 10, PhysicalWeightUnit.Milligram)]
    [InlineData("1.5 t", 1.5, PhysicalWeightUnit.Ton)]
    [InlineData("10 lb", 10, PhysicalWeightUnit.Pound)]
    [InlineData("5 oz", 5, PhysicalWeightUnit.Ounce)]
    [InlineData("  1.5   kg  ", 1.5, PhysicalWeightUnit.Kilogram)]
    [InlineData("5 KILOGRAM", 5, PhysicalWeightUnit.Kilogram)]
    public void Create_ValidString_ReturnsSuccess(string input, decimal expectedValue, PhysicalWeightUnit expectedUnit)
    {
        Result<PhysicalWeight> result = PhysicalWeight.Create(input);

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
        Result<PhysicalWeight> result = PhysicalWeight.Create(input!);

        Assert.True(result.IsFailure);
        Assert.Equal("KernelR.ValueObjects.PhysicalWeight.EMPTY", result.Error.Code);
    }

    [Theory]
    [InlineData("2.5")]
    [InlineData("2.5 kg extra")]
    [InlineData("2.5kg")]
    public void Create_InvalidStringFormat_ReturnsInvalidFormatError(string input)
    {
        Result<PhysicalWeight> result = PhysicalWeight.Create(input);

        Assert.True(result.IsFailure);
        Assert.Equal("KernelR.ValueObjects.PhysicalWeight.INVALID_FORMAT", result.Error.Code);
    }

    [Fact]
    public void Create_InvalidStringAmount_ReturnsInvalidValueError()
    {
        Result<PhysicalWeight> result = PhysicalWeight.Create("abc kg");

        Assert.True(result.IsFailure);
        Assert.Equal("KernelR.ValueObjects.PhysicalWeight.INVALID_VALUE", result.Error.Code);
    }

    [Fact]
    public void Create_InvalidStringUnit_ReturnsInvalidUnitError()
    {
        Result<PhysicalWeight> result = PhysicalWeight.Create("2.5 XYZ");

        Assert.True(result.IsFailure);
        Assert.Equal("KernelR.ValueObjects.PhysicalWeight.INVALID_UNIT", result.Error.Code);
    }

    [Fact]
    public void Parse_ValidString_ReturnsPhysicalWeight()
    {
        PhysicalWeight weight = PhysicalWeight.Parse("2.5 kg", null);

        Assert.Equal(2.5m, weight.Value);
        Assert.Equal(PhysicalWeightUnit.Kilogram, weight.Unit);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("invalid")]
    public void Parse_InvalidString_ThrowsFormatException(string? input)
    {
        Assert.Throws<FormatException>(() => PhysicalWeight.Parse(input!, null));
    }

    [Theory]
    [InlineData("2.5 kg", true)]
    [InlineData("invalid", false)]
    [InlineData(null, false)]
    public void TryParse_VariousInputs_ReturnsExpectedResult(string? input, bool expectedSuccess)
    {
        bool success = PhysicalWeight.TryParse(input, null, out PhysicalWeight result);

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
        PhysicalWeight w1 = PhysicalWeight.Create(100, PhysicalWeightUnit.Kilogram).Value;
        PhysicalWeight w2 = PhysicalWeight.Create(50, PhysicalWeightUnit.Kilogram).Value;

        Result<PhysicalWeight> result = w1.Add(w2);

        Assert.True(result.IsSuccess);
        Assert.Equal(150, result.Value.Value);
        Assert.Equal(PhysicalWeightUnit.Kilogram, result.Value.Unit);
    }

    [Fact]
    public void Add_DifferentUnits_ConvertsAndReturnsSum()
    {
        PhysicalWeight w1 = PhysicalWeight.Create(1, PhysicalWeightUnit.Kilogram).Value;
        PhysicalWeight w2 = PhysicalWeight.Create(500, PhysicalWeightUnit.Gram).Value;

        Result<PhysicalWeight> result = w1.Add(w2);

        Assert.True(result.IsSuccess);
        Assert.Equal(1.5m, result.Value.Value);
        Assert.Equal(PhysicalWeightUnit.Kilogram, result.Value.Unit);
    }

    [Fact]
    public void Subtract_SameUnit_ReturnsDifference()
    {
        PhysicalWeight w1 = PhysicalWeight.Create(100, PhysicalWeightUnit.Kilogram).Value;
        PhysicalWeight w2 = PhysicalWeight.Create(40, PhysicalWeightUnit.Kilogram).Value;

        Result<PhysicalWeight> result = w1.Subtract(w2);

        Assert.True(result.IsSuccess);
        Assert.Equal(60, result.Value.Value);
    }

    [Fact]
    public void Subtract_DifferentUnits_ConvertsAndReturnsDifference()
    {
        PhysicalWeight w1 = PhysicalWeight.Create(1, PhysicalWeightUnit.Kilogram).Value;
        PhysicalWeight w2 = PhysicalWeight.Create(200, PhysicalWeightUnit.Gram).Value;

        Result<PhysicalWeight> result = w1.Subtract(w2);

        Assert.True(result.IsSuccess);
        Assert.Equal(0.8m, result.Value.Value);
        Assert.Equal(PhysicalWeightUnit.Kilogram, result.Value.Unit);
    }

    [Fact]
    public void Subtract_ResultZeroOrNegative_ReturnsNegativeOrZeroResultError()
    {
        PhysicalWeight w1 = PhysicalWeight.Create(50, PhysicalWeightUnit.Kilogram).Value;
        PhysicalWeight w2 = PhysicalWeight.Create(100, PhysicalWeightUnit.Kilogram).Value;

        Result<PhysicalWeight> result = w1.Subtract(w2);

        Assert.True(result.IsFailure);
        Assert.Equal("KernelR.ValueObjects.PhysicalWeight.NEGATIVE_OR_ZERO_RESULT", result.Error.Code);
    }

    [Fact]
    public void ComparisonMethods_ReturnCorrectBool()
    {
        PhysicalWeight w1 = PhysicalWeight.Create(1, PhysicalWeightUnit.Kilogram).Value;
        PhysicalWeight w2 = PhysicalWeight.Create(500, PhysicalWeightUnit.Gram).Value;
        PhysicalWeight w3 = PhysicalWeight.Create(1000, PhysicalWeightUnit.Gram).Value;

        Assert.True(w1.GreaterThan(w2).Value);
        Assert.False(w1.LessThan(w2).Value);
        Assert.True(w1.GreaterThanOrEqual(w3).Value);
        Assert.True(w1.LessThanOrEqual(w3).Value);
    }

    [Fact]
    public void Operator_Plus_ReturnsSum()
    {
        PhysicalWeight w1 = PhysicalWeight.Create(1, PhysicalWeightUnit.Kilogram).Value;
        PhysicalWeight w2 = PhysicalWeight.Create(500, PhysicalWeightUnit.Gram).Value;

        PhysicalWeight result = w1 + w2;

        Assert.Equal(1.5m, result.Value);
        Assert.Equal(PhysicalWeightUnit.Kilogram, result.Unit);
    }

    [Fact]
    public void Operator_Minus_ReturnsDifference()
    {
        PhysicalWeight w1 = PhysicalWeight.Create(1, PhysicalWeightUnit.Kilogram).Value;
        PhysicalWeight w2 = PhysicalWeight.Create(500, PhysicalWeightUnit.Gram).Value;

        PhysicalWeight result = w1 - w2;

        Assert.Equal(0.5m, result.Value);
    }

    [Fact]
    public void Operator_Minus_ZeroOrNegativeResult_ThrowsInvalidOperationException()
    {
        PhysicalWeight w1 = PhysicalWeight.Create(50, PhysicalWeightUnit.Kilogram).Value;
        PhysicalWeight w2 = PhysicalWeight.Create(100, PhysicalWeightUnit.Kilogram).Value;

        Assert.Throws<InvalidOperationException>(() => w1 - w2);
    }

    [Fact]
    public void ComparisonOperators_ReturnCorrectBool()
    {
        PhysicalWeight w1 = PhysicalWeight.Create(1, PhysicalWeightUnit.Kilogram).Value;
        PhysicalWeight w2 = PhysicalWeight.Create(500, PhysicalWeightUnit.Gram).Value;

        Assert.True(w1 > w2);
        Assert.False(w1 < w2);
        Assert.True(w1 >= w2);
        Assert.False(w1 <= w2);
    }

    [Fact]
    public void CompareTo_SameUnit_ReturnsIntegerComparison()
    {
        PhysicalWeight w1 = PhysicalWeight.Create(2, PhysicalWeightUnit.Kilogram).Value;
        PhysicalWeight w2 = PhysicalWeight.Create(1, PhysicalWeightUnit.Kilogram).Value;
        PhysicalWeight w3 = PhysicalWeight.Create(2, PhysicalWeightUnit.Kilogram).Value;

        Assert.True(w1.CompareTo(w2) > 0);
        Assert.True(w2.CompareTo(w1) < 0);
        Assert.Equal(0, w1.CompareTo(w3));
    }

    [Fact]
    public void CompareTo_DifferentUnits_ReturnsIntegerComparison()
    {
        PhysicalWeight w1 = PhysicalWeight.Create(1, PhysicalWeightUnit.Kilogram).Value;
        PhysicalWeight w2 = PhysicalWeight.Create(1000, PhysicalWeightUnit.Gram).Value;

        Assert.Equal(0, w1.CompareTo(w2));
    }

    [Fact]
    public void IComparableCompareTo_WithNull_ReturnsOne()
    {
        PhysicalWeight weight = PhysicalWeight.Create(1, PhysicalWeightUnit.Kilogram).Value;
        IComparable comparable = weight;

        Assert.Equal(1, comparable.CompareTo(null));
    }

    [Fact]
    public void IComparableCompareTo_WithInvalidType_ThrowsArgumentException()
    {
        PhysicalWeight weight = PhysicalWeight.Create(1, PhysicalWeightUnit.Kilogram).Value;
        IComparable comparable = weight;

        Assert.Throws<ArgumentException>(() => comparable.CompareTo("NotPhysicalWeight"));
    }

    [Fact]
    public void Conversions_ReturnCorrectValues()
    {
        PhysicalWeight oneKilogram = PhysicalWeight.Create(1, PhysicalWeightUnit.Kilogram).Value;

        Assert.Equal(1000, oneKilogram.ToGram().Value);
        Assert.Equal(1000000, oneKilogram.ToMilligram().Value);
        Assert.Equal(0.001m, oneKilogram.ToTon().Value);
        Assert.Equal(2.2m, oneKilogram.ToPound().Value);
        Assert.Equal(35.27m, oneKilogram.ToOunce().Value);
    }

    [Fact]
    public void Conversion_SameUnit_ReturnsSelf()
    {
        PhysicalWeight weight = PhysicalWeight.Create(5, PhysicalWeightUnit.Kilogram).Value;

        PhysicalWeight result = weight.ToKilogram();

        Assert.Equal(weight, result);
    }

    [Fact]
    public void ToString_ReturnsFormattedString()
    {
        PhysicalWeight weight = PhysicalWeight.Create(2.5m, PhysicalWeightUnit.Kilogram).Value;

        string result = weight.ToString();

        Assert.Equal("2.5 Kilogram", result);
    }

    [Fact]
    public void ExplicitOperator_ReturnsString()
    {
        PhysicalWeight weight = PhysicalWeight.Create(500, PhysicalWeightUnit.Gram).Value;

        string result = (string)weight;

        Assert.Equal("500 Gram", result);
    }

    [Fact]
    public void Equality_SameValueAndUnit_ReturnsTrue()
    {
        PhysicalWeight w1 = PhysicalWeight.Create(1, PhysicalWeightUnit.Kilogram).Value;
        PhysicalWeight w2 = PhysicalWeight.Create(1, PhysicalWeightUnit.Kilogram).Value;

        Assert.Equal(w1, w2);
        Assert.True(w1 == w2);
        Assert.False(w1 != w2);
    }

    [Fact]
    public void Equality_DifferentValuesOrUnits_ReturnsFalse()
    {
        PhysicalWeight w1 = PhysicalWeight.Create(1, PhysicalWeightUnit.Kilogram).Value;
        PhysicalWeight w2 = PhysicalWeight.Create(1000, PhysicalWeightUnit.Gram).Value;
        PhysicalWeight w3 = PhysicalWeight.Create(2, PhysicalWeightUnit.Kilogram).Value;

        Assert.NotEqual(w1, w2);
        Assert.NotEqual(w1, w3);
    }
}
