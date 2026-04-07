namespace GSDT.MasterData.Entities.Catalogs;

/// <summary>Nhóm dự án (ProjectGroup) — seeded, system-wide read-only catalog.</summary>
public class ProjectGroup : Entity<Guid>
{
    public string Code { get; private set; } = default!;
    public string Name { get; private set; } = default!;
    public bool IsActive { get; private set; } = true;
    public int SortOrder { get; private set; }

    private ProjectGroup() { }
}
