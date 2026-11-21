using KernelR.Domain.ValueObjects.Network;
using KernelR.Patterns.Results;

namespace KernelR.Domain.Tests.ValueObjects.Contact;

public class UrlTests
{
    [Theory]
    [InlineData("https://example.com")]
    [InlineData("http://example.com")]
    [InlineData("https://example.com/path")]
    [InlineData("http://sub.domain.com:8080/api/v1?query=value#fragment")]
    [InlineData("https://example.com/image.jpg")]
    public void Create_ValidUrl_ReturnsSuccess(string validUrl)
    {
        Result<Url> result = Url.Create(validUrl);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value.Value);
    }

    [Theory]
    [InlineData("  https://example.com/path  ", "https://example.com/path")]
    [InlineData("HTTPS://EXAMPLE.COM/PATH", "https://example.com/PATH")]
    [InlineData("http://example.com", "http://example.com/")]
    public void Create_ValidUrl_NormalizesValue(string input, string expectedValue)
    {
        Result<Url> result = Url.Create(input);

        Assert.True(result.IsSuccess);
        Assert.Equal(expectedValue, result.Value.Value);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_NullOrWhitespace_ReturnsEmptyError(string? input)
    {
        Result<Url> result = Url.Create(input!);

        Assert.True(result.IsFailure);
        Assert.Equal("KernelR.ValueObjects.Url.EMPTY", result.Error.Code);
        Assert.Equal("URL cannot be empty.", result.Error.Message);
    }

    [Theory]
    [InlineData("example.com")]
    [InlineData("/path/to/resource")]
    [InlineData("not-a-url")]
    [InlineData("http://")]
    [InlineData("https://.com")]
    public void Create_InvalidFormat_ReturnsInvalidFormatError(string invalidUrl)
    {
        Result<Url> result = Url.Create(invalidUrl);

        Assert.True(result.IsFailure);
        Assert.Equal("KernelR.ValueObjects.Url.INVALID_FORMAT", result.Error.Code);
        Assert.Equal("URL is not a valid absolute URI.", result.Error.Message);
    }

    [Theory]
    [InlineData("ftp://example.com")]
    [InlineData("file:///C:/path/to/file")]
    [InlineData("mailto:test@example.com")]
    [InlineData("ws://example.com")]
    public void Create_InvalidScheme_ReturnsInvalidSchemeError(string invalidSchemeUrl)
    {
        Result<Url> result = Url.Create(invalidSchemeUrl);

        Assert.True(result.IsFailure);
        Assert.Equal("KernelR.ValueObjects.Url.INVALID_SCHEME", result.Error.Code);
        Assert.Equal("URL must use http or https.", result.Error.Message);
    }

    [Fact]
    public void Parse_ValidUrl_ReturnsUrl()
    {
        string input = "  HTTPS://EXAMPLE.COM/PATH  ";

        Url url = Url.Parse(input, null);

        Assert.Equal("https://example.com/PATH", url.Value);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("invalid-url")]
    [InlineData("ftp://example.com")]
    public void Parse_InvalidUrl_ThrowsFormatException(string? input)
    {
        Assert.Throws<FormatException>(() => Url.Parse(input!, null));
    }

    [Theory]
    [InlineData("https://example.com", true)]
    [InlineData("http://example.com/path", true)]
    [InlineData(null, false)]
    [InlineData("", false)]
    [InlineData("invalid-url", false)]
    [InlineData("ftp://example.com", false)]
    public void TryParse_VariousInputs_ReturnsExpectedResult(string? input, bool expectedSuccess)
    {
        bool success = Url.TryParse(input, null, out Url result);

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
    public void ToString_ReturnsValue()
    {
        Url url = Url.Create("https://example.com/path").Value;

        string result = url.ToString();

        Assert.Equal("https://example.com/path", result);
    }

    [Fact]
    public void ExplicitOperator_ReturnsValue()
    {
        Url url = Url.Create("https://example.com/path").Value;

        string result = (string)url;

        Assert.Equal("https://example.com/path", result);
    }

    [Fact]
    public void Equality_SameNormalizedUrl_ReturnsTrue()
    {
        Url url1 = Url.Create("  https://example.com/path  ").Value;
        Url url2 = Url.Create("HTTPS://EXAMPLE.COM/PATH").Value;

        Assert.Equal(url1, url2);
        Assert.True(url1 == url2);
        Assert.False(url1 != url2);
    }

    [Fact]
    public void Equality_DifferentUrls_ReturnsFalse()
    {
        Url url1 = Url.Create("https://example.com/path1").Value;
        Url url2 = Url.Create("https://example.com/path2").Value;

        Assert.NotEqual(url1, url2);
        Assert.False(url1 == url2);
        Assert.True(url1 != url2);
    }
}
