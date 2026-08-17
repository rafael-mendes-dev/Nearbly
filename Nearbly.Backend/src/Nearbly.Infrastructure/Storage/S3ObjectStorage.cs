using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Nearbly.Application.Common;
using Microsoft.Extensions.Configuration;

namespace Nearbly.Infrastructure.Storage;

public sealed class S3ObjectStorage(IConfiguration configuration) : IObjectStorage
{
    private readonly string bucket = configuration["Media:S3:Bucket"] ?? throw new InvalidOperationException("Media:S3:Bucket is required.");
    private readonly IAmazonS3 client = CreateClient(configuration);

    public async Task PutAsync(string key, Stream content, string contentType, CancellationToken cancellationToken) =>
        // R2 doesn't support the chunked/streaming SigV4 payload signing the SDK uses by default,
        // which surfaces as an opaque "Authorization" error on PutObject.
        await client.PutObjectAsync(new PutObjectRequest { BucketName = bucket, Key = key, InputStream = content, ContentType = contentType, DisablePayloadSigning = true, UseChunkEncoding = false }, cancellationToken);

    public async Task<StoredObject?> OpenReadAsync(string key, CancellationToken cancellationToken)
    {
        try
        {
            var response = await client.GetObjectAsync(bucket, key, cancellationToken);
            return new StoredObject(response.ResponseStream, response.Headers.ContentType ?? "application/octet-stream", response.Headers.ContentLength);
        }
        catch (AmazonS3Exception exception) when (exception.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    private static IAmazonS3 CreateClient(IConfiguration configuration)
    {
        // AWS SDK v4 defaults to attaching an automatic CRC32 request checksum, which R2 doesn't
        // handle correctly and which also surfaces as an opaque "Authorization" error.
        var config = new AmazonS3Config
        {
            ServiceURL = configuration["Media:S3:Endpoint"],
            ForcePathStyle = true,
            AuthenticationRegion = "auto",
            RequestChecksumCalculation = RequestChecksumCalculation.WHEN_REQUIRED,
            ResponseChecksumValidation = ResponseChecksumValidation.WHEN_REQUIRED
        };
        return new AmazonS3Client(configuration["Media:S3:AccessKey"], configuration["Media:S3:SecretKey"], config);
    }
}
