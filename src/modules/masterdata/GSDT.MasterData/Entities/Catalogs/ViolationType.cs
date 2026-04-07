namespace GSDT.MasterData.Entities.Catalogs;

/// <summary>Loại vi phạm (ViolationType) — seeded, system-wide read-only catalog.</summary>
public class ViolationType : Entity<Guid>
{
    public string Code { get; private set; } = default!;
    public string Name { get; private set; } = default!;
    public bool IsActive { get; private set; } = true;
    public int SortOrder { get; private set; }

    private ViolationType() { }
}
