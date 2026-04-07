
namespace GSDT.Integration.Application.Commands.CreateContract;

public sealed record CreateContractCommand(
    Guid TenantId, Guid PartnerId, string Title, string? Description,
    DateTime EffectiveDate, DateTime? ExpiryDate,
    string? DataScopeJson) : ICommand<ContractDto>;
