using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;
using Nearbly.Application.Common;

namespace Nearbly.Infrastructure.Storage;

public sealed class ImageSharpProcessor : IImageProcessor
{
    private const long MaxBytes = 5 * 1024 * 1024;

    public async Task<ProcessedImage> ProcessAsync(MediaUpload upload, CancellationToken cancellationToken)
    {
        if (upload.Length <= 0 || upload.Length > MaxBytes)
            throw new ArgumentException("Images must be between 1 byte and 5 MB.", nameof(upload));
        if (upload.ContentType is not ("image/jpeg" or "image/png" or "image/webp"))
            throw new ArgumentException("Only JPEG, PNG and WebP images are supported.", nameof(upload));

        try
        {
            using var image = await Image.LoadAsync(upload.Content, cancellationToken);
            if (image.Width <= 0 || image.Height <= 0) throw new ArgumentException("Image dimensions are invalid.");
            image.Mutate(context => context.Resize(new ResizeOptions { Mode = ResizeMode.Max, Size = new Size(1600, 1600) }));
            image.Metadata.ExifProfile = null;
            image.Metadata.IptcProfile = null;
            image.Metadata.XmpProfile = null;
            var output = new MemoryStream();
            await image.SaveAsWebpAsync(output, new WebpEncoder { Quality = 82 }, cancellationToken);
            output.Position = 0;
            return new ProcessedImage(output, "image/webp", output.Length, image.Width, image.Height);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (ArgumentException)
        {
            throw;
        }
        catch (Exception exception)
        {
            throw new ArgumentException("The uploaded file is not a valid supported image.", nameof(upload), exception);
        }
    }
}
