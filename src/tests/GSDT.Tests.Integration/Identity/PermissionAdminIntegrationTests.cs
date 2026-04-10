using System.Net;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;

namespace GSDT.Tests.Integration.Identity;

/// <summary>
/// HTTP-level tests for PermissionsAdminController and RolesAdminController.
/// Both use [Authorize(Policy = "Admin")] — requires Admin or SystemAdmin role.
/// These are ROLE-based checks, not permission-based.
///
/// Verified routes:
///   GET  /api/v1/admin/permissions       — list all permissions
///   GET  /api/v1/admin/roles             — list all roles
///   GET  /api/v1/admin/roles/{id}        — get single role (404 for unknown id)
/// </summary>
[Collection("Integration")]
public class PermissionAdminIntegrationTests(DatabaseFixture db) : IntegrationTestBase(db)
{
    // ── Permissions Admin ────────────────────────────────────────────────────

    [Fact]
    public async Task ListPermissions_AsAdmin_Returns200()
    {
        // Default Client has Admin + SystemAdmin roles (from IntegrationTestBase)
        var response = await Client.GetAsync("/api/v1/admin/permissions");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ListPermissions_AsNonAdmin_Returns403()
    {
        using var client = CreateAuthenticatedClient(roles: ["BTC"], tenantId: DefaultTenantId.ToString());
        var response = await client.GetAsync("/api/v1/admin/permissions");
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task ListPermissions_Unauthenticated_Returns401Or403()
    {
        using var anonClient = Factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
        });
        var response = await anonClient.GetAsync("/api/v1/admin/permissions");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden);
    }

    // ── Roles Admin ──────────────────────────────────────────────────────────

    [Fact]
    public async Task ListRoles_AsAdmin_Returns200()
    {
        var response = await Client.GetAsync("/api/v1/admin/roles");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ListRoles_AsBtc_Returns403()
    {
        using var client = CreateAuthenticatedClient(roles: ["BTC"], tenantId: DefaultTenantId.ToString());
        var response = await client.GetAsync("/api/v1/admin/roles");
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetRoleById_AsAdmin_ReturnsNotForbidden()
    {
        var fakeId = Guid.NewGuid();
        var response = await Client.GetAsync($"/api/v1/admin/roles/{fakeId}");
        // 200 or 404 — but NOT 403 (auth passed, role with fakeId simply not found)
        response.StatusCode.Should().NotBe(HttpStatusCode.Forbidden);
    }
}
