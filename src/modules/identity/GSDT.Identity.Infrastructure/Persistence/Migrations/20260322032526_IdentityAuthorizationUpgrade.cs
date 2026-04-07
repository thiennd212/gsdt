using System;

#nullable disable

namespace GSDT.Identity.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class IdentityAuthorizationUpgrade : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Permissions_ModuleName_Name",
                schema: "identity",
                table: "Permissions");

            migrationBuilder.RenameColumn(
                name: "ModuleName",
                schema: "identity",
                table: "Permissions",
                newName: "Code");

            migrationBuilder.AddColumn<DateTime>(
                name: "ApprovedAtUtc",
                schema: "identity",
                table: "UserDelegations",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ApprovedBy",
                schema: "identity",
                table: "UserDelegations",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DelegatedRoleIds",
                schema: "identity",
                table: "UserDelegations",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "RequiresApproval",
                schema: "identity",
                table: "UserDelegations",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ScopeJson",
                schema: "identity",
                table: "UserDelegations",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                schema: "identity",
                table: "UserDelegations",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                schema: "identity",
                table: "Permissions",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AddColumn<string>(
                name: "ActionCode",
                schema: "identity",
                table: "Permissions",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsSensitive",
                schema: "identity",
                table: "Permissions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ModuleCode",
                schema: "identity",
                table: "Permissions",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ResourceCode",
                schema: "identity",
                table: "Permissions",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "AuthSource",
                schema: "identity",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "EmployeeCode",
                schema: "identity",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PositionName",
                schema: "identity",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PrimaryOrgUnitId",
                schema: "identity",
                table: "AspNetUsers",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Code",
                schema: "identity",
                table: "AspNetRoles",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                schema: "identity",
                table: "AspNetRoles",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "RoleType",
                schema: "identity",
                table: "AspNetRoles",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "AppMenus",
                schema: "identity",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ParentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Code = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Icon = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Route = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppMenus", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppMenus_AppMenus_ParentId",
                        column: x => x.ParentId,
                        principalSchema: "identity",
                        principalTable: "AppMenus",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DataScopeTypes",
                schema: "identity",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataScopeTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PolicyRules",
                schema: "identity",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PermissionCode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ConditionExpression = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Effect = table.Column<int>(type: "int", nullable: false),
                    Priority = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    LogOnDeny = table.Column<bool>(type: "bit", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PolicyRules", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SodConflictRules",
                schema: "identity",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PermissionCodeA = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    PermissionCodeB = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    EnforcementLevel = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SodConflictRules", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserGroups",
                schema: "identity",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserGroups", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MenuRolePermissions",
                schema: "identity",
                columns: table => new
                {
                    MenuId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PermissionCode = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MenuRolePermissions", x => new { x.MenuId, x.PermissionCode });
                    table.ForeignKey(
                        name: "FK_MenuRolePermissions_AppMenus_MenuId",
                        column: x => x.MenuId,
                        principalSchema: "identity",
                        principalTable: "AppMenus",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RoleDataScopes",
                schema: "identity",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RoleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DataScopeTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ScopeField = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ScopeValue = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Priority = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoleDataScopes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RoleDataScopes_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalSchema: "identity",
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RoleDataScopes_DataScopeTypes_DataScopeTypeId",
                        column: x => x.DataScopeTypeId,
                        principalSchema: "identity",
                        principalTable: "DataScopeTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserDataScopeOverrides",
                schema: "identity",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DataScopeTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ScopeField = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ScopeValue = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Reason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    GrantedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GrantedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpiresAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserDataScopeOverrides", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserDataScopeOverrides_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalSchema: "identity",
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserDataScopeOverrides_DataScopeTypes_DataScopeTypeId",
                        column: x => x.DataScopeTypeId,
                        principalSchema: "identity",
                        principalTable: "DataScopeTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GroupRoleAssignments",
                schema: "identity",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GroupId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RoleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AssignedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AssignedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroupRoleAssignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GroupRoleAssignments_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalSchema: "identity",
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GroupRoleAssignments_UserGroups_GroupId",
                        column: x => x.GroupId,
                        principalSchema: "identity",
                        principalTable: "UserGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserGroupMemberships",
                schema: "identity",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GroupId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AddedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AddedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserGroupMemberships", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserGroupMemberships_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalSchema: "identity",
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserGroupMemberships_UserGroups_GroupId",
                        column: x => x.GroupId,
                        principalSchema: "identity",
                        principalTable: "UserGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Permissions_Code",
                schema: "identity",
                table: "Permissions",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoles_Code_TenantId",
                schema: "identity",
                table: "AspNetRoles",
                columns: new[] { "Code", "TenantId" },
                unique: true,
                filter: "[TenantId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AppMenus_Code_TenantId",
                schema: "identity",
                table: "AppMenus",
                columns: new[] { "Code", "TenantId" },
                unique: true,
                filter: "[TenantId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AppMenus_ParentId",
                schema: "identity",
                table: "AppMenus",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_AppMenus_TenantId",
                schema: "identity",
                table: "AppMenus",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_DataScopeTypes_Code",
                schema: "identity",
                table: "DataScopeTypes",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GroupRoleAssignments_GroupId_RoleId",
                schema: "identity",
                table: "GroupRoleAssignments",
                columns: new[] { "GroupId", "RoleId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GroupRoleAssignments_RoleId",
                schema: "identity",
                table: "GroupRoleAssignments",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_MenuRolePermissions_PermissionCode",
                schema: "identity",
                table: "MenuRolePermissions",
                column: "PermissionCode");

            migrationBuilder.CreateIndex(
                name: "IX_PolicyRules_Code",
                schema: "identity",
                table: "PolicyRules",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PolicyRules_PermissionCode_TenantId_Priority",
                schema: "identity",
                table: "PolicyRules",
                columns: new[] { "PermissionCode", "TenantId", "Priority" });

            migrationBuilder.CreateIndex(
                name: "IX_RoleDataScopes_DataScopeTypeId",
                schema: "identity",
                table: "RoleDataScopes",
                column: "DataScopeTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_RoleDataScopes_RoleId",
                schema: "identity",
                table: "RoleDataScopes",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_RoleDataScopes_RoleId_DataScopeTypeId",
                schema: "identity",
                table: "RoleDataScopes",
                columns: new[] { "RoleId", "DataScopeTypeId" });

            migrationBuilder.CreateIndex(
                name: "IX_SodConflictRules_IsActive",
                schema: "identity",
                table: "SodConflictRules",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_SodConflictRules_PermissionCodeA_PermissionCodeB_TenantId",
                schema: "identity",
                table: "SodConflictRules",
                columns: new[] { "PermissionCodeA", "PermissionCodeB", "TenantId" },
                unique: true,
                filter: "[TenantId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_SodConflictRules_TenantId",
                schema: "identity",
                table: "SodConflictRules",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_UserDataScopeOverrides_DataScopeTypeId",
                schema: "identity",
                table: "UserDataScopeOverrides",
                column: "DataScopeTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_UserDataScopeOverrides_UserId_ExpiresAtUtc",
                schema: "identity",
                table: "UserDataScopeOverrides",
                columns: new[] { "UserId", "ExpiresAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_UserGroupMemberships_GroupId",
                schema: "identity",
                table: "UserGroupMemberships",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_UserGroupMemberships_UserId_GroupId",
                schema: "identity",
                table: "UserGroupMemberships",
                columns: new[] { "UserId", "GroupId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserGroups_Code_TenantId",
                schema: "identity",
                table: "UserGroups",
                columns: new[] { "Code", "TenantId" },
                unique: true,
                filter: "[TenantId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_UserGroups_TenantId",
                schema: "identity",
                table: "UserGroups",
                column: "TenantId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GroupRoleAssignments",
                schema: "identity");

            migrationBuilder.DropTable(
                name: "MenuRolePermissions",
                schema: "identity");

            migrationBuilder.DropTable(
                name: "PolicyRules",
                schema: "identity");

            migrationBuilder.DropTable(
                name: "RoleDataScopes",
                schema: "identity");

            migrationBuilder.DropTable(
                name: "SodConflictRules",
                schema: "identity");

            migrationBuilder.DropTable(
                name: "UserDataScopeOverrides",
                schema: "identity");

            migrationBuilder.DropTable(
                name: "UserGroupMemberships",
                schema: "identity");

            migrationBuilder.DropTable(
                name: "AppMenus",
                schema: "identity");

            migrationBuilder.DropTable(
                name: "DataScopeTypes",
                schema: "identity");

            migrationBuilder.DropTable(
                name: "UserGroups",
                schema: "identity");

            migrationBuilder.DropIndex(
                name: "IX_Permissions_Code",
                schema: "identity",
                table: "Permissions");

            migrationBuilder.DropIndex(
                name: "IX_AspNetRoles_Code_TenantId",
                schema: "identity",
                table: "AspNetRoles");

            migrationBuilder.DropColumn(
                name: "ApprovedAtUtc",
                schema: "identity",
                table: "UserDelegations");

            migrationBuilder.DropColumn(
                name: "ApprovedBy",
                schema: "identity",
                table: "UserDelegations");

            migrationBuilder.DropColumn(
                name: "DelegatedRoleIds",
                schema: "identity",
                table: "UserDelegations");

            migrationBuilder.DropColumn(
                name: "RequiresApproval",
                schema: "identity",
                table: "UserDelegations");

            migrationBuilder.DropColumn(
                name: "ScopeJson",
                schema: "identity",
                table: "UserDelegations");

            migrationBuilder.DropColumn(
                name: "Status",
                schema: "identity",
                table: "UserDelegations");

            migrationBuilder.DropColumn(
                name: "ActionCode",
                schema: "identity",
                table: "Permissions");

            migrationBuilder.DropColumn(
                name: "IsSensitive",
                schema: "identity",
                table: "Permissions");

            migrationBuilder.DropColumn(
                name: "ModuleCode",
                schema: "identity",
                table: "Permissions");

            migrationBuilder.DropColumn(
                name: "ResourceCode",
                schema: "identity",
                table: "Permissions");

            migrationBuilder.DropColumn(
                name: "AuthSource",
                schema: "identity",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "EmployeeCode",
                schema: "identity",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "PositionName",
                schema: "identity",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "PrimaryOrgUnitId",
                schema: "identity",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Code",
                schema: "identity",
                table: "AspNetRoles");

            migrationBuilder.DropColumn(
                name: "IsActive",
                schema: "identity",
                table: "AspNetRoles");

            migrationBuilder.DropColumn(
                name: "RoleType",
                schema: "identity",
                table: "AspNetRoles");

            migrationBuilder.RenameColumn(
                name: "Code",
                schema: "identity",
                table: "Permissions",
                newName: "ModuleName");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                schema: "identity",
                table: "Permissions",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.CreateIndex(
                name: "IX_Permissions_ModuleName_Name",
                schema: "identity",
                table: "Permissions",
                columns: new[] { "ModuleName", "Name" },
                unique: true);
        }
    }
}
