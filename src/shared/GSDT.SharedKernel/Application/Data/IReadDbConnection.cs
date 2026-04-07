namespace GSDT.SharedKernel.Application.Data;

/// <summary>
/// Read-side Dapper abstraction for query handlers.
/// Query handlers use this directly — bypass IRepository entirely.
/// Backed by a read replica connection in production.
/// IMPORTANT: All SQL must use @param DynamicParameters (no string interpolation) to prevent SQLi.
/// </summary>
public interface IReadDbConnection
{
    // E-03: commandTimeout added — callers can set per-query limits (report SQL defaults to 60s)
    Task<IEnumerable<T>> QueryAsync<T>(string sql, object? param = null, CancellationToken cancellationToken = default, int? commandTimeout = null);
    Task<T?> QueryFirstOrDefaultAsync<T>(string sql, object? param = null, CancellationToken cancellationToken = default, int? commandTimeout = null);
    Task<T> QuerySingleAsync<T>(string sql, object? param = null, CancellationToken cancellationToken = default, int? commandTimeout = null);
    Task<int> ExecuteAsync(string sql, object? param = null, CancellationToken cancellationToken = default, int? commandTimeout = null);
}
