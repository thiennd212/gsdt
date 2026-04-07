using Dapper;

namespace GSDT.Organization.Infrastructure.Clients;

/// <summary>
/// Monolith in-process adapter for IOrgUnitModuleClient.
/// Direct Dapper query against organization.OrgUnits.
/// Swap to HttpOrgUnitModuleClient when module extracted (Phase 2+).
/// </summary>
public sealed class InProcessOrgUnitModuleClient(IReadDbConnection db) : IOrgUnitModuleClient
{
    private const int ChunkSize = 500;

    /// <summary>
    /// Batch lookup org unit info by IDs with chunking.
    /// [RT-02] Optional tenant filter. [RT-09] Hard cap 5000 IDs.
    /// </summary>
    public async Task<IReadOnlyDictionary<Guid, OrgUnitInfoDto>> GetOrgUnitsByIdsAsync(
        IEnumerable<Guid> ids, Guid? tenantId = null, CancellationToken ct = default)
    {
        var idList = ids?.Distinct().ToList();
        if (idList is null || idList.Count == 0)
            return new Dictionary<Guid, OrgUnitInfoDto>();

        if (idList.Count > IOrgUnitModuleClient.MaxBatchSize)
            throw new ArgumentException(
                $"Batch size {idList.Count} exceeds maximum {IOrgUnitModuleClient.MaxBatchSize}.",
                nameof(ids));

        var sql = """
            SELECT Id, Name, ParentId
            FROM organization.OrgUnits
            WHERE Id IN @Ids AND IsDeleted = 0
            """;

        if (tenantId.HasValue)
            sql += " AND TenantId = @TenantId";

        var result = new Dictionary<Guid, OrgUnitInfoDto>(idList.Count);

        foreach (var chunk in idList.Chunk(ChunkSize))
        {
            var parameters = new DynamicParameters();
            parameters.Add("Ids", chunk);
            if (tenantId.HasValue)
                parameters.Add("TenantId", tenantId.Value);

            // OrgUnitInfoDto is a class (not record) — safe for Dapper [RT-12]
            var rows = await db.QueryAsync<OrgUnitInfoDto>(sql, parameters, ct);
            foreach (var row in rows)
                result[row.Id] = row;
        }

        return result;
    }
}
