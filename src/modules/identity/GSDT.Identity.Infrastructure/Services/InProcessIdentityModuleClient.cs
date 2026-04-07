using Dapper;

namespace GSDT.Identity.Infrastructure.Services;

/// <summary>
/// Monolith in-process adapter for IIdentityModuleClient.
/// All queries use Dapper for performance and consistent behavior.
/// Swap to GrpcIdentityModuleClient when module is extracted (Phase 2+).
/// </summary>
public sealed class InProcessIdentityModuleClient(IReadDbConnection db) : IIdentityModuleClient
{
    private const int ChunkSize = 500;

    /// <summary>
    /// [RT-05/VAL-01] Find user by email, optionally scoped to tenant.
    /// Returns UserInfoDto with full user info (not just UserId/TenantId).
    /// </summary>
    public async Task<UserInfoDto?> FindByEmailAsync(string email, Guid? tenantId = null, CancellationToken ct = default)
    {
        var sql = """
            SELECT TOP 1 Id AS UserId, Email, FullName, TenantId, PrimaryOrgUnitId
            FROM [identity].AspNetUsers
            WHERE Email = @Email
            """;

        var parameters = new DynamicParameters();
        parameters.Add("Email", email);

        if (tenantId.HasValue)
        {
            sql += " AND TenantId = @TenantId";
            parameters.Add("TenantId", tenantId.Value);
        }

        return await db.QueryFirstOrDefaultAsync<UserInfoDto>(sql, parameters, ct);
    }

    /// <summary>
    /// Batch lookup user info by IDs with chunking (SQL Server IN clause limit ~2100 params).
    /// [RT-02] Optional tenant filter. [RT-09] Hard cap 5000 IDs.
    /// </summary>
    public async Task<IReadOnlyDictionary<Guid, UserInfoDto>> GetUserInfoByIdsAsync(
        IEnumerable<Guid> userIds, Guid? tenantId = null, CancellationToken ct = default)
    {
        var idList = userIds?.Distinct().ToList();
        if (idList is null || idList.Count == 0)
            return new Dictionary<Guid, UserInfoDto>();

        if (idList.Count > IIdentityModuleClient.MaxBatchSize)
            throw new ArgumentException(
                $"Batch size {idList.Count} exceeds maximum {IIdentityModuleClient.MaxBatchSize}.",
                nameof(userIds));

        var sql = """
            SELECT Id AS UserId, Email, FullName, TenantId, PrimaryOrgUnitId
            FROM [identity].AspNetUsers
            WHERE Id IN @Ids
            """;

        if (tenantId.HasValue)
            sql += " AND TenantId = @TenantId";

        var result = new Dictionary<Guid, UserInfoDto>(idList.Count);

        foreach (var chunk in idList.Chunk(ChunkSize))
        {
            var parameters = new DynamicParameters();
            parameters.Add("Ids", chunk);
            if (tenantId.HasValue)
                parameters.Add("TenantId", tenantId.Value);

            // UserInfoDto is a class (not record) — safe for Dapper materialization [RT-12]
            var rows = await db.QueryAsync<UserInfoDto>(sql, parameters, ct);
            foreach (var row in rows)
                result[row.UserId] = row;
        }

        return result;
    }
}
