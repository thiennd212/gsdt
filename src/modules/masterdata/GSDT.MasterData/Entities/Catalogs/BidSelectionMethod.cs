namespace GSDT.MasterData.Entities.Catalogs;

/// <summary>Phương thức lựa chọn nhà thầu (BidSelectionMethod) — seeded, system-wide read-only catalog.</summary>
public class BidSelectionMethod : Entity<Guid>
{
    public string Code { get; private set; } = default!;
    public string Name { get; private set; } = default!;
    public bool IsActive { get; private set; } = true;
    public int SortOrder { get; private set; }

    private BidSelectionMethod() { }
}
