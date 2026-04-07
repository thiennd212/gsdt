
namespace GSDT.MasterData.Domain.Entities;

/// <summary>
/// Administrative unit (Đơn vị hành chính) — tracks VN admin reform successor chains.
/// IsActive=false means merged/dissolved. SuccessorCode points to current active unit.
/// </summary>
public class AdministrativeUnit : Entity<Guid>
{
    public string Code { get; private set; } = default!;
    public string NameVi { get; private set; } = default!;
    public string NameEn { get; private set; } = default!;

    /// <summary>Administrative level: 1=Province, 2=District, 3=Ward.</summary>
    public int Level { get; private set; }

    public string? ParentCode { get; private set; }

    /// <summary>Code of the active successor after merger/reform. Null if still active.</summary>
    public string? SuccessorCode { get; private set; }

    public bool IsActive { get; private set; }

    /// <summary>Date when this unit was dissolved (null if still active).</summary>
    public DateTimeOffset? EffectiveTo { get; private set; }

    private AdministrativeUnit() { }

    public static AdministrativeUnit Create(
        string code, string nameVi, string nameEn, int level,
        string? parentCode, string? successorCode, bool isActive,
        DateTimeOffset? effectiveTo = null) =>
        new()
        {
            Id = Guid.NewGuid(),
            Code = code,
            NameVi = nameVi,
            NameEn = nameEn,
            Level = level,
            ParentCode = parentCode,
            SuccessorCode = successorCode,
            IsActive = isActive,
            EffectiveTo = effectiveTo
        };
}
