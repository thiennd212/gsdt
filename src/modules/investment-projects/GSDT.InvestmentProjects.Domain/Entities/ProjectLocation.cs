namespace GSDT.InvestmentProjects.Domain.Entities;

/// <summary>Geographic location(s) of an investment project — one project may span multiple locations.</summary>
public sealed class ProjectLocation : AuditableEntity<Guid>, ITenantScoped
{
    public Guid TenantId { get; set; }

    public Guid ProjectId { get; set; }

    /// <summary>Guid ref to MasterData: province.</summary>
    public Guid ProvinceId { get; set; }

    /// <summary>Guid ref to MasterData: district (optional).</summary>
    public Guid? DistrictId { get; set; }

    /// <summary>Guid ref to MasterData: ward (optional).</summary>
    public Guid? WardId { get; set; }

    public string? Address { get; set; }

    /// <summary>Optional: industrial/economic zone name for KKT/KCN projects (max 500).</summary>
    public string? IndustrialZoneName { get; set; }

    // Navigation
    public InvestmentProject Project { get; set; } = default!;

    private ProjectLocation() { } // EF Core

    public static ProjectLocation Create(Guid tenantId, Guid projectId, Guid provinceId,
        Guid? districtId = null, Guid? wardId = null, string? address = null)
        => new()
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            ProjectId = projectId,
            ProvinceId = provinceId,
            DistrictId = districtId,
            WardId = wardId,
            Address = address
        };
}
