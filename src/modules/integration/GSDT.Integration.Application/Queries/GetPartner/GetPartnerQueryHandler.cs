using FluentResults;
using MediatR;

namespace GSDT.Integration.Application.Queries.GetPartner;

public sealed class GetPartnerQueryHandler(IReadDbConnection db)
    : IRequestHandler<GetPartnerQuery, Result<PartnerDto>>
{
    public async Task<Result<PartnerDto>> Handle(
        GetPartnerQuery request, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT Id, TenantId, Name, Code, ContactEmail, ContactPhone,
                   Endpoint, AuthScheme, Status, CreatedAt, UpdatedAt
            FROM integration.Partners
            WHERE Id = @Id AND TenantId = @TenantId AND IsDeleted = 0
            """;

        var row = await db.QueryFirstOrDefaultAsync<PartnerDto>(
            sql, new { request.Id, request.TenantId }, cancellationToken);

        return row is null
            ? Result.Fail(new NotFoundError($"Partner {request.Id} not found."))
            : Result.Ok(row);
    }
}
