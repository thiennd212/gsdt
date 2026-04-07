
namespace GSDT.Integration.Domain.Entities;

/// <summary>
/// GOV partner aggregate root — tracks integration partner lifecycle.
/// State transitions: Active ↔ Suspended → Deactivated.
/// </summary>
public sealed class Partner : AuditableEntity<Guid>, IAggregateRoot, ITenantScoped
{

    public Guid TenantId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Code { get; private set; } = string.Empty;
    public string? ContactEmail { get; private set; }
    public string? ContactPhone { get; private set; }
    public string? Endpoint { get; private set; }
    public string? AuthScheme { get; private set; }
    public PartnerStatus Status { get; private set; } = PartnerStatus.Active;


    private Partner() { } // EF Core

    public static Partner Create(Guid tenantId, string name, string code, Guid createdBy,
        string? contactEmail = null, string? contactPhone = null,
        string? endpoint = null, string? authScheme = null)
    {
        var partner = new Partner
        {
            Id = Guid.NewGuid(), TenantId = tenantId,
            Name = name, Code = code,
            ContactEmail = contactEmail, ContactPhone = contactPhone,
            Endpoint = endpoint, AuthScheme = authScheme,
            Status = PartnerStatus.Active
        };
        partner.SetAuditCreate(createdBy);
        partner.AddDomainEvent(new PartnerCreatedEvent(partner.Id, tenantId));
        return partner;
    }

    public void Update(string name, string code, string? contactEmail, string? contactPhone,
        string? endpoint, string? authScheme, Guid modifiedBy)
    {
        Name = name; Code = code;
        ContactEmail = contactEmail; ContactPhone = contactPhone;
        Endpoint = endpoint; AuthScheme = authScheme;
        SetAuditUpdate(modifiedBy);
    }

    public void Suspend()
    {
        if (Status != PartnerStatus.Active)
            throw new InvalidOperationException($"Cannot suspend partner in {Status} state.");
        Status = PartnerStatus.Suspended;
    }

    public void Activate()
    {
        if (Status != PartnerStatus.Suspended)
            throw new InvalidOperationException($"Cannot activate partner in {Status} state.");
        Status = PartnerStatus.Active;
    }

    public void Deactivate()
    {
        if (Status == PartnerStatus.Deactivated)
            throw new InvalidOperationException("Partner already deactivated.");
        Status = PartnerStatus.Deactivated;
        AddDomainEvent(new PartnerDeactivatedEvent(Id, TenantId));
    }

    public void ClearDomainEvents() => _domainEvents.Clear();
}
