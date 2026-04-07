namespace GSDT.MasterData.Entities.Catalogs;

/// <summary>Loại hợp đồng (ContractForm) — seeded, system-wide read-only catalog.</summary>
public class ContractForm : Entity<Guid>
{
    public string Code { get; private set; } = default!;
    public string Name { get; private set; } = default!;
    public bool IsActive { get; private set; } = true;
    public int SortOrder { get; private set; }

    private ContractForm() { }
}
