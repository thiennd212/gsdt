namespace GSDT.InvestmentProjects.Domain.Entities;

/// <summary>Document attachment for an investment project — shared by domestic and ODA.</summary>
public sealed class ProjectDocument : AuditableEntity<Guid>, ITenantScoped
{
    public Guid TenantId { get; set; }
    public Guid ProjectId { get; set; }

    /// <summary>Guid ref to MasterData: document type classification.</summary>
    public Guid DocumentTypeId { get; set; }

    /// <summary>Guid ref to Files module: the stored file.</summary>
    public Guid FileId { get; set; }

    public string Title { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; }
    public string? Notes { get; set; }

    // Navigation
    public InvestmentProject Project { get; set; } = default!;

    private ProjectDocument() { } // EF Core

    /// <summary>Factory method — use instead of object initializer (Id is protected set).</summary>
    public static ProjectDocument Create(
        Guid tenantId, Guid projectId,
        Guid documentTypeId, Guid fileId,
        string title, string? notes = null)
        => new()
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            ProjectId = projectId,
            DocumentTypeId = documentTypeId,
            FileId = fileId,
            Title = title,
            UploadedAt = DateTime.UtcNow,
            Notes = notes
        };
}
