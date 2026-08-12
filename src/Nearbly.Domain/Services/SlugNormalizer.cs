using System.Globalization;
using System.Text;

namespace Nearbly.Domain.Services;

public static class SlugNormalizer
{
    public static string Normalize(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Slug is required.", nameof(value));

        var normalized = value.Trim().Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(normalized.Length);
        foreach (var character in normalized)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(character) == UnicodeCategory.NonSpacingMark)
                continue;
            if (char.IsLetterOrDigit(character))
                builder.Append(char.ToLowerInvariant(character));
            else if (builder.Length > 0 && builder[^1] != '-')
                builder.Append('-');
        }

        var result = builder.ToString().Trim('-');
        if (result.Length == 0)
            throw new ArgumentException("Slug must contain at least one letter or digit.", nameof(value));
        return result;
    }
}
