using Nearbly.Domain.Services;

namespace Nearbly.UnitTests;

public sealed class DomainRulesTests
{
    [Theory]
    [InlineData("São Paulo / Café", "sao-paulo-cafe")]
    [InlineData("  Loja__Dois  ", "loja-dois")]
    [InlineData("áéíóú", "aeiou")]
    public void NormalizeSlug_RemovesDiacriticsAndConsolidatesSeparators(string input, string expected)
    {
        Assert.Equal(expected, SlugNormalizer.Normalize(input));
    }

    [Theory]
    [InlineData("https://example.com/path", true)]
    [InlineData("http://example.com", true)]
    [InlineData("ftp://example.com", false)]
    [InlineData("https://user:pass@example.com", false)]
    [InlineData("example.com", false)]
    public void ValidateUrl_OnlyAllowsAbsoluteHttpUrlsWithoutCredentials(string input, bool expected)
    {
        Assert.Equal(expected, UrlValidator.IsValid(input));
    }

    [Theory]
    [InlineData("#AABBCC", true)]
    [InlineData("#a1b2c3", true)]
    [InlineData("AABBCC", false)]
    [InlineData("#ABCDE", false)]
    public void ValidateColor_RequiresSixDigitHex(string input, bool expected)
    {
        Assert.Equal(expected, ColorValidator.IsValid(input));
    }

    [Theory]
    [InlineData(0, 20, 0)]
    [InlineData(100, 5, 5)]
    [InlineData(3, 1, 33.33)]
    public void CalculateCtr_DoesNotDivideByZero(long views, long clicks, decimal expected)
    {
        Assert.Equal(expected, AnalyticsMetrics.CalculateCtr(views, clicks));
    }
}
