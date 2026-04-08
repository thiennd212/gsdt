using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GSDT.InvestmentProjects.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPppProjectType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "IndustrialZoneName",
                schema: "investment",
                table: "ProjectLocations",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "DesignEstimates",
                schema: "investment",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ApprovalDecisionNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ApprovalDecisionDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ApprovalAuthority = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ApprovalSigner = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ApprovalSummary = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    ApprovalFileId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    EquipmentCost = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    ConstructionCost = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    LandCompensationCost = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    ManagementCost = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    ConsultancyCost = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    ContingencyCost = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    OtherCost = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    TotalEstimate = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
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
                    table.PrimaryKey("PK_DesignEstimates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DesignEstimates_InvestmentProjects_ProjectId",
                        column: x => x.ProjectId,
                        principalSchema: "investment",
                        principalTable: "InvestmentProjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InvestorSelections",
                schema: "investment",
                columns: table => new
                {
                    ProjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SelectionMethod = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    SelectionDecisionNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    SelectionDecisionDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SelectionFileId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ClassificationLevel = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvestorSelections", x => x.ProjectId);
                    table.ForeignKey(
                        name: "FK_InvestorSelections_InvestmentProjects_ProjectId",
                        column: x => x.ProjectId,
                        principalSchema: "investment",
                        principalTable: "InvestmentProjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PppProjects",
                schema: "investment",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ContractType = table.Column<int>(type: "int", nullable: false),
                    SubProjectType = table.Column<int>(type: "int", nullable: false),
                    ProjectGroupId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StatusId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CompetentAuthorityId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PreparationUnit = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Objective = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    PrelimTotalInvestment = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    PrelimStateCapital = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    PrelimEquityCapital = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    PrelimLoanCapital = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    AreaHectares = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    Capacity = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    MainItems = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    StopContent = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    StopDecisionNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    StopDecisionDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    StopFileId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PppProjects", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PppProjects_InvestmentProjects_Id",
                        column: x => x.Id,
                        principalSchema: "investment",
                        principalTable: "InvestmentProjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DesignEstimateItems",
                schema: "investment",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DesignEstimateId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ItemName = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Scale = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Cost = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    FileId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ClassificationLevel = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DesignEstimateItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DesignEstimateItems_DesignEstimates_DesignEstimateId",
                        column: x => x.DesignEstimateId,
                        principalSchema: "investment",
                        principalTable: "DesignEstimates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InvestorSelectionInvestors",
                schema: "investment",
                columns: table => new
                {
                    InvestorSelectionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InvestorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvestorSelectionInvestors", x => new { x.InvestorSelectionId, x.InvestorId });
                    table.ForeignKey(
                        name: "FK_InvestorSelectionInvestors_InvestorSelections_InvestorSelectionId",
                        column: x => x.InvestorSelectionId,
                        principalSchema: "investment",
                        principalTable: "InvestorSelections",
                        principalColumn: "ProjectId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PppCapitalPlans",
                schema: "investment",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DecisionType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DecisionNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DecisionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    StateCapitalByDecision = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    FileId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
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
                    table.PrimaryKey("PK_PppCapitalPlans", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PppCapitalPlans_PppProjects_ProjectId",
                        column: x => x.ProjectId,
                        principalSchema: "investment",
                        principalTable: "PppProjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PppContractInfos",
                schema: "investment",
                columns: table => new
                {
                    ProjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TotalInvestment = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    StateCapital = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    CentralBudget = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    LocalBudget = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    OtherStateBudget = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    EquityCapital = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    LoanCapital = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    EquityRatio = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    ImplementationProgress = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ContractDuration = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    RevenueSharingMechanism = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    ContractAuthority = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ContractNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ContractDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ConstructionStartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CompletionDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ClassificationLevel = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PppContractInfos", x => x.ProjectId);
                    table.ForeignKey(
                        name: "FK_PppContractInfos_PppProjects_ProjectId",
                        column: x => x.ProjectId,
                        principalSchema: "investment",
                        principalTable: "PppProjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PppDisbursementRecords",
                schema: "investment",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReportDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    StateCapitalPeriod = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    StateCapitalCumulative = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    EquityCapitalPeriod = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    EquityCapitalCumulative = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    LoanCapitalPeriod = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    LoanCapitalCumulative = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
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
                    table.PrimaryKey("PK_PppDisbursementRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PppDisbursementRecords_PppProjects_ProjectId",
                        column: x => x.ProjectId,
                        principalSchema: "investment",
                        principalTable: "PppProjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PppExecutionRecords",
                schema: "investment",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReportDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ValueExecutedPeriod = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    ValueExecutedCumulative = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    CumulativeFromStart = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    SubProjectStateCapitalPeriod = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    SubProjectStateCapitalCumulative = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    BidPackageId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ContractId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
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
                    table.PrimaryKey("PK_PppExecutionRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PppExecutionRecords_PppProjects_ProjectId",
                        column: x => x.ProjectId,
                        principalSchema: "investment",
                        principalTable: "PppProjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PppInvestmentDecisions",
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
                    StateCapital = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    CentralBudget = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    LocalBudget = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    OtherStateBudget = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    EquityCapital = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    LoanCapital = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
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
                    table.PrimaryKey("PK_PppInvestmentDecisions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PppInvestmentDecisions_PppProjects_ProjectId",
                        column: x => x.ProjectId,
                        principalSchema: "investment",
                        principalTable: "PppProjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RevenueReports",
                schema: "investment",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReportYear = table.Column<int>(type: "int", nullable: false),
                    ReportPeriod = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    RevenuePeriod = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    RevenueCumulative = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    RevenueIncreaseSharing = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    RevenueDecreaseSharing = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    Difficulties = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Recommendations = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
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
                    table.PrimaryKey("PK_RevenueReports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RevenueReports_PppProjects_ProjectId",
                        column: x => x.ProjectId,
                        principalSchema: "investment",
                        principalTable: "PppProjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DesignEstimateItems_DesignEstimateId",
                schema: "investment",
                table: "DesignEstimateItems",
                column: "DesignEstimateId");

            migrationBuilder.CreateIndex(
                name: "IX_DesignEstimates_ProjectId",
                schema: "investment",
                table: "DesignEstimates",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_PppCapitalPlans_ProjectId",
                schema: "investment",
                table: "PppCapitalPlans",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_PppDisbursementRecords_ProjectId_ReportDate",
                schema: "investment",
                table: "PppDisbursementRecords",
                columns: new[] { "ProjectId", "ReportDate" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PppExecutionRecords_ProjectId",
                schema: "investment",
                table: "PppExecutionRecords",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_PppInvestmentDecisions_ProjectId",
                schema: "investment",
                table: "PppInvestmentDecisions",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_RevenueReports_ProjectId",
                schema: "investment",
                table: "RevenueReports",
                column: "ProjectId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DesignEstimateItems",
                schema: "investment");

            migrationBuilder.DropTable(
                name: "InvestorSelectionInvestors",
                schema: "investment");

            migrationBuilder.DropTable(
                name: "PppCapitalPlans",
                schema: "investment");

            migrationBuilder.DropTable(
                name: "PppContractInfos",
                schema: "investment");

            migrationBuilder.DropTable(
                name: "PppDisbursementRecords",
                schema: "investment");

            migrationBuilder.DropTable(
                name: "PppExecutionRecords",
                schema: "investment");

            migrationBuilder.DropTable(
                name: "PppInvestmentDecisions",
                schema: "investment");

            migrationBuilder.DropTable(
                name: "RevenueReports",
                schema: "investment");

            migrationBuilder.DropTable(
                name: "DesignEstimates",
                schema: "investment");

            migrationBuilder.DropTable(
                name: "InvestorSelections",
                schema: "investment");

            migrationBuilder.DropTable(
                name: "PppProjects",
                schema: "investment");

            migrationBuilder.DropColumn(
                name: "IndustrialZoneName",
                schema: "investment",
                table: "ProjectLocations");
        }
    }
}
