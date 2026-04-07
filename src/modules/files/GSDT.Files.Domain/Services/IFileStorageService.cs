namespace GSDT.Files.Domain.Services;

/// <summary>
/// Object storage abstraction — swap MinIO for Azure Blob / AWS S3 via DI without code changes.
/// All operations are async-streaming; no full buffer in memory.
/// </summary>
public interface IFileStorageService
{
    /// <summary>Upload stream to storage. Returns the storage key used.</summary>
    Task<string> UploadAsync(Stream stream, string bucket, string key, string contentType, CancellationToken cancellationToken = default);

    /// <summary>Download stream from storage. Caller is responsible for disposing the stream.</summary>
    Task<Stream> DownloadAsync(string bucket, string key, CancellationToken cancellationToken = default);

    /// <summary>Permanently delete object from storage.</summary>
    Task DeleteAsync(string bucket, string key, CancellationToken cancellationToken = default);
}
