namespace GSDT.Infrastructure.Persistence;

/// <summary>
/// Static helper for generating SQL Server Row-Level Security (RLS) DDL.
///
/// Usage pattern in EF migrations:
///   migrationBuilder.Sql(RlsMigrationHelper.GenerateRlsPolicy("cases", "Cases"));
///   migrationBuilder.Sql(RlsMigrationHelper.DropRlsPolicy("cases", "Cases")); // Down()
///
/// How it works:
///   1. fn_TenantFilter reads SESSION_CONTEXT(N'TenantId') set by TenantSessionContextInterceptor.
///   2. Security policy filters SELECT/INSERT — blocks cross-tenant reads at DB engine level.
///   3. Works alongside EF global query filters (defense in depth).
///
/// Requirements:
///   - sp_set_session_context must be called before any query (done by TenantSessionContextInterceptor).
///   - Column must be named 'TenantId' of type uniqueidentifier (EF default Guid mapping).
///   - Admin operations that need cross-tenant access must connect via a db_owner login
///     that bypasses RLS, or call DROP SECURITY POLICY temporarily (not recommended in prod).
/// </summary>
public static class RlsMigrationHelper
{
    /// <summary>
    /// Generates the full RLS setup SQL for one table:
    ///   1. Predicate function fn_{schema}_{table}_TenantFilter
    ///   2. Security policy pol_{schema}_{table}_Tenant
    /// </summary>
    /// <param name="schemaName">DB schema (e.g. "cases", "files", "audit").</param>
    /// <param name="tableName">Table name without schema (e.g. "Cases", "FileRecords").</param>
    public static string GenerateRlsPolicy(string schemaName, string tableName)
    {
        ValidateIdentifier(schemaName);
        ValidateIdentifier(tableName);

        var fnName = $"fn_{schemaName}_{tableName}_TenantFilter";
        var policyName = $"pol_{schemaName}_{tableName}_Tenant";

        return $"""
            -- ── RLS for [{schemaName}].[{tableName}] ──────────────────────────────────────
            -- Predicate function: returns 1 when row's TenantId matches session context
            CREATE OR ALTER FUNCTION [{schemaName}].[{fnName}](@TenantId uniqueidentifier)
            RETURNS TABLE
            WITH SCHEMABINDING
            AS
            RETURN
            (
                SELECT 1 AS fn_result
                WHERE
                    -- Allow db_owner / sa (migration tooling, admin jobs)
                    IS_MEMBER('db_owner') = 1
                    OR
                    -- Match session context set by TenantSessionContextInterceptor
                    @TenantId = TRY_CAST(SESSION_CONTEXT(N'TenantId') AS uniqueidentifier)
            );
            GO

            -- Security policy: filter predicate (SELECT) + block predicate (INSERT)
            CREATE SECURITY POLICY [{schemaName}].[{policyName}]
            ADD FILTER PREDICATE [{schemaName}].[{fnName}](TenantId)
                ON [{schemaName}].[{tableName}],
            ADD BLOCK PREDICATE [{schemaName}].[{fnName}](TenantId)
                ON [{schemaName}].[{tableName}] AFTER INSERT
            WITH (STATE = ON, SCHEMABINDING = ON);
            GO
            """;
    }

    /// <summary>
    /// Generates DROP statements to remove RLS from a table (use in migration Down()).
    /// </summary>
    public static string DropRlsPolicy(string schemaName, string tableName)
    {
        ValidateIdentifier(schemaName);
        ValidateIdentifier(tableName);

        var fnName = $"fn_{schemaName}_{tableName}_TenantFilter";
        var policyName = $"pol_{schemaName}_{tableName}_Tenant";

        return $"""
            -- ── Remove RLS for [{schemaName}].[{tableName}] ──────────────────────────────
            DROP SECURITY POLICY IF EXISTS [{schemaName}].[{policyName}];
            GO
            DROP FUNCTION IF EXISTS [{schemaName}].[{fnName}];
            GO
            """;
    }

    /// <summary>
    /// Generates RLS policies for multiple tables in the same schema in one migration block.
    /// </summary>
    public static string GenerateRlsPolicies(string schemaName, params string[] tableNames)
    {
        var parts = tableNames.Select(t => GenerateRlsPolicy(schemaName, t));
        return string.Join(Environment.NewLine + Environment.NewLine, parts);
    }

    // ── Validation ────────────────────────────────────────────────────────────

    private static void ValidateIdentifier(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Identifier must not be empty.");

        foreach (var c in value)
        {
            if (!char.IsLetterOrDigit(c) && c != '_')
                throw new ArgumentException(
                    $"Invalid identifier character '{c}' in '{value}'. Only letters, digits, underscores allowed.");
        }
    }
}
