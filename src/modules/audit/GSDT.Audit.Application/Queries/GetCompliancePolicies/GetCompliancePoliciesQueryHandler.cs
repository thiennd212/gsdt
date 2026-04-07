using Dapper;
using FluentResults;
using MediatR;

namespace GSDT.Audit.Application.Queries.GetCompliancePolicies;

public sealed class GetCompliancePoliciesQueryHandler(IReadDbConnection db)
    : IRequestHandler<GetCompliancePoliciesQuery, Result<IReadOnlyList<CompliancePolicyDto>>>
{
    public async Task<Result<IReadOnlyList<CompliancePolicyDto>>> Handle(
        GetCompliancePoliciesQuery request,
        CancellationToken cancellationToken)
    {
        var sql = request.IncludeDisabled
            ? "SELECT Id, Name, Category, Rules, Enforcement, IsEnabled, CreatedAt FROM audit.CompliancePolicies ORDER BY Name"
            : "SELECT Id, Name, Category, Rules, Enforcement, IsEnabled, CreatedAt FROM audit.CompliancePolicies WHERE IsEnabled = 1 ORDER BY Name";

        var rows = await db.QueryAsync<CompliancePolicyDto>(sql, null, cancellationToken);
        return Result.Ok<IReadOnlyList<CompliancePolicyDto>>(rows.ToList());
    }
}
