namespace GSDT.MasterData.Entities.Catalogs;

/// <summary>Hình thức lựa chọn nhà thầu (BidSelectionForm) — seeded, system-wide read-only catalog.</summary>
public class BidSelectionForm : Entity<Guid>
{
    public string Code { get; private set; } = default!;
    public string Name { get; private set; } = default!;
    public bool IsActive { get; private set; } = true;
    public int SortOrder { get; private set; }

    private BidSelectionForm() { }
}
