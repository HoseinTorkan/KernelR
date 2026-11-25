using KernelR.Domain.ValueObjects.Common;
using KernelR.Patterns.Results;

namespace KernelR.Domain.Tests.ValueObjects.Common;

public class ColorTests
{
    [Theory]
    [InlineData(255, 0, 0, 0)]
    [InlineData(128, 255, 128, 64)]
    [InlineData(0, 255, 255, 255)]
    public void FromArgb_ReturnsCorrectComponents(byte a, byte r, byte g, byte b)
    {
        Color color = Color.FromArgb(a, r, g, b);

        Assert.Equal(a, color.A);
        Assert.Equal(r, color.R);
        Assert.Equal(g, color.G);
        Assert.Equal(b, color.B);
    }

    [Theory]
    [InlineData(255, 87, 51)]
    [InlineData(0, 0, 0)]
    [InlineData(255, 255, 255)]
    public void FromRgb_SetsAlphaTo255(byte r, byte g, byte b)
    {
        Color color = Color.FromRgb(r, g, b);

        Assert.Equal(255, color.A);
        Assert.Equal(r, color.R);
        Assert.Equal(g, color.G);
        Assert.Equal(b, color.B);
    }

    [Theory]
    [InlineData("#FF5733", 255, 255, 87, 51)]
    [InlineData("#80FF5733", 128, 255, 87, 51)]
    [InlineData("#000000", 255, 0, 0, 0)]
    [InlineData("#ffffff", 255, 255, 255, 255)]
    [InlineData("  #Ff5733  ", 255, 255, 87, 51)]
    public void FromHex_ValidHex_ReturnsSuccess(string hex, byte expectedA, byte expectedR, byte expectedG, byte expectedB)
    {
        Result<Color> result = Color.FromHex(hex);

        Assert.True(result.IsSuccess);
        Assert.Equal(expectedA, result.Value.A);
        Assert.Equal(expectedR, result.Value.R);
        Assert.Equal(expectedG, result.Value.G);
        Assert.Equal(expectedB, result.Value.B);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void FromHex_NullOrWhitespace_ReturnsEmptyError(string? input)
    {
        Result<Color> result = Color.FromHex(input!);

        Assert.True(result.IsFailure);
        Assert.Equal("KernelR.ValueObjects.Color.EMPTY", result.Error.Code);
    }

    [Theory]
    [InlineData("FF5733")]
    [InlineData("#FF573")]
    [InlineData("#FF57331")]
    [InlineData("#GG5733")]
    [InlineData("#FF57")]
    public void FromHex_InvalidFormat_ReturnsInvalidFormatError(string input)
    {
        Result<Color> result = Color.FromHex(input);

        Assert.True(result.IsFailure);
        Assert.Equal("KernelR.ValueObjects.Color.INVALID_FORMAT", result.Error.Code);
    }

    [Fact]
    public void FromHsl_ValidHsl_ReturnsColor()
    {
        HslColor hsl = HslColor.Create(0, 100, 50).Value;

        Color color = Color.FromHsl(hsl);

        Assert.Equal(255, color.R);
        Assert.Equal(0, color.G);
        Assert.Equal(0, color.B);
    }

    [Fact]
    public void PredefinedColors_ReturnCorrectValues()
    {
        Assert.Equal(new Color(0, 0, 0, 0), Color.Transparent);
        Assert.Equal(new Color(255, 0, 0, 0), Color.Black);
        Assert.Equal(new Color(255, 255, 255, 255), Color.White);
        Assert.Equal(new Color(255, 255, 0, 0), Color.Red);
        Assert.Equal(new Color(255, 0, 128, 0), Color.Green);
        Assert.Equal(new Color(255, 0, 0, 255), Color.Blue);
    }

    [Theory]
    [InlineData(255, 255, 87, 51, "#FF5733")]
    [InlineData(128, 255, 87, 51, "#80FF5733")]
    [InlineData(255, 0, 0, 0, "#000000")]
    [InlineData(0, 0, 0, 0, "#00000000")]
    public void Hex_ReturnsCorrectFormat(byte a, byte r, byte g, byte b, string expectedHex)
    {
        Color color = Color.FromArgb(a, r, g, b);

        Assert.Equal(expectedHex, color.Hex);
    }

    [Fact]
    public void IsTransparent_ReturnsTrueOnlyWhenAlphaIsZero()
    {
        Assert.True(Color.FromArgb(0, 255, 255, 255).IsTransparent);
        Assert.False(Color.FromArgb(1, 255, 255, 255).IsTransparent);
    }

    [Fact]
    public void BrightnessAndIsDark_ReturnCorrectValues()
    {
        Assert.Equal(0, Color.Black.Brightness);
        Assert.True(Color.Black.IsDark);

        Assert.Equal(1, Color.White.Brightness);
        Assert.False(Color.White.IsDark);
    }

    [Fact]
    public void Parse_ValidHex_ReturnsColor()
    {
        Color color = Color.Parse("#FF5733", null);

        Assert.Equal(255, color.R);
        Assert.Equal(87, color.G);
        Assert.Equal(51, color.B);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("Invalid")]
    public void Parse_InvalidHex_ThrowsFormatException(string? input)
    {
        Assert.Throws<FormatException>(() => Color.Parse(input!, null));
    }

    [Fact]
    public void TryParse_ValidHex_ReturnsTrueAndOutValue()
    {
        bool success = Color.TryParse("#FF5733", null, out Color result);

        Assert.True(success);
        Assert.Equal(255, result.R);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("Invalid")]
    public void TryParse_InvalidHex_ReturnsFalseAndDefaultValue(string? input)
    {
        bool success = Color.TryParse(input, null, out Color result);

        Assert.False(success);
        Assert.Equal(default, result);
    }

    [Fact]
    public void WithAlpha_ReturnsNewColorWithSameRgb()
    {
        Color color = Color.FromRgb(100, 150, 200);

        Color result = color.WithAlpha(128);

        Assert.Equal(128, result.A);
        Assert.Equal(100, result.R);
        Assert.Equal(150, result.G);
        Assert.Equal(200, result.B);
    }

    [Fact]
    public void Lighten_ReturnsLighterColor()
    {
        Color color = Color.FromRgb(100, 100, 100);

        Color result = color.Lighten(0.5);

        Assert.Equal(178, result.R);
        Assert.Equal(178, result.G);
        Assert.Equal(178, result.B);
    }

    [Theory]
    [InlineData(-0.1)]
    [InlineData(1.1)]
    public void Lighten_AmountOutOfRange_ThrowsArgumentOutOfRangeException(double amount)
    {
        Color color = Color.FromRgb(100, 100, 100);

        Assert.Throws<ArgumentOutOfRangeException>(() => color.Lighten(amount));
    }

    [Fact]
    public void Darken_ReturnsDarkerColor()
    {
        Color color = Color.FromRgb(200, 200, 200);

        Color result = color.Darken(0.5);

        Assert.Equal(100, result.R);
        Assert.Equal(100, result.G);
        Assert.Equal(100, result.B);
    }

    [Theory]
    [InlineData(-0.1)]
    [InlineData(1.1)]
    public void Darken_AmountOutOfRange_ThrowsArgumentOutOfRangeException(double amount)
    {
        Color color = Color.FromRgb(100, 100, 100);

        Assert.Throws<ArgumentOutOfRangeException>(() => color.Darken(amount));
    }

    [Fact]
    public void Invert_ReturnsInvertedColor()
    {
        Color black = Color.Black;
        Color white = Color.White;

        Assert.Equal(white, black.Invert());
        Assert.Equal(black, white.Invert());
    }

    [Fact]
    public void ToRgbString_ReturnsCorrectFormat()
    {
        Color color = Color.FromRgb(255, 87, 51);

        string result = color.ToRgbString();

        Assert.Equal("rgb(255, 87, 51)", result);
    }

    [Theory]
    [InlineData(255, "rgba(255, 87, 51, 1)")]
    [InlineData(128, "rgba(255, 87, 51, 0.5)")]
    [InlineData(0, "rgba(255, 87, 51, 0)")]
    public void ToRgbaString_ReturnsCorrectFormat(byte alpha, string expected)
    {
        Color color = Color.FromArgb(alpha, 255, 87, 51);

        string result = color.ToRgbaString();

        Assert.Equal(expected, result);
    }

    [Fact]
    public void ToHsl_ReturnsCorrectHslValues()
    {
        Color red = Color.FromRgb(255, 0, 0);
        Color white = Color.FromRgb(255, 255, 255);
        Color black = Color.FromRgb(0, 0, 0);

        HslColor redHsl = red.ToHsl();
        HslColor whiteHsl = white.ToHsl();
        HslColor blackHsl = black.ToHsl();

        Assert.Equal(0, redHsl.H);
        Assert.Equal(100, redHsl.S);
        Assert.Equal(50, redHsl.L);

        Assert.Equal(0, whiteHsl.S);
        Assert.Equal(100, whiteHsl.L);

        Assert.Equal(0, blackHsl.S);
        Assert.Equal(0, blackHsl.L);
    }

    [Fact]
    public void ToString_ReturnsHex()
    {
        Color color = Color.FromRgb(255, 87, 51);

        Assert.Equal("#FF5733", color.ToString());
    }

    [Fact]
    public void ExplicitOperator_ReturnsHex()
    {
        Color color = Color.FromRgb(255, 87, 51);

        string result = (string)color;

        Assert.Equal("#FF5733", result);
    }

    [Fact]
    public void Equality_SameComponents_ReturnsTrue()
    {
        Color color1 = Color.FromRgb(255, 87, 51);
        Color color2 = Color.FromRgb(255, 87, 51);

        Assert.Equal(color1, color2);
        Assert.True(color1 == color2);
        Assert.False(color1 != color2);
    }

    [Fact]
    public void Equality_DifferentComponents_ReturnsFalse()
    {
        Color color1 = Color.FromRgb(255, 87, 51);
        Color color2 = Color.FromRgb(255, 87, 52);

        Assert.NotEqual(color1, color2);
        Assert.False(color1 == color2);
        Assert.True(color1 != color2);
    }
}
