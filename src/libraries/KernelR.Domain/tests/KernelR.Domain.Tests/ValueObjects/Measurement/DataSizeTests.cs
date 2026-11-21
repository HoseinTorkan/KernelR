using KernelR.Domain.Enums;
using KernelR.Domain.ValueObjects.Measurement;
using KernelR.Patterns.Results;

namespace KernelR.Domain.Tests.ValueObjects.Measurement;

public class DataSizeTests
{
    [Theory]
    [InlineData(0, DataSizeUnit.Byte, 0)]
    [InlineData(1024, DataSizeUnit.Kilobyte, 1024)]
    [InlineData(1.555, DataSizeUnit.Megabyte, 1.56)]
    [InlineData(999999.99, DataSizeUnit.Terabyte, 1000000)]
    public void Create_ValidAmountAndUnit_ReturnsSuccess(decimal amount, DataSizeUnit unit, decimal expectedAmount)
    {
        Result<DataSize> result = DataSize.Create(amount, unit);

        Assert.True(result.IsSuccess);
        Assert.Equal(expectedAmount, result.Value.Value);
        Assert.Equal(unit, result.Value.Unit);
    }

    [Fact]
    public void Create_NegativeAmount_ReturnsInvalidAmountError()
    {
        Result<DataSize> result = DataSize.Create(-1, DataSizeUnit.Byte);

        Assert.True(result.IsFailure);
        Assert.Equal("KernelR.ValueObjects.DataSize.INVALID_AMOUNT", result.Error.Code);
    }

    [Fact]
    public void Create_ExceedsMaxValue_ReturnsTooLargeError()
    {
        Result<DataSize> result = DataSize.Create(1000001, DataSizeUnit.Byte);

        Assert.True(result.IsFailure);
        Assert.Equal("KernelR.ValueObjects.DataSize.TOO_LARGE", result.Error.Code);
    }

    [Fact]
    public void Create_InvalidUnit_ReturnsInvalidUnitError()
    {
        Result<DataSize> result = DataSize.Create(10, (DataSizeUnit)99);

        Assert.True(result.IsFailure);
        Assert.Equal("KernelR.ValueObjects.DataSize.INVALID_UNIT", result.Error.Code);
    }

    [Theory]
    [InlineData("1.5 GB", 1.5, DataSizeUnit.Gigabyte)]
    [InlineData("500 MB", 500, DataSizeUnit.Megabyte)]
    [InlineData("0 Byte", 0, DataSizeUnit.Byte)]
    [InlineData("1024 KB", 1024, DataSizeUnit.Kilobyte)]
    [InlineData("  1.5   GB  ", 1.5, DataSizeUnit.Gigabyte)]
    [InlineData("1 gb", 1, DataSizeUnit.Gigabyte)]
    [InlineData("1 GIGABYTE", 1, DataSizeUnit.Gigabyte)]
    public void Create_ValidString_ReturnsSuccess(string input, decimal expectedAmount, DataSizeUnit expectedUnit)
    {
        Result<DataSize> result = DataSize.Create(input);

        Assert.True(result.IsSuccess);
        Assert.Equal(expectedAmount, result.Value.Value);
        Assert.Equal(expectedUnit, result.Value.Unit);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_NullOrWhitespaceString_ReturnsEmptyError(string? input)
    {
        Result<DataSize> result = DataSize.Create(input!);

        Assert.True(result.IsFailure);
        Assert.Equal("KernelR.ValueObjects.DataSize.EMPTY", result.Error.Code);
    }

    [Theory]
    [InlineData("1.5")]
    [InlineData("1.5 GB extra")]
    [InlineData("1.5GB")]
    public void Create_InvalidStringFormat_ReturnsInvalidFormatError(string input)
    {
        Result<DataSize> result = DataSize.Create(input);

        Assert.True(result.IsFailure);
        Assert.Equal("KernelR.ValueObjects.DataSize.INVALID_FORMAT", result.Error.Code);
    }

    [Fact]
    public void Create_InvalidStringAmount_ReturnsInvalidAmountError()
    {
        Result<DataSize> result = DataSize.Create("abc GB");

        Assert.True(result.IsFailure);
        Assert.Equal("KernelR.ValueObjects.DataSize.INVALID_AMOUNT", result.Error.Code);
    }

    [Fact]
    public void Create_InvalidStringUnit_ReturnsInvalidUnitError()
    {
        Result<DataSize> result = DataSize.Create("1.5 ZB");

        Assert.True(result.IsFailure);
        Assert.Equal("KernelR.ValueObjects.DataSize.INVALID_UNIT", result.Error.Code);
    }

    [Fact]
    public void Zero_ValidUnit_ReturnsZeroAmount()
    {
        Result<DataSize> result = DataSize.Zero(DataSizeUnit.Megabyte);

        Assert.True(result.IsSuccess);
        Assert.Equal(0, result.Value.Value);
        Assert.Equal(DataSizeUnit.Megabyte, result.Value.Unit);
    }

    [Fact]
    public void Parse_ValidString_ReturnsDataSize()
    {
        DataSize size = DataSize.Parse("1.5 GB", null);

        Assert.Equal(1.5m, size.Value);
        Assert.Equal(DataSizeUnit.Gigabyte, size.Unit);
    }

    [Fact]
    public void Parse_InvalidString_ThrowsFormatException()
    {
        Assert.Throws<FormatException>(() => DataSize.Parse("invalid", null));
    }

    [Theory]
    [InlineData("1.5 GB", true)]
    [InlineData("invalid", false)]
    [InlineData(null, false)]
    public void TryParse_VariousInputs_ReturnsExpectedResult(string? input, bool expectedSuccess)
    {
        bool success = DataSize.TryParse(input, null, out DataSize result);

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
        DataSize s1 = DataSize.Create(100, DataSizeUnit.Kilobyte).Value;
        DataSize s2 = DataSize.Create(200, DataSizeUnit.Kilobyte).Value;

        Result<DataSize> result = s1.Add(s2);

        Assert.True(result.IsSuccess);
        Assert.Equal(300, result.Value.Value);
        Assert.Equal(DataSizeUnit.Kilobyte, result.Value.Unit);
    }

    [Fact]
    public void Add_DifferentUnits_ConvertsAndReturnsSum()
    {
        DataSize s1 = DataSize.Create(1, DataSizeUnit.Gigabyte).Value;
        DataSize s2 = DataSize.Create(1024, DataSizeUnit.Megabyte).Value;

        Result<DataSize> result = s1.Add(s2);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.Value);
        Assert.Equal(DataSizeUnit.Gigabyte, result.Value.Unit);
    }

