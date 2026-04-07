
namespace GSDT.Integration.Application.Queries.GetContract;

public sealed record GetContractQuery(Guid Id, Guid TenantId) : IQuery<ContractDto>;
