
namespace GSDT.Integration.Domain.Entities;

/// <summary>
/// Integration contract — defines data scope and validity period for a partner relationship.
/// State transitions: Draft → Active → Expired | Terminated.
/// </summary>
public sealed class Contract : AuditableEntity<Guid>, ITenantScoped
{
    public Guid TenantId { get; private set; }
    public Guid PartnerId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public DateTime EffectiveDate { get; private set; }
    public DateTime? ExpiryDate { get; private set; }
    public ContractStatus Status { get; private set; } = ContractStatus.Draft;
    public string? DataScopeJson { get; private set; }

    private Contract() { } // EF Core

    public static Contract Create(Guid tenantId, Guid partnerId, string title, string? description,
        DateTime effectiveDate, DateTime? expiryDate, string? dataScopeJson, Guid createdBy)
    {
        var contract = new Contract
        {
            Id = Guid.NewGuid(), TenantId = tenantId, PartnerId = partnerId,
            Title = title, Description = description,
            EffectiveDate = effectiveDate, ExpiryDate = expiryDate,
            DataScopeJson = dataScopeJson, Status = ContractStatus.Draft
        };
        contract.SetAuditCreate(createdBy);
        return contract;
    }

    public void Update(string title, string? description, DateTime effectiveDate,
        DateTime? expiryDate, string? dataScopeJson, Guid modifiedBy)
    {
        Title = title; Description = description;
        EffectiveDate = effectiveDate; ExpiryDate = expiryDate;
        DataScopeJson = dataScopeJson;
        SetAuditUpdate(modifiedBy);
    }

    public void Activate()
    {
        if (Status != ContractStatus.Draft)
            throw new InvalidOperationException($"Cannot activate contract in {Status} state.");
        Status = ContractStatus.Active;
    }

    public void Terminate()
    {
        if (Status is not (ContractStatus.Draft or ContractStatus.Active))
            throw new InvalidOperationException($"Cannot terminate contract in {Status} state.");
        Status = ContractStatus.Terminated;
    }

    public void MarkExpired()
    {
        if (Status != ContractStatus.Active)
            throw new InvalidOperationException($"Cannot expire contract in {Status} state.");
        Status = ContractStatus.Expired;
    }
}