    [Fact]
    public void Subtract_SameUnit_ReturnsDifference()
    {
        DataSize s1 = DataSize.Create(200, DataSizeUnit.Megabyte).Value;
        DataSize s2 = DataSize.Create(50, DataSizeUnit.Megabyte).Value;

        Result<DataSize> result = s1.Subtract(s2);

        Assert.True(result.IsSuccess);
        Assert.Equal(150, result.Value.Value);
    }

    [Fact]
    public void Subtract_DifferentUnits_ConvertsAndReturnsDifference()
    {
        DataSize s1 = DataSize.Create(1, DataSizeUnit.Gigabyte).Value;
        DataSize s2 = DataSize.Create(512, DataSizeUnit.Megabyte).Value;

        Result<DataSize> result = s1.Subtract(s2);

        Assert.True(result.IsSuccess);
        Assert.Equal(0.5m, result.Value.Value);
        Assert.Equal(DataSizeUnit.Gigabyte, result.Value.Unit);
    }

    [Fact]
    public void Subtract_ResultNegative_ReturnsNegativeResultError()
    {
        DataSize s1 = DataSize.Create(100, DataSizeUnit.Kilobyte).Value;
        DataSize s2 = DataSize.Create(200, DataSizeUnit.Kilobyte).Value;

        Result<DataSize> result = s1.Subtract(s2);

        Assert.True(result.IsFailure);
        Assert.Equal("KernelR.ValueObjects.DataSize.NEGATIVE_RESULT", result.Error.Code);
    }

    [Fact]
    public void ComparisonMethods_ReturnCorrectBool()
    {
        DataSize s1 = DataSize.Create(1, DataSizeUnit.Gigabyte).Value;
        DataSize s2 = DataSize.Create(500, DataSizeUnit.Megabyte).Value;
        DataSize s3 = DataSize.Create(1024, DataSizeUnit.Megabyte).Value;

        Assert.True(s1.GreaterThan(s2).Value);
        Assert.False(s1.LessThan(s2).Value);
        Assert.True(s1.GreaterThanOrEqual(s3).Value);
        Assert.True(s1.LessThanOrEqual(s3).Value);
    }

    [Fact]
    public void Operator_Plus_ReturnsSum()
    {
        DataSize s1 = DataSize.Create(1, DataSizeUnit.Gigabyte).Value;
        DataSize s2 = DataSize.Create(1024, DataSizeUnit.Megabyte).Value;

        DataSize result = s1 + s2;

        Assert.Equal(2, result.Value);
        Assert.Equal(DataSizeUnit.Gigabyte, result.Unit);
    }

    [Fact]
    public void Operator_Minus_ReturnsDifference()
    {
        DataSize s1 = DataSize.Create(1, DataSizeUnit.Gigabyte).Value;
        DataSize s2 = DataSize.Create(512, DataSizeUnit.Megabyte).Value;

        DataSize result = s1 - s2;

        Assert.Equal(0.5m, result.Value);
    }

