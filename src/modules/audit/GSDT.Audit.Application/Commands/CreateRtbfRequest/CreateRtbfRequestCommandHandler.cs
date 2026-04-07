using FluentResults;
using MediatR;

namespace GSDT.Audit.Application.Commands.CreateRtbfRequest;

/// <summary>
/// Creates a new RTBF anonymization request — PDPL Art. 17.
/// Resolves data subject by email via IIdentityModuleClient, then persists RtbfRequest.
/// </summary>
public sealed class CreateRtbfRequestCommandHandler(
    IRtbfRequestRepository repo,
    IIdentityModuleClient identityClient) : IRequestHandler<CreateRtbfRequestCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateRtbfRequestCommand cmd, CancellationToken ct)
    {
        // Resolve data subject by email — tenant-scoped to prevent cross-tenant access [RT-05]
        var lookup = await identityClient.FindByEmailAsync(cmd.SubjectEmail, cmd.TenantId, ct);
        if (lookup is null)
            return Result.Fail(new ValidationError($"No user found with email '{cmd.SubjectEmail}'."));

        var rtbf = RtbfRequest.Create(
            tenantId: lookup.TenantId ?? cmd.TenantId,
            dataSubjectId: lookup.UserId,
            subjectEmail: lookup.Email ?? cmd.SubjectEmail); // denormalized for search

        await repo.AddAsync(rtbf, ct);
        await repo.SaveChangesAsync(ct);

        return Result.Ok(rtbf.Id);
    }
}
