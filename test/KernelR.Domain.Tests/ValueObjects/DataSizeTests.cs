namespace KernelR.Domain.Tests.ValueObjects;

public class DataSizeTests
{
    // ─────────── Create(decimal, DataSizeUnit) ───────────

    [Theory]
    [InlineData(1.5, DataSizeUnit.Gigabyte, 1.5, DataSizeUnit.Gigabyte)]
    [InlineData(0, DataSizeUnit.Byte, 0, DataSizeUnit.Byte)]
    [InlineData(500, DataSizeUnit.Megabyte, 500, DataSizeUnit.Megabyte)]
    public void Create_WithValidInput_ReturnsSuccess(decimal value, DataSizeUnit unit, decimal expectedValue, DataSizeUnit expectedUnit)
    {
        Result<DataSize> result = DataSize.Create(value, unit);

        Assert.True(result.IsSuccess);
        Assert.Equal(expectedValue, result.Value.Value);
        Assert.Equal(expectedUnit, result.Value.Unit);
    }

    [Fact]
    public void Create_WithNegativeValue_ReturnsInvalidValue()
    {
        Result<DataSize> result = DataSize.Create(-1, DataSizeUnit.Byte);

        Assert.True(result.IsFailure);
        Assert.Equal("KernelR.ValueObjects.DataSize.INVALID_VALUE", result.Error.Code);
    }

    [Fact]
    public void Create_WithTooLargeValue_ReturnsTooLarge()
    {
        Result<DataSize> result = DataSize.Create(1_000_001, DataSizeUnit.Byte);

        Assert.True(result.IsFailure);
        Assert.Equal("KernelR.ValueObjects.DataSize.TOO_LARGE", result.Error.Code);
    }

    [Fact]
    public void Create_WithUndefinedUnit_ReturnsInvalidUnit()
    {
        Result<DataSize> result = DataSize.Create(100, (DataSizeUnit)99);

        Assert.True(result.IsFailure);
        Assert.Equal("KernelR.ValueObjects.DataSize.INVALID_UNIT", result.Error.Code);
    }

    // ─────────── Create(string) ───────────

    [Theory]
    [InlineData("1.5 GB", 1.5, DataSizeUnit.Gigabyte)]
    [InlineData("500 MB", 500, DataSizeUnit.Megabyte)]
    [InlineData("0 Byte", 0, DataSizeUnit.Byte)]
    [InlineData("2 TB", 2, DataSizeUnit.Terabyte)]
    [InlineData("1024 KB", 1024, DataSizeUnit.Kilobyte)]
    public void Create_WithValidString_ReturnsSuccess(string input, decimal expectedValue, DataSizeUnit expectedUnit)
    {
        Result<DataSize> result = DataSize.Create(input);

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
        Result<DataSize> result = DataSize.Create(input!);

        Assert.True(result.IsFailure);
        Assert.Equal("KernelR.ValueObjects.DataSize.EMPTY", result.Error.Code);
    }

    [Theory]
    [InlineData("1.5")]
    [InlineData("GB")]
    [InlineData("1.5 GB extra")]
    public void Create_WithInvalidFormat_ReturnsInvalidFormat(string input)
    {
        Result<DataSize> result = DataSize.Create(input);

        Assert.True(result.IsFailure);
        Assert.Equal("KernelR.ValueObjects.DataSize.INVALID_FORMAT", result.Error.Code);
    }

    [Fact]
    public void Create_WithInvalidNumber_ReturnsInvalidValue()
    {
        Result<DataSize> result = DataSize.Create("abc GB");

        Assert.True(result.IsFailure);
        Assert.Equal("KernelR.ValueObjects.DataSize.INVALID_VALUE", result.Error.Code);
    }

    [Fact]
    public void Create_WithInvalidUnit_ReturnsInvalidUnit()
    {
        Result<DataSize> result = DataSize.Create("5 xyz");

        Assert.True(result.IsFailure);
        Assert.Equal("KernelR.ValueObjects.DataSize.INVALID_UNIT", result.Error.Code);
    }

    // ─────────── IParsable ───────────

    [Fact]
    public void TryParse_WithValidInput_ReturnsTrue()
    {
        bool success = DataSize.TryParse("1.5 GB", null, out DataSize result);

        Assert.True(success);
        Assert.Equal(1.5m, result.Value);
    }

    [Fact]
    public void TryParse_WithInvalidInput_ReturnsFalse()
    {
        bool success = DataSize.TryParse("invalid", null, out DataSize result);

        Assert.False(success);
        Assert.Equal(default, result);
    }

    [Fact]
    public void Parse_WithValidInput_ReturnsDataSize()
    {
        DataSize result = DataSize.Parse("1.5 GB", null);

        Assert.Equal(1.5m, result.Value);
    }

    [Fact]
    public void Parse_WithInvalidInput_ThrowsFormatException()
    {
        Assert.Throws<FormatException>(() => DataSize.Parse("invalid", null));
    }

    // ─────────── Zero ───────────

    [Fact]
    public void Zero_ReturnsZeroAmountInUnit()
    {
        Result<DataSize> result = DataSize.Zero(DataSizeUnit.Gigabyte);

        Assert.True(result.IsSuccess);
        Assert.Equal(0, result.Value.Value);
        Assert.Equal(DataSizeUnit.Gigabyte, result.Value.Unit);
    }

    // ─────────── Conversions ───────────

    [Fact]
    public void ToKilobyte_FromMegabyte_Returns1024()
    {
        DataSize mb = DataSize.Create(1, DataSizeUnit.Megabyte).Value;

        DataSize kb = mb.ToKilobyte();

        Assert.Equal(1024, kb.Value);
        Assert.Equal(DataSizeUnit.Kilobyte, kb.Unit);
    }

    [Fact]
    public void ToByte_FromByte_ReturnsSame()
    {
        DataSize b = DataSize.Create(100, DataSizeUnit.Byte).Value;

        DataSize same = b.ToByte();

        Assert.Equal(b, same);
    }

    // ─────────── ToString & implicit operator ───────────

    [Fact]
    public void ToString_ReturnsValueAndUnit()
    {
        DataSize size = DataSize.Create(1.5m, DataSizeUnit.Gigabyte).Value;

        Assert.Equal("1.5 Gigabyte", size.ToString());
    }

    [Fact]
    public void ImplicitOperator_ConvertsToString()
    {
        DataSize size = DataSize.Create(1.5m, DataSizeUnit.Gigabyte).Value;

        string result = size;

        Assert.Equal("1.5 Gigabyte", result);
    }
}
