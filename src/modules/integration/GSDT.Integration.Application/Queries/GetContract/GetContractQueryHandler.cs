using FluentResults;
using MediatR;

namespace GSDT.Integration.Application.Queries.GetContract;

public sealed class GetContractQueryHandler(IReadDbConnection db)
    : IRequestHandler<GetContractQuery, Result<ContractDto>>
{
    public async Task<Result<ContractDto>> Handle(
        GetContractQuery request, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT Id, TenantId, PartnerId, Title, Description,
                   EffectiveDate, ExpiryDate, Status, DataScopeJson,
                   CreatedAt, UpdatedAt
            FROM integration.Contracts
            WHERE Id = @Id AND TenantId = @TenantId AND IsDeleted = 0
            """;

        var row = await db.QueryFirstOrDefaultAsync<ContractDto>(
            sql, new { request.Id, request.TenantId }, cancellationToken);

        return row is null
            ? Result.Fail(new NotFoundError($"Contract {request.Id} not found."))
            : Result.Ok(row);
    }
}
