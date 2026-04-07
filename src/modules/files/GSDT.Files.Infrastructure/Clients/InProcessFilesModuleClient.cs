
namespace GSDT.Files.Infrastructure.Clients;

/// <summary>
/// Monolith in-process implementation of IFilesModuleClient.
/// Zero overhead — direct repository call within same process.
/// Swap for GrpcFilesModuleClient when Files module is extracted to microservice (1 DI line change).
/// </summary>
public sealed class InProcessFilesModuleClient(IFileRepository fileRepository) : IFilesModuleClient
{
    public async Task<bool> IsFileAvailableAsync(
        Guid fileId,
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        var result = await fileRepository.GetByIdAsync(FileId.From(fileId), cancellationToken);
        if (result.IsFailed) return false;

        var file = result.Value;
        return file.TenantId == tenantId && file.Status == FileStatus.Available;
    }

    public async Task<FileReferenceInfo?> GetFileReferenceAsync(
        Guid fileId,
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        var result = await fileRepository.GetByIdAsync(FileId.From(fileId), cancellationToken);
        if (result.IsFailed) return null;

        var file = result.Value;
        if (file.TenantId != tenantId) return null;

        return new FileReferenceInfo(
            file.Id.Value,
            file.OriginalFileName,
            file.ContentType,
            file.SizeBytes,
            file.Status.ToString());
    }
}
