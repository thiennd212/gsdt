
namespace GSDT.Identity.Application.Queries.ListCredentialPolicies;

public sealed record ListCredentialPoliciesQuery(
    Guid TenantId,
    int Page = 1,
    int PageSize = 20) : IQuery<PagedResult<CredentialPolicyDto>>;
