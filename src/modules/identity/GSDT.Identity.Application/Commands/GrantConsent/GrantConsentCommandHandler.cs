using FluentResults;
using MediatR;

namespace GSDT.Identity.Application.Commands.GrantConsent;

public sealed class GrantConsentCommandHandler : IRequestHandler<GrantConsentCommand, Result<Guid>>
{
    private readonly IConsentRepository _consents;

    public GrantConsentCommandHandler(IConsentRepository consents) => _consents = consents;

    public async Task<Result<Guid>> Handle(GrantConsentCommand cmd, CancellationToken ct)
    {
        var record = new ConsentRecord
        {
            Id = Guid.NewGuid(),
            DataSubjectId = cmd.UserId,
            TenantId = cmd.TenantId,
            Purpose = cmd.PurposeCode,
            LegalBasis = cmd.LegalBasis,
            EvidenceJson = cmd.EvidenceJson,
            IsWithdrawn = false,
            CreatedAtUtc = DateTime.UtcNow
        };

        await _consents.AddAsync(record, ct);
        await _consents.SaveChangesAsync(ct);

        return Result.Ok(record.Id);
    }
}
