using FluentResults;
using MediatR;

namespace GSDT.Integration.Application.Commands.UpdatePartner;

public sealed class UpdatePartnerCommandHandler(
    IPartnerRepository repository,
    ICurrentUser currentUser)
    : IRequestHandler<UpdatePartnerCommand, Result<PartnerDto>>
{
    public async Task<Result<PartnerDto>> Handle(
        UpdatePartnerCommand request, CancellationToken cancellationToken)
    {
        var partner = await repository.GetByIdAsync(request.Id, cancellationToken);
        if (partner is null)
            return Result.Fail(new NotFoundError($"Partner {request.Id} not found."));

        partner.Update(request.Name, request.Code,
            request.ContactEmail, request.ContactPhone,
            request.Endpoint, request.AuthScheme, currentUser.UserId);

        await repository.UpdateAsync(partner, cancellationToken);

        return Result.Ok(new PartnerDto(
            partner.Id, partner.TenantId, partner.Name, partner.Code,
            partner.ContactEmail, partner.ContactPhone, partner.Endpoint, partner.AuthScheme,
            partner.Status.ToString(), partner.CreatedAt, partner.UpdatedAt));
    }
}
