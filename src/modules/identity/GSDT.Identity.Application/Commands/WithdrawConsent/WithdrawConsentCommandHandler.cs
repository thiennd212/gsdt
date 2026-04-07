using FluentResults;
using MediatR;

namespace GSDT.Identity.Application.Commands.WithdrawConsent;

public sealed class WithdrawConsentCommandHandler : IRequestHandler<WithdrawConsentCommand, Result>
{
    private readonly IConsentRepository _consents;
    private readonly ISender _sender;

    public WithdrawConsentCommandHandler(IConsentRepository consents, ISender sender)
    {
        _consents = consents;
        _sender = sender;
    }

    public async Task<Result> Handle(WithdrawConsentCommand cmd, CancellationToken ct)
    {
        var record = await _consents.GetByIdAsync(cmd.ConsentId, ct);

        if (record is null)
            return Result.Fail(new NotFoundError($"Consent record {cmd.ConsentId} not found."));

        // Validate ownership — data subject can only withdraw their own consent
        if (record.DataSubjectId != cmd.UserId)
            return Result.Fail(new ForbiddenError("You can only withdraw your own consent records."));

        if (record.IsWithdrawn)
            return Result.Fail(new ConflictError($"Consent record {cmd.ConsentId} is already withdrawn."));

        record.IsWithdrawn = true;
        record.WithdrawnAt = DateTime.UtcNow;
        // TODO: add WithdrawReason string? column to ConsentRecord in next migration.

        await _consents.SaveChangesAsync(ct);

        // Security (F-18): revoke active tokens for the data subject so ongoing sessions end immediately
        await _sender.Send(
            new RevokeTokenCommand(TokenId: null, UserId: record.DataSubjectId, ActorId: cmd.UserId), ct);

        return Result.Ok();
    }
}
