
#nullable disable

namespace GSDT.Audit.Infrastructure.Persistence.Migrations;

/// <summary>
/// Deploy SQL Server RLS policies for tenant isolation on the audit schema.
/// AuditLogEntries uses filter predicate only (append-only table — db_owner bypasses via IS_MEMBER check).
/// RtbfRequests and AiPromptTraces have TenantId and get full filter+block policies.
/// </summary>
public partial class AddRlsPolicies : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(RlsMigrationHelper.GenerateRlsPolicies(
            "audit",
            "AuditLogEntries",
            "SecurityEventLogs",
            "PersonalDataProcessingLogs",
            "RtbfRequests",
            "AiPromptTraces"));
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(RlsMigrationHelper.DropRlsPolicy("audit", "AuditLogEntries"));
        migrationBuilder.Sql(RlsMigrationHelper.DropRlsPolicy("audit", "SecurityEventLogs"));
        migrationBuilder.Sql(RlsMigrationHelper.DropRlsPolicy("audit", "PersonalDataProcessingLogs"));
        migrationBuilder.Sql(RlsMigrationHelper.DropRlsPolicy("audit", "RtbfRequests"));
        migrationBuilder.Sql(RlsMigrationHelper.DropRlsPolicy("audit", "AiPromptTraces"));
    }
}
