using System.Text.Json;

namespace GSDT.Identity.Domain.Entities;

/// <summary>
/// Extended IdentityUser — teams add custom fields via ExtraProperties without forking.
/// Convention: prefix ExtraProperties keys with module name, e.g. "hr.employee_code".
/// </summary>
public class ApplicationUser : IdentityUser<Guid>
{
    // IdentityUser inherited PII properties — shadowed to apply DataClassification attributes.
    [DataClassification(DataClassificationLevel.Confidential)]
    public override string? Email { get => base.Email; set => base.Email = value; }

    [DataClassification(DataClassificationLevel.Confidential)]
    public override string? PhoneNumber { get => base.PhoneNumber; set => base.PhoneNumber = value; }

    [DataClassification(DataClassificationLevel.Restricted)]
    public override string? PasswordHash { get => base.PasswordHash; set => base.PasswordHash = value; }

    [DataClassification(DataClassificationLevel.Restricted)]
    public override string? SecurityStamp { get => base.SecurityStamp; set => base.SecurityStamp = value; }

    [DataClassification(DataClassificationLevel.Internal)]
    public override string? ConcurrencyStamp { get => base.ConcurrencyStamp; set => base.ConcurrencyStamp = value; }

    /// <summary>IdentityUser.UserName — shadowed to apply DataClassification.</summary>
    [DataClassification(DataClassificationLevel.Confidential)]
    public override string? UserName { get => base.UserName; set => base.UserName = value; }

    [DataClassification(DataClassificationLevel.Confidential)]
    public string FullName { get; set; } = string.Empty;

    /// <summary>Department code — used by ABAC DepartmentAccessHandler.</summary>
    [DataClassification(DataClassificationLevel.Confidential)]
    public string? DepartmentCode { get; set; }

    /// <summary>Data classification clearance per QĐ742.</summary>
    public ClassificationLevel ClearanceLevel { get; set; } = ClassificationLevel.Internal;

    /// <summary>Soft-delete / admin lock flag.</summary>
    public bool IsActive { get; set; } = true;

    public Guid? TenantId { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? LastLoginAt { get; set; }

    /// <summary>Password expiry enforcement per QĐ742 password policy.</summary>
    public DateTime? PasswordExpiresAt { get; set; }

    /// <summary>HR employee code, e.g. "CB001234". Cross-module reference — no FK constraint.</summary>
    [DataClassification(DataClassificationLevel.Confidential)]
    public string? EmployeeCode { get; set; }

    /// <summary>Primary organizational unit (cross-module ref to organization.OrgUnit — no nav property).</summary>
    public Guid? PrimaryOrgUnitId { get; set; }

    /// <summary>Display position/title text, e.g. "Chuyên viên xử lý hồ sơ".</summary>
    public string? PositionName { get; set; }

    /// <summary>Authentication source: LOCAL | AD | VNEID | LDAP. Defaults to LOCAL.</summary>
    public string AuthSource { get; set; } = "LOCAL";

    /// <summary>
    /// Extension point: teams add custom fields without forking ApplicationUser.
    /// Key = "module.field_name", Value = JSON-serialized value.
    /// EF Core serializes via value converter (nvarchar(max)).
    /// </summary>
    public Dictionary<string, JsonElement> ExtraProperties { get; set; } = new();

    // Navigation
    public ICollection<UserDelegation> DelegationsAsDelegate { get; set; } = new List<UserDelegation>();
    public ICollection<UserDelegation> DelegationsAsDelegator { get; set; } = new List<UserDelegation>();
}
