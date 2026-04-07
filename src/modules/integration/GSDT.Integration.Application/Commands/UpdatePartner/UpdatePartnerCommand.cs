
namespace GSDT.Integration.Application.Commands.UpdatePartner;

public sealed record UpdatePartnerCommand(
    Guid Id, string Name, string Code,
    string? ContactEmail, string? ContactPhone,
    string? Endpoint, string? AuthScheme) : ICommand<PartnerDto>;
