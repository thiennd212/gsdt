namespace GSDT.MasterData.Entities.Catalogs;

/// <summary>Nội dung điều chỉnh (AdjustmentContent) — seeded, system-wide read-only catalog.</summary>
public class AdjustmentContent : Entity<Guid>
{
    public string Code { get; private set; } = default!;
    public string Name { get; private set; } = default!;
    public bool IsActive { get; private set; } = true;
    public int SortOrder { get; private set; }

    private AdjustmentContent() { }
}
