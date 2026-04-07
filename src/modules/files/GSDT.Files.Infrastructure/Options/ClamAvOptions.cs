namespace GSDT.Files.Infrastructure.Options;

/// <summary>ClamAV daemon connection options — bound from appsettings "ClamAV" section.</summary>
public sealed class ClamAvOptions
{
    public const string SectionName = "ClamAV";

    public string Host { get; init; } = "localhost";
    public int Port { get; init; } = 3310;
    public int TimeoutMs { get; init; } = 30000; // 30s per NF2

    /// <summary>When true: log warning and pass scan if ClamAV daemon is unreachable (dev/staging only).</summary>
    public bool BypassWhenUnavailable { get; init; } = false;
}