    [Fact]
    public void Operator_Minus_NegativeResult_ThrowsInvalidOperationException()
    {
        DataSize s1 = DataSize.Create(100, DataSizeUnit.Byte).Value;
        DataSize s2 = DataSize.Create(200, DataSizeUnit.Byte).Value;

        Assert.Throws<InvalidOperationException>(() => s1 - s2);
    }

    [Fact]
    public void ComparisonOperators_ReturnCorrectBool()
    {
        DataSize s1 = DataSize.Create(1, DataSizeUnit.Gigabyte).Value;
        DataSize s2 = DataSize.Create(500, DataSizeUnit.Megabyte).Value;

        Assert.True(s1 > s2);
        Assert.False(s1 < s2);
        Assert.True(s1 >= s2);
        Assert.False(s1 <= s2);
    }

    [Fact]
    public void CompareTo_SameUnit_ReturnsIntegerComparison()
    {
        DataSize s1 = DataSize.Create(2, DataSizeUnit.Gigabyte).Value;
        DataSize s2 = DataSize.Create(1, DataSizeUnit.Gigabyte).Value;
        DataSize s3 = DataSize.Create(2, DataSizeUnit.Gigabyte).Value;

        Assert.True(s1.CompareTo(s2) > 0);
        Assert.True(s2.CompareTo(s1) < 0);
        Assert.Equal(0, s1.CompareTo(s3));
    }

    [Fact]
    public void CompareTo_DifferentUnits_ReturnsIntegerComparison()
    {
        DataSize s1 = DataSize.Create(1, DataSizeUnit.Gigabyte).Value;
        DataSize s2 = DataSize.Create(1024, DataSizeUnit.Megabyte).Value;

        Assert.Equal(0, s1.CompareTo(s2));
    }

    [Fact]
    public void IComparableCompareTo_WithNull_ReturnsOne()
    {
        DataSize size = DataSize.Create(1, DataSizeUnit.Gigabyte).Value;
        IComparable comparable = size;

        Assert.Equal(1, comparable.CompareTo(null));
    }

    [Fact]
    public void IComparableCompareTo_WithInvalidType_ThrowsArgumentException()
    {
        DataSize size = DataSize.Create(1, DataSizeUnit.Gigabyte).Value;
        IComparable comparable = size;

        Assert.Throws<ArgumentException>(() => comparable.CompareTo("NotDataSize"));
    }

    [Fact]
    public void Conversions_ReturnCorrectValues()
    {
        DataSize oneGigabyte = DataSize.Create(1, DataSizeUnit.Gigabyte).Value;

        Assert.Equal(1024, oneGigabyte.ToMegabyte().Value);
        Assert.Equal(1048576, oneGigabyte.ToKilobyte().Value);
        Assert.Equal(1073741824, oneGigabyte.ToByte().Value);
        Assert.Equal(0.0009765625m, oneGigabyte.ToTerabyte().Value);
    }

    [Fact]
    public void Conversion_SameUnit_ReturnsSelf()
    {
        DataSize size = DataSize.Create(5, DataSizeUnit.Megabyte).Value;

        DataSize result = size.ToMegabyte();

        Assert.Equal(size, result);
    }

    [Fact]
    public void ToString_ReturnsFormattedString()
    {
        DataSize size = DataSize.Create(1.5m, DataSizeUnit.Gigabyte).Value;

        string result = size.ToString();

        Assert.Equal("1.5 Gigabyte", result);
    }

    [Fact]
    public void ExplicitOperator_ReturnsString()
    {
        DataSize size = DataSize.Create(500, DataSizeUnit.Megabyte).Value;

        string result = (string)size;

        Assert.Equal("500 Megabyte", result);
    }

    [Fact]
    public void Equality_SameValueAndUnit_ReturnsTrue()
    {
        DataSize s1 = DataSize.Create(1, DataSizeUnit.Gigabyte).Value;
        DataSize s2 = DataSize.Create(1, DataSizeUnit.Gigabyte).Value;

        Assert.Equal(s1, s2);
        Assert.True(s1 == s2);
        Assert.False(s1 != s2);
    }

    [Fact]
    public void Equality_DifferentValuesOrUnits_ReturnsFalse()
    {
        DataSize s1 = DataSize.Create(1, DataSizeUnit.Gigabyte).Value;
        DataSize s2 = DataSize.Create(1024, DataSizeUnit.Megabyte).Value;
        DataSize s3 = DataSize.Create(2, DataSizeUnit.Gigabyte).Value;

        Assert.NotEqual(s1, s2);
        Assert.NotEqual(s1, s3);
    }
}
