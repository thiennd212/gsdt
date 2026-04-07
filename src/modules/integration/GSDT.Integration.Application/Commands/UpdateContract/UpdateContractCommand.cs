
namespace GSDT.Integration.Application.Commands.UpdateContract;

public sealed record UpdateContractCommand(
    Guid Id, string Title, string? Description,
    DateTime EffectiveDate, DateTime? ExpiryDate,
    string? DataScopeJson) : ICommand<ContractDto>;
