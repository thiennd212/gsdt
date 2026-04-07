namespace GSDT.Files.Domain.Services;

/// <summary>
/// ClamAV virus scanner abstraction.
/// Dev fallback: configurable bypass with warning log when ClamAV daemon unreachable.
/// </summary>
public interface IVirusScanner
{
    /// <summary>Scan stream for viruses. Returns result with virus name if infected.</summary>
    Task<VirusScanResult> ScanAsync(Stream stream, CancellationToken cancellationToken = default);
}

public sealed record VirusScanResult(VirusScanStatus Status, string? VirusName = null)
{
    public static VirusScanResult Clean() => new(VirusScanStatus.Clean);
    public static VirusScanResult Infected(string virusName) => new(VirusScanStatus.Infected, virusName);
    public static VirusScanResult Error(string reason) => new(VirusScanStatus.Error, reason);
}

public enum VirusScanStatus { Clean, Infected, Error }
