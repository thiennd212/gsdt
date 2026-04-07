
namespace GSDT.Identity.Application.Queries.GetCredentialPolicyById;

public sealed record GetCredentialPolicyByIdQuery(Guid Id) : IQuery<CredentialPolicyDto>;
