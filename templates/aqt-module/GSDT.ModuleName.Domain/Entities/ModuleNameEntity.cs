using GSDT.SharedKernel.Domain;

namespace GSDT.ModuleName.Domain.Entities;

/// <summary>
/// Root aggregate entity for the ModuleName module.
/// Replace with your actual domain entity.
/// </summary>
public sealed class ModuleNameEntity : AuditableEntity
{
    public Guid TenantId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string? Description { get; private set; }

    private ModuleNameEntity() { } // EF constructor

    public static ModuleNameEntity Create(Guid tenantId, string title, string? description, Guid createdBy)
    {
        var entity = new ModuleNameEntity
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Title = title,
            Description = description,
        };
        entity.SetCreatedBy(createdBy);
        return entity;
    }

    public void Update(string title, string? description, Guid updatedBy)
    {
        Title = title;
        Description = description;
        SetUpdatedBy(updatedBy);
    }
}
