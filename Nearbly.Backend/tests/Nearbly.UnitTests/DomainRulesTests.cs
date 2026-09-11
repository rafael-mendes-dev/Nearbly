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
    [InlineData("https://www.google.com/maps/dir//JV+UNIFORMES+ESCOLARES+-+R.+Hip%C3%B3lito+Cesar+Sobrinho,+110+-+Uberaba,+Curitiba+-+PR,+81590-337/@-25.4860123,-49.2092385,17z/data=!4m17!1m7!3m6!1s0x94dcfb994229381f:0x9aef791e0ec8d63e!2sJV+UNIFORMES+ESCOLARES!8m2!3d-25.4860172!4d-49.2066636!16s%2Fg%2F11h022cqmt!4m8!1m0!1m5!1m1!1s0x94dcfb994229381f:0x9aef791e0ec8d63e!2m2!1d-49.2066638!2d-25.4860181!3e0?entry=ttu&g_ep=EgoyMDI2MDkwOC4wIKXMDSoASAFQAw%3D%3D", true)]
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
