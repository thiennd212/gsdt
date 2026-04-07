namespace GSDT.MasterData.Entities.Catalogs;

/// <summary>Lĩnh vực gói thầu (BidSectorType) — seeded, system-wide read-only catalog.</summary>
public class BidSectorType : Entity<Guid>
{
    public string Code { get; private set; } = default!;
    public string Name { get; private set; } = default!;
    public bool IsActive { get; private set; } = true;
    public int SortOrder { get; private set; }

    private BidSectorType() { }
}
