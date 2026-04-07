using System;

#nullable disable

namespace GSDT.Files.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddDataClassificationToFileRecord : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "files");

            migrationBuilder.CreateTable(
                name: "DocumentTemplates",
                schema: "files",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    OutputFormat = table.Column<int>(type: "int", nullable: false),
                    TemplateContent = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ClassificationLevel = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentTemplates", x => x.Id);
                });

            // FileRecords and OutboxMessages already created in 20260314051944_InitialCreate.
            // ClassificationLevel was included in InitialCreate — no column add needed here.

            migrationBuilder.CreateTable(
                name: "FileVersions",
                schema: "files",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FileRecordId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VersionNumber = table.Column<int>(type: "int", nullable: false),
                    StoragePath = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: false),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    ContentHash = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    UploadedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UploadedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ClassificationLevel = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FileVersions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RecordLifecycles",
                schema: "files",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FileRecordId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CurrentStatus = table.Column<int>(type: "int", nullable: false),
                    RetentionPolicyId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ArchivedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ScheduledDestroyAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DestroyedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DestroyedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ClassificationLevel = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecordLifecycles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RetentionPolicies",
                schema: "files",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    DocumentType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    RetainDays = table.Column<int>(type: "int", nullable: false),
                    ArchiveAfterDays = table.Column<int>(type: "int", nullable: true),
                    DestroyAfterDays = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ClassificationLevel = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RetentionPolicies", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DocumentTemplate_IsDeleted",
                schema: "files",
                table: "DocumentTemplates",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentTemplate_TenantId_CreatedAt",
                schema: "files",
                table: "DocumentTemplates",
                columns: new[] { "TenantId", "CreatedAt" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_DocumentTemplate_TenantId_Id",
                schema: "files",
                table: "DocumentTemplates",
                columns: new[] { "TenantId", "Id" });

            migrationBuilder.CreateIndex(
                name: "IX_DocumentTemplates_TenantId_Status",
                schema: "files",
                table: "DocumentTemplates",
                columns: new[] { "TenantId", "Status" });

            migrationBuilder.CreateIndex(
                name: "UX_DocumentTemplates_TenantId_Code",
                schema: "files",
                table: "DocumentTemplates",
                columns: new[] { "TenantId", "Code" },
                unique: true);

            // IX_FileRecords_* indexes already created in 20260314051944_InitialCreate.

            migrationBuilder.CreateIndex(
                name: "IX_FileVersion_IsDeleted",
                schema: "files",
                table: "FileVersions",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_FileVersion_TenantId_CreatedAt",
                schema: "files",
                table: "FileVersions",
                columns: new[] { "TenantId", "CreatedAt" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_FileVersion_TenantId_Id",
                schema: "files",
                table: "FileVersions",
                columns: new[] { "TenantId", "Id" });

            migrationBuilder.CreateIndex(
                name: "IX_FileVersions_TenantId_ContentHash",
                schema: "files",
                table: "FileVersions",
                columns: new[] { "TenantId", "ContentHash" });

            migrationBuilder.CreateIndex(
                name: "UX_FileVersions_FileRecordId_VersionNumber",
                schema: "files",
                table: "FileVersions",
                columns: new[] { "FileRecordId", "VersionNumber" },
                unique: true);

            // IX_OutboxMessages_files_ProcessedAt already created in 20260314051944_InitialCreate.

            migrationBuilder.CreateIndex(
                name: "IX_RecordLifecycle_IsDeleted",
                schema: "files",
                table: "RecordLifecycles",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_RecordLifecycles_Status_ScheduledDestroyAt",
                schema: "files",
                table: "RecordLifecycles",
                columns: new[] { "CurrentStatus", "ScheduledDestroyAt" });

            migrationBuilder.CreateIndex(
                name: "UX_RecordLifecycles_FileRecordId",
                schema: "files",
                table: "RecordLifecycles",
                column: "FileRecordId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RetentionPolicies_TenantId_DocumentType_IsActive",
                schema: "files",
                table: "RetentionPolicies",
                columns: new[] { "TenantId", "DocumentType", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_RetentionPolicy_IsDeleted",
                schema: "files",
                table: "RetentionPolicies",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_RetentionPolicy_TenantId_CreatedAt",
                schema: "files",
                table: "RetentionPolicies",
                columns: new[] { "TenantId", "CreatedAt" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_RetentionPolicy_TenantId_Id",
                schema: "files",
                table: "RetentionPolicies",
                columns: new[] { "TenantId", "Id" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DocumentTemplates",
                schema: "files");

            // FileRecords and OutboxMessages are owned by 20260314051944_InitialCreate — do not drop here.

            migrationBuilder.DropTable(
                name: "FileVersions",
                schema: "files");

            migrationBuilder.DropTable(
                name: "RecordLifecycles",
                schema: "files");

            migrationBuilder.DropTable(
                name: "RetentionPolicies",
                schema: "files");
        }
    }
}
