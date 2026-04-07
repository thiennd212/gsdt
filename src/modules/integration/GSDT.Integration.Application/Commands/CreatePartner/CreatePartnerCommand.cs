
namespace GSDT.Integration.Application.Commands.CreatePartner;

public sealed record CreatePartnerCommand(
    Guid TenantId, string Name, string Code,
    string? ContactEmail, string? ContactPhone,
    string? Endpoint, string? AuthScheme) : ICommand<PartnerDto>;
