
#nullable disable

namespace GSDT.Files.Infrastructure.Persistence.Migrations;

/// <summary>Deploy SQL Server RLS policies for tenant isolation on the files schema.</summary>
public partial class AddRlsPolicies : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(RlsMigrationHelper.GenerateRlsPolicies(
            "files",
            "FileRecords",
            "FileVersions",
            "DocumentTemplates",
            "RetentionPolicies"));
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(RlsMigrationHelper.DropRlsPolicy("files", "FileRecords"));
        migrationBuilder.Sql(RlsMigrationHelper.DropRlsPolicy("files", "FileVersions"));
        migrationBuilder.Sql(RlsMigrationHelper.DropRlsPolicy("files", "DocumentTemplates"));
        migrationBuilder.Sql(RlsMigrationHelper.DropRlsPolicy("files", "RetentionPolicies"));
    }
}
