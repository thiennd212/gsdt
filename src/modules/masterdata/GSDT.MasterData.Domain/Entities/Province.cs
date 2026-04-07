
namespace GSDT.MasterData.Domain.Entities;

/// <summary>Province (Tỉnh/Thành phố) — loaded into IMemoryCache NeverRemove at startup.</summary>
public class Province : Entity<Guid>
{
    public string Code { get; private set; } = default!;
    public string NameVi { get; private set; } = default!;
    public string NameEn { get; private set; } = default!;
    public bool IsActive { get; private set; }
    public int SortOrder { get; private set; }

    private Province() { }

    public static Province Create(string code, string nameVi, string nameEn, int sortOrder = 0) =>
        new()
        {
            Id = Guid.NewGuid(),
            Code = code,
            NameVi = nameVi,
            NameEn = nameEn,
            IsActive = true,
            SortOrder = sortOrder
        };
}
