
namespace GSDT.SystemParams.Entities;

public enum AnnouncementSeverity { Info, Warning, Critical }

/// <summary>System-wide announcement shown to users. Cached 1min on public endpoint.</summary>
public class SystemAnnouncement : AuditableEntity<Guid>
{
    public string Title { get; private set; } = default!;
    public string Content { get; private set; } = default!;
    public AnnouncementSeverity Severity { get; private set; }
    public DateTimeOffset? StartAt { get; private set; }
    public DateTimeOffset? EndAt { get; private set; }
    public bool IsActive { get; private set; }

    /// <summary>JSON array of role names; null means all users.</summary>
    public string? TargetRoles { get; private set; }

    private SystemAnnouncement() { }

    public static SystemAnnouncement Create(
        string title, string content, AnnouncementSeverity severity,
        DateTimeOffset? startAt = null, DateTimeOffset? endAt = null,
        string? targetRoles = null) =>
        new()
        {
            Id = Guid.NewGuid(),
            Title = title,
            Content = content,
            Severity = severity,
            StartAt = startAt,
            EndAt = endAt,
            IsActive = true,
            TargetRoles = targetRoles
        };

    public void Update(string title, string content, AnnouncementSeverity severity,
        DateTimeOffset? startAt, DateTimeOffset? endAt, bool isActive)
    {
        Title = title;
        Content = content;
        Severity = severity;
        StartAt = startAt;
        EndAt = endAt;
        IsActive = isActive;
        MarkUpdated();
    }

    public void Deactivate() { IsActive = false; MarkUpdated(); }
}
