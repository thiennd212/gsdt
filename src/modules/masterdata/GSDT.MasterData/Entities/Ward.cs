using GSDT.MasterData.Enums;

namespace GSDT.MasterData.Entities;

/// <summary>
/// Ward (Phường/Xã/Thị trấn/Đặc khu) — supports both:
///   Old 3-tier: Province → District → Ward (DistrictCode filled, ProvinceCode null)
///   New 2-tier: Province → Ward (ProvinceCode filled, DistrictCode null) per QĐ 19/2025
/// </summary>
public class Ward : Entity<Guid>
{
    public string Code { get; private set; } = default!;
    /// <summary>Old 3-tier: district parent. Null for new 2-tier wards.</summary>
    public string? DistrictCode { get; private set; }
    /// <summary>New 2-tier: direct province parent. Null for old 3-tier wards (derive from District).</summary>
    public string? ProvinceCode { get; private set; }
    public string NameVi { get; private set; } = default!;
    public string NameEn { get; private set; } = default!;
    public bool IsActive { get; private set; }
    public int SortOrder { get; private set; }
    public DateTime? EffectiveDate { get; private set; }
    public AdministrativeStatus Status { get; private set; } = AdministrativeStatus.Active;

    private Ward() { }

    /// <summary>Create old 3-tier ward (district-based).</summary>
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

    /// <summary>Create new 2-tier ward (province-based, QĐ 19/2025).</summary>
    public static Ward CreateNew2Tier(string code, string provinceCode, string nameVi, string nameEn, int sortOrder = 0) =>
        new()
        {
            Id = Guid.NewGuid(),
            Code = code,
            ProvinceCode = provinceCode,
            NameVi = nameVi,
            NameEn = nameEn,
            IsActive = true,
            SortOrder = sortOrder
        };
}
