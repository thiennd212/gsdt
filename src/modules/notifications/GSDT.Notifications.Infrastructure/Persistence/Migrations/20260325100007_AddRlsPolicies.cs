
#nullable disable

namespace GSDT.Notifications.Infrastructure.Persistence.Migrations;

/// <summary>Deploy SQL Server RLS policies for tenant isolation on the notifications schema.</summary>
public partial class AddRlsPolicies : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(RlsMigrationHelper.GenerateRlsPolicies(
            "notifications",
            "Notifications",
            "NotificationPreferences",
            "NotificationTemplates"));
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(RlsMigrationHelper.DropRlsPolicy("notifications", "Notifications"));
        migrationBuilder.Sql(RlsMigrationHelper.DropRlsPolicy("notifications", "NotificationPreferences"));
        migrationBuilder.Sql(RlsMigrationHelper.DropRlsPolicy("notifications", "NotificationTemplates"));
    }
}
