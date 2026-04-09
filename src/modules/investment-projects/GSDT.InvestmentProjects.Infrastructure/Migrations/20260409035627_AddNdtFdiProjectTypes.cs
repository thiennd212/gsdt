using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GSDT.InvestmentProjects.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddNdtFdiProjectTypes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FdiProjects",
                schema: "investment",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SubProjectType = table.Column<int>(type: "int", nullable: false),
                    ProjectGroupId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StatusId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CompetentAuthorityId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    InvestorName = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    StateOwnershipRatio = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    Objective = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    PrelimTotalInvestment = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    PrelimEquityCapital = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    PrelimOdaLoanCapital = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    PrelimCreditLoanCapital = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    AreaHectares = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    Capacity = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    MainItems = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    ImplementationTimeline = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ProgressDescription = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    StopContent = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    StopDecisionNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    StopDecisionDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    StopFileId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FdiProjects", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FdiProjects_InvestmentProjects_Id",
                        column: x => x.Id,
                        principalSchema: "investment",
                        principalTable: "InvestmentProjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NdtProjects",
                schema: "investment",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SubProjectType = table.Column<int>(type: "int", nullable: false),
                    ProjectGroupId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StatusId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CompetentAuthorityId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    InvestorName = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    StateOwnershipRatio = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    Objective = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    PrelimTotalInvestment = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    PrelimEquityCapital = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    PrelimOdaLoanCapital = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    PrelimCreditLoanCapital = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    AreaHectares = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    Capacity = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    MainItems = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    ImplementationTimeline = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ProgressDescription = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    StopContent = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    StopDecisionNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    StopDecisionDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    StopFileId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NdtProjects", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NdtProjects_InvestmentProjects_Id",
                        column: x => x.Id,
                        principalSchema: "investment",
                        principalTable: "InvestmentProjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FdiInvestmentDecisions",
                schema: "investment",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DecisionType = table.Column<int>(type: "int", nullable: false),
                    DecisionNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DecisionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DecisionAuthority = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    DecisionPerson = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    TotalInvestment = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    EquityCapital = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    OdaLoanCapital = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    CreditLoanCapital = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    EquityRatio = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    AdjustmentContentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    FileId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ClassificationLevel = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FdiInvestmentDecisions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FdiInvestmentDecisions_FdiProjects_ProjectId",
                        column: x => x.ProjectId,
                        principalSchema: "investment",
                        principalTable: "FdiProjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NdtInvestmentDecisions",
                schema: "investment",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DecisionType = table.Column<int>(type: "int", nullable: false),
                    DecisionNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DecisionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DecisionAuthority = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    DecisionPerson = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    TotalInvestment = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    EquityCapital = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    OdaLoanCapital = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    CreditLoanCapital = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    EquityRatio = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    AdjustmentContentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    FileId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ClassificationLevel = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NdtInvestmentDecisions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NdtInvestmentDecisions_NdtProjects_ProjectId",
                        column: x => x.ProjectId,
                        principalSchema: "investment",
                        principalTable: "NdtProjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FdiInvestmentDecisions_ProjectId",
                schema: "investment",
                table: "FdiInvestmentDecisions",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_NdtInvestmentDecisions_ProjectId",
                schema: "investment",
                table: "NdtInvestmentDecisions",
                column: "ProjectId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FdiInvestmentDecisions",
                schema: "investment");

            migrationBuilder.DropTable(
                name: "NdtInvestmentDecisions",
                schema: "investment");

            migrationBuilder.DropTable(
                name: "FdiProjects",
                schema: "investment");

            migrationBuilder.DropTable(
                name: "NdtProjects",
                schema: "investment");
        }
    }
}
