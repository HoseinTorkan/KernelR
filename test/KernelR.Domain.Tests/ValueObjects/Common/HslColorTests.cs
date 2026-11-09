using KernelR.Domain.ValueObjects.Common;
using KernelR.Patterns.Results;

namespace KernelR.Domain.Tests.ValueObjects.Common;

public class HslColorTests
{
    [Theory]
    [InlineData(0, 0, 0)]
    [InlineData(120, 50, 50)]
    [InlineData(360, 100, 100)]
    [InlineData(240.5, 75.5, 25.5)]
    public void Create_ValidComponents_ReturnsSuccess(double h, double s, double l)
    {
        Result<HslColor> result = HslColor.Create(h, s, l);

        Assert.True(result.IsSuccess);
        Assert.Equal(h, result.Value.H);
        Assert.Equal(s, result.Value.S);
        Assert.Equal(l, result.Value.L);
    }

    [Theory]
    [InlineData(-1, 50, 50)]
    [InlineData(361, 50, 50)]
    public void Create_InvalidHue_ReturnsFailure(double h, double s, double l)
    {
        Result<HslColor> result = HslColor.Create(h, s, l);

        Assert.True(result.IsFailure);
        Assert.Equal("KernelR.ValueObjects.HslColor.INVALID_COMPONENTS", result.Error.Code);
        Assert.Contains("Hue", result.Error.Message);
    }

    [Theory]
    [InlineData(120, -1, 50)]
    [InlineData(120, 101, 50)]
    public void Create_InvalidSaturation_ReturnsFailure(double h, double s, double l)
    {
        Result<HslColor> result = HslColor.Create(h, s, l);

        Assert.True(result.IsFailure);
        Assert.Equal("KernelR.ValueObjects.HslColor.INVALID_COMPONENTS", result.Error.Code);
        Assert.Contains("Saturation", result.Error.Message);
    }

    [Theory]
    [InlineData(120, 50, -1)]
    [InlineData(120, 50, 101)]
    public void Create_InvalidLightness_ReturnsFailure(double h, double s, double l)
    {
        Result<HslColor> result = HslColor.Create(h, s, l);

        Assert.True(result.IsFailure);
        Assert.Equal("KernelR.ValueObjects.HslColor.INVALID_COMPONENTS", result.Error.Code);
        Assert.Contains("Lightness", result.Error.Message);
    }

    [Theory]
    [InlineData("120,50,50", 120, 50, 50)]
    [InlineData("0, 0, 0", 0, 0, 0)]
    [InlineData(" 360 , 100 , 100 ", 360, 100, 100)]
    [InlineData("120.5,50.5,50.5", 120.5, 50.5, 50.5)]
    public void Parse_ValidString_ReturnsHslColor(string input, double expectedH, double expectedS, double expectedL)
    {
        HslColor hsl = HslColor.Parse(input, null);

        Assert.Equal(expectedH, hsl.H);
        Assert.Equal(expectedS, hsl.S);
        Assert.Equal(expectedL, hsl.L);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("120")]
    [InlineData("120,50")]
    [InlineData("120,50,50,50")]
    [InlineData("abc,50,50")]
    [InlineData("120,abc,50")]
    [InlineData("120,50,abc")]
    [InlineData("-10,50,50")]
    [InlineData("370,50,50")]
    public void Parse_InvalidString_ThrowsFormatException(string? input)
    {
        Assert.Throws<FormatException>(() => HslColor.Parse(input!, null));
    }

    [Theory]
    [InlineData("120,50,50", true)]
    [InlineData("0, 0, 0", true)]
    [InlineData(" 360 , 100 , 100 ", true)]
    [InlineData("120.5,50.5,50.5", true)]
    [InlineData(null, false)]
    [InlineData("", false)]
    [InlineData("   ", false)]
    [InlineData("120", false)]
    [InlineData("120,50", false)]
    [InlineData("120,50,50,50", false)]
    [InlineData("abc,50,50", false)]
    [InlineData("120,abc,50", false)]
    [InlineData("120,50,abc", false)]
    [InlineData("-10,50,50", false)]
    [InlineData("370,50,50", false)]
    public void TryParse_VariousInputs_ReturnsExpectedResult(string? input, bool expectedSuccess)
    {
        bool success = HslColor.TryParse(input, null, out HslColor result);

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
    public void ToColor_PureRed_ReturnsCorrectRgb()
    {
        HslColor hsl = HslColor.Create(0, 100, 50).Value;

        Color color = hsl.ToColor();

        Assert.Equal(255, color.R);
        Assert.Equal(0, color.G);
        Assert.Equal(0, color.B);
    }

    [Fact]
    public void ToColor_PureGreen_ReturnsCorrectRgb()
    {
        HslColor hsl = HslColor.Create(120, 100, 50).Value;

        Color color = hsl.ToColor();

        Assert.Equal(0, color.R);
        Assert.Equal(255, color.G);
        Assert.Equal(0, color.B);
    }

    [Fact]
    public void ToColor_PureBlue_ReturnsCorrectRgb()
    {
        HslColor hsl = HslColor.Create(240, 100, 50).Value;

        Color color = hsl.ToColor();

        Assert.Equal(0, color.R);
        Assert.Equal(0, color.G);
        Assert.Equal(255, color.B);
    }

    [Fact]
    public void ToColor_White_ReturnsCorrectRgb()
    {
        HslColor hsl = HslColor.Create(0, 0, 100).Value;

        Color color = hsl.ToColor();

        Assert.Equal(255, color.R);
        Assert.Equal(255, color.G);
        Assert.Equal(255, color.B);
    }

    [Fact]
    public void ToColor_Black_ReturnsCorrectRgb()
    {
        HslColor hsl = HslColor.Create(0, 0, 0).Value;

        Color color = hsl.ToColor();

        Assert.Equal(0, color.R);
        Assert.Equal(0, color.G);
        Assert.Equal(0, color.B);
    }

    [Fact]
    public void ToColor_Gray_ReturnsCorrectRgb()
    {
        HslColor hsl = HslColor.Create(0, 0, 50).Value;

        Color color = hsl.ToColor();

        Assert.Equal(128, color.R);
        Assert.Equal(128, color.G);
        Assert.Equal(128, color.B);
    }

    [Fact]
    public void ToString_ReturnsCorrectFormat()
    {
        HslColor hsl = HslColor.Create(120, 50, 50).Value;

        string result = hsl.ToString();

        Assert.Equal("hsl(120, 50%, 50%)", result);
    }

    [Fact]
    public void ToString_WithDecimals_ReturnsCorrectFormat()
    {
        HslColor hsl = HslColor.Create(120.5, 50.5, 50.5).Value;

        string result = hsl.ToString();

        Assert.Equal("hsl(120.5, 50.5%, 50.5%)", result);
    }

    [Fact]
    public void Equality_SameValues_ReturnsTrue()
    {
        HslColor hsl1 = HslColor.Create(120, 50, 50).Value;
        HslColor hsl2 = HslColor.Create(120, 50, 50).Value;

        Assert.Equal(hsl1, hsl2);
        Assert.True(hsl1 == hsl2);
        Assert.False(hsl1 != hsl2);
    }

    [Fact]
    public void Equality_DifferentValues_ReturnsFalse()
    {
        HslColor hsl1 = HslColor.Create(120, 50, 50).Value;
        HslColor hsl2 = HslColor.Create(121, 50, 50).Value;

        Assert.NotEqual(hsl1, hsl2);
        Assert.False(hsl1 == hsl2);
        Assert.True(hsl1 != hsl2);
    }
}
