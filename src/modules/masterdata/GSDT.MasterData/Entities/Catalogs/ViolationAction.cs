namespace GSDT.MasterData.Entities.Catalogs;

/// <summary>Hành động xử lý vi phạm (ViolationAction) — seeded, system-wide read-only catalog.</summary>
public class ViolationAction : Entity<Guid>
{
    public string Code { get; private set; } = default!;
    public string Name { get; private set; } = default!;
    public bool IsActive { get; private set; } = true;
    public int SortOrder { get; private set; }

    private ViolationAction() { }
}
