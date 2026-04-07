
namespace GSDT.Organization.Entities;

/// <summary>
/// Immutable record of a staff member's position in an org unit over time.
/// Written on assign and transfer; never updated — append-only history.
/// </summary>
public class StaffPositionHistory : AuditableEntity<Guid>
{
    [DataClassification(DataClassificationLevel.Internal)]
    public Guid UserId { get; private set; }
    public Guid OrgUnitId { get; private set; }
    [DataClassification(DataClassificationLevel.Confidential)]
    public string PositionTitle { get; private set; } = string.Empty;
    public DateTimeOffset StartDate { get; private set; }
    public DateTimeOffset? EndDate { get; private set; }
    public Guid TenantId { get; private set; }

    private StaffPositionHistory() { }

    public static StaffPositionHistory Create(
        Guid userId,
        Guid orgUnitId,
        string positionTitle,
        Guid tenantId,
        DateTimeOffset? startDate = null)
    {
        return new StaffPositionHistory
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            OrgUnitId = orgUnitId,
            PositionTitle = positionTitle,
            TenantId = tenantId,
            StartDate = startDate ?? DateTimeOffset.UtcNow
        };
    }

    /// <summary>Closes the position record on transfer or departure.</summary>
    public void Close(DateTimeOffset endDate)
    {
        EndDate = endDate;
        MarkUpdated();
    }
}
