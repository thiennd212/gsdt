using GSDT.MasterData.Enums;

namespace GSDT.MasterData.Entities;

/// <summary>Ward (Phường/Xã/Thị trấn) — loaded into IMemoryCache NeverRemove at startup.</summary>
public class Ward : Entity<Guid>
{
    public string Code { get; private set; } = default!;
    public string DistrictCode { get; private set; } = default!;
    public string NameVi { get; private set; } = default!;
    public string NameEn { get; private set; } = default!;
    public bool IsActive { get; private set; }
    public int SortOrder { get; private set; }
    public DateTime? EffectiveDate { get; private set; }
    public AdministrativeStatus Status { get; private set; } = AdministrativeStatus.Active;

    private Ward() { }

    public static Ward Create(string code, string districtCode, string nameVi, string nameEn, int sortOrder = 0) =>
        new()
        {
            Id = Guid.NewGuid(),
            Code = code,
            DistrictCode = districtCode,
            NameVi = nameVi,
            NameEn = nameEn,
            IsActive = true,
            SortOrder = sortOrder
        };
}
