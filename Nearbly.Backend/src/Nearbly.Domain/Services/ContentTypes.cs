namespace Nearbly.Domain.Services;

public enum ContentType
{
    Links,
    Products,
    Markdown,
    Gallery
}

public static class ContentTypeParser
{
    public static ContentType Parse(string? value) => value?.Trim().ToLowerInvariant() switch
    {
        null or "" or "links" => ContentType.Links,
        "products" => ContentType.Products,
        "markdown" => ContentType.Markdown,
        "gallery" => ContentType.Gallery,
        _ => throw new ArgumentException("ContentType must be links, products, markdown or gallery.", nameof(value))
    };

    public static string ToWireValue(this ContentType value) => value switch
    {
        ContentType.Products => "products",
        ContentType.Markdown => "markdown",
        ContentType.Gallery => "gallery",
        _ => "links"
    };
}
