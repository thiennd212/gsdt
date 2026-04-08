using Minio;
using Minio.DataModel.Args;

namespace GSDT.Files.Infrastructure.Storage;

/// <summary>
/// MinIO S3-compatible storage adapter.
/// Swap for Azure Blob / AWS S3 by replacing this implementation via DI — interface unchanged.
/// F-05: RecyclableMemoryStreamManager used for downloads — avoids LOH pressure for large files.
/// </summary>
public sealed class MinioFileStorageService(
    IMinioClient minioClient,
    RecyclableMemoryStreamManager streamManager,
    ILogger<MinioFileStorageService> logger) : IFileStorageService
{
    public async Task<string> UploadAsync(
        Stream stream,
        string bucket,
        string key,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        await EnsureBucketExistsAsync(bucket, cancellationToken);

        var args = new PutObjectArgs()
            .WithBucket(bucket)
            .WithObject(key)
            .WithStreamData(stream)
            .WithObjectSize(stream.Length > 0 ? stream.Length : -1)
            .WithContentType(contentType);

        await minioClient.PutObjectAsync(args, cancellationToken);

        logger.LogDebug("Uploaded object {Key} to bucket {Bucket}", key, bucket);
        return key;
    }

    public async Task<Stream> DownloadAsync(
        string bucket,
        string key,
        CancellationToken cancellationToken = default)
    {
        // F-05: RecyclableMemoryStream — avoids MemoryStream LOH allocations for large downloads
        var recyclableStream = streamManager.GetStream("minio-download");

        var args = new GetObjectArgs()
            .WithBucket(bucket)
            .WithObject(key)
            .WithCallbackStream(async (stream, ct) =>
            {
                await stream.CopyToAsync(recyclableStream, ct);
            });

        await minioClient.GetObjectAsync(args, cancellationToken);
        recyclableStream.Position = 0;

        return recyclableStream;
    }

    public async Task DeleteAsync(
        string bucket,
        string key,
        CancellationToken cancellationToken = default)
    {
        var args = new RemoveObjectArgs()
            .WithBucket(bucket)
            .WithObject(key);

        await minioClient.RemoveObjectAsync(args, cancellationToken);
        logger.LogDebug("Deleted object {Key} from bucket {Bucket}", key, bucket);
    }

    private async Task EnsureBucketExistsAsync(string bucket, CancellationToken cancellationToken)
    {
        var exists = await minioClient.BucketExistsAsync(
            new BucketExistsArgs().WithBucket(bucket), cancellationToken);

        if (!exists)
        {
            await minioClient.MakeBucketAsync(
                new MakeBucketArgs().WithBucket(bucket), cancellationToken);
            logger.LogInformation("Created MinIO bucket {Bucket}", bucket);
        }
    }
}
