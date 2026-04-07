using System.Text;
using Dapper;
using FluentResults;
using MediatR;

namespace GSDT.Audit.Application.Queries.ExportAuditLogs;

/// <summary>
/// Builds a CSV (UTF-8 BOM) from up to 10,000 audit log rows.
/// Uses the same Dapper read-side pattern as GetAuditLogsQueryHandler.
/// No string interpolation for user inputs — all via DynamicParameters.
/// </summary>
public sealed class ExportAuditLogsCsvQueryHandler(IReadDbConnection db)
    : IRequestHandler<ExportAuditLogsCsvQuery, Result<byte[]>>
{
    private const int MaxRows = 10_000;

    // CSV header columns — order must match WriteRow()
    private static readonly string CsvHeader =
        "Timestamp,Action,ResourceType,ResourceId,UserId,IpAddress,Details";

    public async Task<Result<byte[]>> Handle(
        ExportAuditLogsCsvQuery request,
        CancellationToken cancellationToken)
    {
        var p = new DynamicParameters();
        var where = BuildWhere(request, p);
        p.Add("MaxRows", MaxRows);

        var sql = $"""
            SELECT TOP (@MaxRows)
                   OccurredAt, Action, ResourceType, ResourceId,
                   UserId, IpAddress, CorrelationId
            FROM audit.AuditLogEntries
            {where}
            ORDER BY OccurredAt DESC
            """;

        var rows = await db.QueryAsync<ExportRow>(sql, p, cancellationToken);

        var csv = BuildCsv(rows);
        return Result.Ok(csv);
    }

    // Build CSV bytes with UTF-8 BOM so Excel opens correctly
    private static byte[] BuildCsv(IEnumerable<ExportRow> rows)
    {
        var sb = new StringBuilder();
        sb.AppendLine(CsvHeader);

        foreach (var r in rows)
        {
            sb.Append(EscapeCell(r.OccurredAt.ToString("o"))); sb.Append(',');
            sb.Append(EscapeCell(r.Action));                    sb.Append(',');
            sb.Append(EscapeCell(r.ResourceType));              sb.Append(',');
            sb.Append(EscapeCell(r.ResourceId));                sb.Append(',');
            sb.Append(EscapeCell(r.UserId?.ToString()));        sb.Append(',');
            sb.Append(EscapeCell(r.IpAddress));                 sb.Append(',');
            sb.AppendLine(EscapeCell(r.CorrelationId));
        }

        // UTF-8 BOM (EF BB BF) + content
        var bom = Encoding.UTF8.GetPreamble();
        var body = Encoding.UTF8.GetBytes(sb.ToString());
        var result = new byte[bom.Length + body.Length];
        bom.CopyTo(result, 0);
        body.CopyTo(result, bom.Length);
        return result;
    }

    // RFC 4180: wrap in double-quotes if cell contains comma, quote, or newline
    private static string EscapeCell(string? value)
    {
        if (string.IsNullOrEmpty(value)) return string.Empty;
        if (value.Contains(',') || value.Contains('"') || value.Contains('\n') || value.Contains('\r'))
            return $"\"{value.Replace("\"", "\"\"")}\"";
        return value;
    }

    private static string BuildWhere(ExportAuditLogsCsvQuery q, DynamicParameters p)
    {
        var clauses = new List<string>();

        if (q.TenantId.HasValue)                    { clauses.Add("TenantId = @TenantId");       p.Add("TenantId", q.TenantId); }
        if (q.UserId.HasValue)                      { clauses.Add("UserId = @UserId");             p.Add("UserId", q.UserId); }
        if (q.From.HasValue)                        { clauses.Add("OccurredAt >= @From");          p.Add("From", q.From); }
        if (q.To.HasValue)                          { clauses.Add("OccurredAt <= @To");            p.Add("To", q.To); }
        if (!string.IsNullOrEmpty(q.Action))        { clauses.Add("Action = @Action");             p.Add("Action", q.Action); }
        if (!string.IsNullOrEmpty(q.ModuleName))    { clauses.Add("ModuleName = @ModuleName");     p.Add("ModuleName", q.ModuleName); }
        if (!string.IsNullOrEmpty(q.ResourceType))  { clauses.Add("ResourceType = @ResourceType"); p.Add("ResourceType", q.ResourceType); }
        if (!string.IsNullOrEmpty(q.ResourceId))    { clauses.Add("ResourceId = @ResourceId");     p.Add("ResourceId", q.ResourceId); }

        return clauses.Count == 0 ? string.Empty : "WHERE " + string.Join(" AND ", clauses);
    }

    // Lightweight projection — only fields needed for CSV
    private sealed record ExportRow(
        DateTimeOffset OccurredAt,
        string Action,
        string ResourceType,
        string? ResourceId,
        Guid? UserId,
        string? IpAddress,
        string? CorrelationId);
}
