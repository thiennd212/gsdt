using System;

#nullable disable

namespace GSDT.Audit.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Phase4_AddGovernance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AiOutputReviews",
                schema: "audit",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PromptTraceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReviewerId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Decision = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    ReviewedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ClassificationLevel = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AiOutputReviews", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AiPromptTraces",
                schema: "audit",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SessionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ModelProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ModelName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    PromptHash = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    PromptText = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ResponseHash = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    InputTokens = table.Column<int>(type: "int", nullable: false),
                    OutputTokens = table.Column<int>(type: "int", nullable: false),
                    LatencyMs = table.Column<int>(type: "int", nullable: false),
                    Cost = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    ClassificationLevel = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AiPromptTraces", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CompliancePolicies",
                schema: "audit",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Category = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    Rules = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Enforcement = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    IsEnabled = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ClassificationLevel = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompliancePolicies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CompliancePolicyEvaluations",
                schema: "audit",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PolicyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EntityType = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    EntityId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Result = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    Details = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EvaluatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ClassificationLevel = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompliancePolicyEvaluations", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AiOutputReviews_Decision",
                schema: "audit",
                table: "AiOutputReviews",
                column: "Decision");

            migrationBuilder.CreateIndex(
                name: "IX_AiOutputReviews_PromptTraceId",
                schema: "audit",
                table: "AiOutputReviews",
                column: "PromptTraceId");

            migrationBuilder.CreateIndex(
                name: "IX_AiPromptTraces_ModelName",
                schema: "audit",
                table: "AiPromptTraces",
                column: "ModelName");

            migrationBuilder.CreateIndex(
                name: "IX_AiPromptTraces_SessionId",
                schema: "audit",
                table: "AiPromptTraces",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_AiPromptTraces_TenantId_CreatedAt",
                schema: "audit",
                table: "AiPromptTraces",
                columns: new[] { "TenantId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_CompliancePolicies_Category",
                schema: "audit",
                table: "CompliancePolicies",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_CompliancePolicies_IsEnabled",
                schema: "audit",
                table: "CompliancePolicies",
                column: "IsEnabled");

            migrationBuilder.CreateIndex(
                name: "IX_CompliancePolicyEvaluations_EntityType_EntityId",
                schema: "audit",
                table: "CompliancePolicyEvaluations",
                columns: new[] { "EntityType", "EntityId" });

            migrationBuilder.CreateIndex(
                name: "IX_CompliancePolicyEvaluations_PolicyId_EvaluatedAt",
                schema: "audit",
                table: "CompliancePolicyEvaluations",
                columns: new[] { "PolicyId", "EvaluatedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AiOutputReviews",
                schema: "audit");

            migrationBuilder.DropTable(
                name: "AiPromptTraces",
                schema: "audit");

            migrationBuilder.DropTable(
                name: "CompliancePolicies",
                schema: "audit");

            migrationBuilder.DropTable(
                name: "CompliancePolicyEvaluations",
                schema: "audit");
        }
    }
}
