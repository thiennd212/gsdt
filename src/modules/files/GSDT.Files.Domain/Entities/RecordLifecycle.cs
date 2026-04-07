
namespace GSDT.Files.Domain.Entities;

/// <summary>
/// Tracks the lifecycle state of a FileRecord under a RetentionPolicy.
/// One RecordLifecycle per FileRecord — created when retention policy is applied.
/// CurrentStatus transitions: Active → Archived → PendingDestruction → Destroyed.
/// ScheduledDestroyAt populated when policy's DestroyAfterDays threshold is reached.
/// </summary>
public sealed class RecordLifecycle : AuditableEntity<Guid>
{
    public Guid FileRecordId { get; private set; }
    public RecordLifecycleStatus CurrentStatus { get; private set; }
    public Guid? RetentionPolicyId { get; private set; }
    public DateTime? ArchivedAt { get; private set; }
    public DateTime? ScheduledDestroyAt { get; private set; }
    public DateTime? DestroyedAt { get; private set; }
    public Guid? DestroyedBy { get; private set; }

    private RecordLifecycle() { } // EF Core

    public static RecordLifecycle Create(
        Guid fileRecordId,
        Guid? retentionPolicyId,
        Guid createdBy)
    {
        var lifecycle = new RecordLifecycle
        {
            Id = Guid.NewGuid(),
            FileRecordId = fileRecordId,
            RetentionPolicyId = retentionPolicyId,
            CurrentStatus = RecordLifecycleStatus.Active
        };
        lifecycle.SetAuditCreate(createdBy);
        return lifecycle;
    }

    /// <summary>Moves record to archived state — file may be moved to cold storage.</summary>
    public void Archive(Guid modifiedBy)
    {
        CurrentStatus = RecordLifecycleStatus.Archived;
        ArchivedAt = DateTime.UtcNow;
        SetAuditUpdate(modifiedBy);
    }

    /// <summary>Schedules record for destruction at the given time.</summary>
    public void ScheduleDestruction(DateTime scheduledDestroyAt, Guid modifiedBy)
    {
        CurrentStatus = RecordLifecycleStatus.PendingDestruction;
        ScheduledDestroyAt = scheduledDestroyAt;
        SetAuditUpdate(modifiedBy);
    }

    /// <summary>Marks record as permanently destroyed — irreversible.</summary>
    public void MarkDestroyed(Guid destroyedBy)
    {
        CurrentStatus = RecordLifecycleStatus.Destroyed;
        DestroyedAt = DateTime.UtcNow;
        DestroyedBy = destroyedBy;
        SetAuditUpdate(destroyedBy);
    }
}

public enum RecordLifecycleStatus
{
    Active = 0,
    Archived = 1,
    PendingDestruction = 2,
    Destroyed = 3
}
