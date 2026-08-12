using Nearbly.Application.Common;
using Microsoft.Extensions.Configuration;

namespace Nearbly.Infrastructure.Storage;

public sealed class LocalObjectStorage(IConfiguration configuration) : IObjectStorage
{
    private readonly string root = configuration["Media:RootPath"] is { Length: > 0 } configured
        ? Path.GetFullPath(configured)
        : Path.Combine(AppContext.BaseDirectory, "media");

    public async Task PutAsync(string key, Stream content, string contentType, CancellationToken cancellationToken)
    {
        var path = GetPath(key);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        await using var output = File.Create(path);
        await content.CopyToAsync(output, cancellationToken);
    }

    public Task<StoredObject?> OpenReadAsync(string key, CancellationToken cancellationToken)
    {
        var path = GetPath(key);
        if (!File.Exists(path)) return Task.FromResult<StoredObject?>(null);
        Stream stream = File.OpenRead(path);
        return Task.FromResult<StoredObject?>(new StoredObject(stream, "image/webp", stream.Length));
    }

    private string GetPath(string key)
    {
        var normalized = key.Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar);
        if (normalized.Contains("..", StringComparison.Ordinal)) throw new ArgumentException("Invalid storage key.", nameof(key));
        return Path.Combine(root, normalized);
    }
}
