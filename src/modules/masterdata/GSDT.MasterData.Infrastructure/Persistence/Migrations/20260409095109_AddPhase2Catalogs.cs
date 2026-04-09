using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GSDT.MasterData.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPhase2Catalogs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GovernmentAgencies",
                schema: "masterdata",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ParentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    AgencyType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Origin = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    LdaServer = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Address = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Fax = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    ReportDisplayOrder = table.Column<int>(type: "int", nullable: true),
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
                    table.PrimaryKey("PK_GovernmentAgencies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GovernmentAgencies_GovernmentAgencies_ParentId",
                        column: x => x.ParentId,
                        principalSchema: "masterdata",
                        principalTable: "GovernmentAgencies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Investors",
                schema: "masterdata",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InvestorType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    BusinessIdOrCccd = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    NameVi = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    NameEn = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_Investors", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GovernmentAgencies_Code_TenantId",
                schema: "masterdata",
                table: "GovernmentAgencies",
                columns: new[] { "Code", "TenantId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GovernmentAgencies_ParentId",
                schema: "masterdata",
                table: "GovernmentAgencies",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_GovernmentAgencies_TenantId",
                schema: "masterdata",
                table: "GovernmentAgencies",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Investors_BusinessIdOrCccd_TenantId",
                schema: "masterdata",
                table: "Investors",
                columns: new[] { "BusinessIdOrCccd", "TenantId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Investors_TenantId",
                schema: "masterdata",
                table: "Investors",
                column: "TenantId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GovernmentAgencies",
                schema: "masterdata");

            migrationBuilder.DropTable(
                name: "Investors",
                schema: "masterdata");
        }
    }
}
