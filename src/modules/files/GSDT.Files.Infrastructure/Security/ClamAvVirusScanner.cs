using nClam;

namespace GSDT.Files.Infrastructure.Security;

/// <summary>
/// ClamAV virus scanner via TCP connection to clamd daemon (nClam library).
/// Dev fallback: if ClamAV unreachable, logs warning and returns Clean (bypass mode).
/// Configure ClamAV:BypassInDev=true to enable bypass.
/// Scan timeout: 30s (NF2 requirement).
/// </summary>
public sealed class ClamAvVirusScanner(
    IOptions<ClamAvOptions> options,
    ILogger<ClamAvVirusScanner> logger) : IVirusScanner
{
    public async Task<VirusScanResult> ScanAsync(
        Stream stream,
        CancellationToken cancellationToken = default)
    {
        var opts = options.Value;

        try
        {
            var client = new ClamClient(opts.Host, opts.Port)
            {
                MaxStreamSize = 200 * 1024 * 1024 // 200 MB safety cap
            };

            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(TimeSpan.FromMilliseconds(opts.TimeoutMs));

            stream.Position = 0;
            var result = await client.SendAndScanFileAsync(stream, cts.Token);

            return result.Result switch
            {
                ClamScanResults.Clean => VirusScanResult.Clean(),
                ClamScanResults.VirusDetected => VirusScanResult.Infected(
                    result.InfectedFiles?.FirstOrDefault()?.VirusName ?? "Unknown"),
                _ => VirusScanResult.Error($"ClamAV returned: {result.Result}")
            };
        }
        catch (Exception ex) when (opts.BypassWhenUnavailable)
        {
            logger.LogWarning(ex,
                "ClamAV unavailable — bypass mode active. File not scanned. Host={Host}:{Port}",
                opts.Host, opts.Port);
            return VirusScanResult.Clean(); // dev/staging bypass
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "ClamAV scan failed. Host={Host}:{Port}", opts.Host, opts.Port);
            return VirusScanResult.Error(ex.Message);
        }
    }
}
