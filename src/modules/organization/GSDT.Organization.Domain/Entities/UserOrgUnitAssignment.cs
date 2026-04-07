
namespace GSDT.Organization.Domain.Entities;

/// <summary>
/// Temporal assignment of a user to an org unit with a role.
/// ValidFrom/ValidTo enables assignment history tracking.
/// </summary>
public class UserOrgUnitAssignment : AuditableEntity<Guid>
{
    [DataClassification(DataClassificationLevel.Internal)]
    public Guid UserId { get; private set; }
    public Guid OrgUnitId { get; private set; }
    public string RoleInOrg { get; private set; } = string.Empty;
    public DateTimeOffset ValidFrom { get; private set; }
    public DateTimeOffset? ValidTo { get; private set; }
    public bool IsActive { get; private set; }
    public Guid TenantId { get; private set; }

    private UserOrgUnitAssignment() { }

    public static UserOrgUnitAssignment Create(
        Guid userId, Guid orgUnitId, string roleInOrg,
        Guid tenantId, DateTimeOffset? validFrom = null)
    {
        return new UserOrgUnitAssignment
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            OrgUnitId = orgUnitId,
            RoleInOrg = roleInOrg,
            TenantId = tenantId,
            ValidFrom = validFrom ?? DateTimeOffset.UtcNow,
            IsActive = true
        };
    }

    public void Close(DateTimeOffset? closedAt = null)
    {
        ValidTo = closedAt ?? DateTimeOffset.UtcNow;
        IsActive = false;
        MarkUpdated();
    }
}
