
namespace GSDT.Identity.Infrastructure.Services;

/// <summary>
/// In-process implementation of IConsentModuleClient.
/// Directly creates ConsentRecord via repository — no MediatR cross-module hop.
/// </summary>
public sealed class InProcessConsentModuleClient(IConsentRepository consents)
    : IConsentModuleClient
{
    public async Task<Guid> RecordConsentAsync(RecordConsentRequest request, CancellationToken ct)
    {
        var record = new ConsentRecord
        {
            Id = Guid.NewGuid(),
            DataSubjectId = request.DataSubjectId,
            TenantId = request.TenantId,
            Purpose = request.PurposeCode,
            LegalBasis = request.LegalBasis,
            DataSubjectType = request.DataSubjectType,
            EvidenceJson = request.EvidenceJson,
            IsWithdrawn = false,
            CreatedAtUtc = DateTime.UtcNow
        };

        await consents.AddAsync(record, ct);
        await consents.SaveChangesAsync(ct);

        return record.Id;
    }
}
