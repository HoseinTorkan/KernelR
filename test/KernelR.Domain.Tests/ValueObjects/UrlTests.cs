namespace KernelR.Domain.Tests.ValueObjects;

public class UrlTests
{
    // ─────────── Create ───────────

    [Theory]
    [InlineData("https://example.com", "https://example.com/")]
    [InlineData("http://example.com/path", "http://example.com/path")]
    [InlineData("  https://example.com/path  ", "https://example.com/path")]
    [InlineData("HTTPS://EXAMPLE.COM", "https://example.com/")]
    public void Create_WithValidUrl_ReturnsNormalizedSuccess(string input, string expected)
    {
        Result<Url> result = Url.Create(input);

        Assert.True(result.IsSuccess);
        Assert.Equal(expected, result.Value.Value);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_WithEmptyInput_ReturnsEmpty(string? input)
    {
        Result<Url> result = Url.Create(input!);

        Assert.True(result.IsFailure);
        Assert.Equal("KernelR.ValueObjects.Url.EMPTY", result.Error.Code);
    }

    [Theory]
    [InlineData("not a url")]
    [InlineData("example.com")]
    [InlineData("/relative/path")]
    public void Create_WithInvalidFormat_ReturnsInvalidFormat(string input)
    {
        Result<Url> result = Url.Create(input);

        Assert.True(result.IsFailure);
        Assert.Equal("KernelR.ValueObjects.Url.INVALID_FORMAT", result.Error.Code);
    }

    [Theory]
    [InlineData("ftp://example.com")]
    [InlineData("file:///path")]
    [InlineData("mailto:user@example.com")]
    public void Create_WithInvalidScheme_ReturnsInvalidScheme(string input)
    {
        Result<Url> result = Url.Create(input);

        Assert.True(result.IsFailure);
        Assert.Equal("KernelR.ValueObjects.Url.INVALID_SCHEME", result.Error.Code);
    }

    // ─────────── IParsable ───────────

    [Fact]
    public void TryParse_WithValidInput_ReturnsTrue()
    {
        bool success = Url.TryParse("https://example.com", null, out Url result);

        Assert.True(success);
        Assert.Equal("https://example.com/", result.Value);
    }

    [Fact]
    public void TryParse_WithInvalidInput_ReturnsFalse()
    {
        bool success = Url.TryParse("invalid", null, out Url result);

        Assert.False(success);
        Assert.Equal(default, result);
    }

    [Fact]
    public void Parse_WithValidInput_ReturnsUrl()
    {
        Url result = Url.Parse("https://example.com", null);

        Assert.Equal("https://example.com/", result.Value);
    }

    [Fact]
    public void Parse_WithInvalidInput_ThrowsFormatException()
    {
        Assert.Throws<FormatException>(() => Url.Parse("invalid", null));
    }

    // ─────────── ToString & implicit operator ───────────

    [Fact]
    public void ToString_ReturnsNormalizedValue()
    {
        Url url = Url.Create("https://example.com").Value;

        Assert.Equal("https://example.com/", url.ToString());
    }

    [Fact]
    public void ImplicitOperator_ConvertsToString()
    {
        Url url = Url.Create("https://example.com").Value;

        string result = url;

        Assert.Equal("https://example.com/", result);
    }
}
