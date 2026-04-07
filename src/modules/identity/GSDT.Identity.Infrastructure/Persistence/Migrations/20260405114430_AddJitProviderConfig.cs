using System;

#nullable disable

namespace GSDT.Identity.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddJitProviderConfig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "JitProviderConfigs",
                schema: "identity",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Scheme = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ProviderType = table.Column<int>(type: "int", nullable: false),
                    JitEnabled = table.Column<bool>(type: "bit", nullable: false),
                    DefaultRoleName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    RequireApproval = table.Column<bool>(type: "bit", nullable: false),
                    ClaimMappingJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DefaultTenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    AllowedDomainsJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MaxProvisionsPerHour = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ClassificationLevel = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JitProviderConfigs", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_JitProviderConfig_Scheme",
                schema: "identity",
                table: "JitProviderConfigs",
                column: "Scheme",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "JitProviderConfigs",
                schema: "identity");
        }
    }
}
