using Dapper;
using FluentResults;
using MediatR;

namespace GSDT.Identity.Application.Queries.ListDelegations;

public sealed class ListDelegationsQueryHandler
    : IRequestHandler<ListDelegationsQuery, Result<IReadOnlyList<DelegationDto>>>
{
    private readonly IReadDbConnection _db;

    public ListDelegationsQueryHandler(IReadDbConnection db) => _db = db;

    public async Task<Result<IReadOnlyList<DelegationDto>>> Handle(
        ListDelegationsQuery query, CancellationToken ct)
    {
        // Build safe parameterized SQL — no string interpolation
        var conditions = new List<string> { "1=1" };
        var p = new DynamicParameters();

        if (query.DelegatorId.HasValue)
        {
            conditions.Add("d.DelegatorId = @DelegatorId");
            p.Add("DelegatorId", query.DelegatorId.Value);
        }

        if (query.DelegateId.HasValue)
        {
            conditions.Add("d.DelegateId = @DelegateId");
            p.Add("DelegateId", query.DelegateId.Value);
        }

        if (query.ActiveOnly == true)
        {
            conditions.Add("d.IsRevoked = 0 AND d.ValidFrom <= @Now AND d.ValidTo >= @Now");
            p.Add("Now", DateTime.UtcNow);
        }

        var sql = $"""
            SELECT d.Id, d.DelegatorId, d.DelegateId, d.ValidFrom, d.ValidTo,
                   d.Reason, d.IsRevoked, CAST(NULL AS datetime2) AS RevokedAt
            FROM [identity].UserDelegations d
            WHERE {string.Join(" AND ", conditions)}
            ORDER BY d.CreatedAtUtc DESC
            """;

        var rows = await _db.QueryAsync<DelegationDto>(sql, p, ct);
        return Result.Ok<IReadOnlyList<DelegationDto>>(rows.ToList());
    }
}
