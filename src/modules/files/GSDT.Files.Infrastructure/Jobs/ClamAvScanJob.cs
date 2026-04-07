
namespace GSDT.Files.Infrastructure.Jobs;

/// <summary>
/// Phase 2 of two-phase upload: async Hangfire job for ClamAV virus scan.
/// Pass → FileStatus.Available (file accessible for download).
/// Fail → FileStatus.Rejected + delete from MinIO (file permanently removed).
/// Executed in Hangfire "virus-scan" queue with retry on transient errors.
/// Gated by feature:files.virus-scan flag — when disabled, auto-passes (dev/demo mode).
/// </summary>
public sealed class ClamAvScanJob(
    IFileRepository fileRepository,
    IFileStorageService storageService,
    IVirusScanner virusScanner,
    IFeatureFlagService featureFlags,
    IOptions<FilesOptions> options,
    ILogger<ClamAvScanJob> logger) : IClamAvScanJob
{
    public async Task ScanAsync(Guid fileId)
    {
        var fileResult = await fileRepository.GetByIdAsync(FileId.From(fileId));
        if (fileResult.IsFailed)
        {
            logger.LogWarning("ClamAvScanJob: FileRecord {FileId} not found.", fileId);
            return;
        }

        var file = fileResult.Value;

        if (file.Status != FileStatus.Quarantined)
        {
            logger.LogInformation(
                "ClamAvScanJob: File {FileId} already processed (status={Status}). Skipping.",
                fileId, file.Status);
            return;
        }

        // Feature flag gate: skip virus scan when disabled (dev/demo mode)
        // SECURITY: In production, feature:files.virus-scan MUST be enabled (NĐ68 compliance).
        if (!featureFlags.IsEnabled("feature:files.virus-scan"))
        {
            file.MarkAvailable();
            await fileRepository.UpdateAsync(file);
            logger.LogCritical(
                "SECURITY BYPASS: Virus scan disabled by feature flag. File {FileId} marked Available WITHOUT scan. " +
                "Ensure feature:files.virus-scan is enabled in production.",
                fileId);
            return;
        }

        try
        {
            // Download from MinIO for scanning
            await using var stream = await storageService.DownloadAsync(
                options.Value.BucketName, file.StorageKey);

            var scanResult = await virusScanner.ScanAsync(stream);

            if (scanResult.Status == VirusScanStatus.Clean)
            {
                file.MarkAvailable();
                await fileRepository.UpdateAsync(file);
                logger.LogInformation(
                    "ClamAvScanJob: File {FileId} scan PASSED → Available.", fileId);
            }
            else
            {
                file.MarkRejected();
                await fileRepository.UpdateAsync(file);

                // Delete from MinIO — infected file must not remain in storage
                await storageService.DeleteAsync(options.Value.BucketName, file.StorageKey);

                logger.LogWarning(
                    "ClamAvScanJob: File {FileId} scan FAILED (virus={VirusName}) → Rejected + deleted from storage.",
                    fileId, scanResult.VirusName);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "ClamAvScanJob: Unhandled error scanning file {FileId}.", fileId);
            throw; // Hangfire retries on exception
        }
    }
}
