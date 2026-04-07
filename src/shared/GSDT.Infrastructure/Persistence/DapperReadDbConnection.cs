using Dapper;

namespace GSDT.Infrastructure.Persistence;

/// <summary>
/// Dapper-backed read connection for query handlers.
/// Sets sp_set_session_context on every connection open to enable SQL Server RLS
/// as a safety net alongside explicit WHERE TenantId filters.
///
/// ADO.NET connection pool handles physical connection reuse; open/close per call is correct.
/// IMPORTANT: All SQL MUST use @param DynamicParameters — no string interpolation (SQLi prevention).
/// </summary>
public sealed class DapperReadDbConnection(
    IOptionsMonitor<DatabaseOptions> dbOptions,
    ITenantContext tenantContext) : IReadDbConnection
{
    /// <summary>Opens connection and sets SESSION_CONTEXT for RLS enforcement.</summary>
    private async Task<SqlConnection> OpenWithTenantAsync(CancellationToken ct)
    {
        var conn = new SqlConnection(dbOptions.CurrentValue.ConnectionString);
        try
        {
            await conn.OpenAsync(ct);
            if (tenantContext.TenantId.HasValue)
            {
                await using var cmd = conn.CreateCommand();
                cmd.CommandText = "EXEC sp_set_session_context N'TenantId', @tenantId";
                cmd.Parameters.Add(new SqlParameter("@tenantId", tenantContext.TenantId.Value.ToString()));
                await cmd.ExecuteNonQueryAsync(ct);
            }
            return conn;
        }
        catch
        {
            await conn.DisposeAsync();
            throw;
        }
    }

    public async Task<IEnumerable<T>> QueryAsync<T>(
        string sql, object? param = null, CancellationToken cancellationToken = default, int? commandTimeout = null)
    {
        await using var conn = await OpenWithTenantAsync(cancellationToken);
        return await conn.QueryAsync<T>(
            new CommandDefinition(sql, param, cancellationToken: cancellationToken, commandTimeout: commandTimeout));
    }

    public async Task<T?> QueryFirstOrDefaultAsync<T>(
        string sql, object? param = null, CancellationToken cancellationToken = default, int? commandTimeout = null)
    {
        await using var conn = await OpenWithTenantAsync(cancellationToken);
        return await conn.QueryFirstOrDefaultAsync<T>(
            new CommandDefinition(sql, param, cancellationToken: cancellationToken, commandTimeout: commandTimeout));
    }

    public async Task<T> QuerySingleAsync<T>(
        string sql, object? param = null, CancellationToken cancellationToken = default, int? commandTimeout = null)
    {
        await using var conn = await OpenWithTenantAsync(cancellationToken);
        return await conn.QuerySingleAsync<T>(
            new CommandDefinition(sql, param, cancellationToken: cancellationToken, commandTimeout: commandTimeout));
    }

    public async Task<int> ExecuteAsync(
        string sql, object? param = null, CancellationToken cancellationToken = default, int? commandTimeout = null)
    {
        await using var conn = await OpenWithTenantAsync(cancellationToken);
        return await conn.ExecuteAsync(
            new CommandDefinition(sql, param, cancellationToken: cancellationToken, commandTimeout: commandTimeout));
    }
}
