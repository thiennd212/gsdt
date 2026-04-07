using FluentResults;
using MediatR;

namespace GSDT.Integration.Application.Commands.DeletePartner;

public sealed class DeletePartnerCommandHandler(IPartnerRepository repository)
    : IRequestHandler<DeletePartnerCommand, Result>
{
    public async Task<Result> Handle(
        DeletePartnerCommand request, CancellationToken cancellationToken)
    {
        var partner = await repository.GetByIdAsync(request.Id, cancellationToken);
        if (partner is null)
            return Result.Fail(new NotFoundError($"Partner {request.Id} not found."));

        partner.Delete();
        await repository.UpdateAsync(partner, cancellationToken);
        return Result.Ok();
    }
}
