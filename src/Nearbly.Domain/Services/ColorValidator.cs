using System.Text.RegularExpressions;

namespace Nearbly.Domain.Services;

public static partial class ColorValidator
{
    [GeneratedRegex("^#[0-9A-Fa-f]{6}$")]
    private static partial Regex HexColorRegex();

    public static bool IsValid(string? value) => value is null || HexColorRegex().IsMatch(value);
}
