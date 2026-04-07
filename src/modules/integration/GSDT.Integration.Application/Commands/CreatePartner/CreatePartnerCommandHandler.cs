using FluentResults;
using MediatR;

namespace GSDT.Integration.Application.Commands.CreatePartner;

public sealed class CreatePartnerCommandHandler(
    IPartnerRepository repository,
    ICurrentUser currentUser,
    IDomainEventPublisher eventPublisher)
    : IRequestHandler<CreatePartnerCommand, Result<PartnerDto>>
{
    public async Task<Result<PartnerDto>> Handle(
        CreatePartnerCommand request, CancellationToken cancellationToken)
    {
        var partner = Partner.Create(
            request.TenantId, request.Name, request.Code, currentUser.UserId,
            request.ContactEmail, request.ContactPhone,
            request.Endpoint, request.AuthScheme);

        await repository.AddAsync(partner, cancellationToken);
        await eventPublisher.PublishEventsAsync(partner.DomainEvents, cancellationToken);
        partner.ClearDomainEvents();

        return Result.Ok(MapToDto(partner));
    }

    private static PartnerDto MapToDto(Partner p) => new(
        p.Id, p.TenantId, p.Name, p.Code,
        p.ContactEmail, p.ContactPhone, p.Endpoint, p.AuthScheme,
        p.Status.ToString(), p.CreatedAt, p.UpdatedAt);
}
