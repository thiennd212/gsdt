using Dapper;
using FluentResults;
using MediatR;

namespace GSDT.Audit.Application.Queries.GetAuditStatistics;

public sealed class GetAuditStatisticsQueryHandler(IReadDbConnection db)
    : IRequestHandler<GetAuditStatisticsQuery, Result<AuditStatisticsDto>>
{
    public async Task<Result<AuditStatisticsDto>> Handle(
        GetAuditStatisticsQuery request,
        CancellationToken cancellationToken)
    {
        var p = new DynamicParameters();
        var tenantFilter = string.Empty;
        if (request.TenantId.HasValue)
        {
            tenantFilter = "WHERE TenantId = @TenantId";
            p.Add("TenantId", request.TenantId);
        }

        var countSql = $"""
            SELECT
                SUM(CASE WHEN OccurredAt >= CAST(GETUTCDATE() AS DATE) THEN 1 ELSE 0 END) AS TodayTotal,
                SUM(CASE WHEN OccurredAt >= DATEADD(day, -7, GETUTCDATE()) THEN 1 ELSE 0 END) AS WeekTotal,
                SUM(CASE WHEN OccurredAt >= DATEADD(day, -30, GETUTCDATE()) THEN 1 ELSE 0 END) AS MonthTotal
            FROM audit.AuditLogEntries {tenantFilter}
            """;

        var byActionSql = $"""
            SELECT Action, COUNT(*) AS Count
            FROM audit.AuditLogEntries {tenantFilter}
            WHERE OccurredAt >= DATEADD(day, -30, GETUTCDATE())
            GROUP BY Action ORDER BY Count DESC
            """;

        var byModuleSql = $"""
            SELECT ModuleName AS Module, COUNT(*) AS Count
            FROM audit.AuditLogEntries {tenantFilter}
            WHERE OccurredAt >= DATEADD(day, -30, GETUTCDATE())
            GROUP BY ModuleName ORDER BY Count DESC
            """;

        var counts = await db.QueryFirstOrDefaultAsync<(int TodayTotal, int WeekTotal, int MonthTotal)>(countSql, p, cancellationToken);
        var byAction = await db.QueryAsync<ActionSummary>(byActionSql, p, cancellationToken);
        var byModule = await db.QueryAsync<ModuleSummary>(byModuleSql, p, cancellationToken);

        var dto = new AuditStatisticsDto(
            counts.TodayTotal,
            counts.WeekTotal,
            counts.MonthTotal,
            byAction.ToList(),
            byModule.ToList());

        return Result.Ok(dto);
    }
}
