namespace Nearbly.Domain.Services;

public static class UrlValidator
{
    public static bool IsValid(string? value)
    {
        if (!Uri.TryCreate(value, UriKind.Absolute, out var uri))
            return false;
        if (uri.Scheme is not ("http" or "https") || string.IsNullOrWhiteSpace(uri.Host))
            return false;
        return string.IsNullOrEmpty(uri.UserInfo);
    }
}
