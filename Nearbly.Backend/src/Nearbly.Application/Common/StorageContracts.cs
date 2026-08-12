namespace Nearbly.Application.Common;

public sealed record MediaUpload(Stream Content, string FileName, string ContentType, long Length);
public sealed record ProcessedImage(Stream Content, string MimeType, long Length, int Width, int Height);
public sealed record StoredObject(Stream Content, string ContentType, long Length);

public interface IImageProcessor
{
    Task<ProcessedImage> ProcessAsync(MediaUpload upload, CancellationToken cancellationToken);
}

public interface IObjectStorage
{
    Task PutAsync(string key, Stream content, string contentType, CancellationToken cancellationToken);
    Task<StoredObject?> OpenReadAsync(string key, CancellationToken cancellationToken);
}
