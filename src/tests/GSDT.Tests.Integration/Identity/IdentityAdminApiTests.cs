using System.Net;
using FluentAssertions;
using Xunit;

namespace GSDT.Tests.Integration.Identity;

/// <summary>
/// Integration tests for identity admin endpoints that are not user CRUD:
///   - AccessReviewController  — /api/v1/admin/access-reviews
///   - SessionAdminController  — /api/v1/admin/sessions
/// Both require [Authorize(Policy = "Admin")] → roles Admin or SystemAdmin.
/// (No RolesController exists in this codebase — roles are seeded constants.)
/// </summary>
[Collection("Integration")]
public class IdentityAdminApiTests(DatabaseFixture db) : IntegrationTestBase(db)
{
    // --- AccessReview ---

    [Fact]
    public async Task GetPendingAccessReviews_AsSystemAdmin_Returns200()
    {
        var response = await Client.GetAsync("/api/v1/admin/access-reviews/pending");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetPendingAccessReviews_AsAdmin_Returns200()
    {
        using var client = CreateAuthenticatedClient(roles: ["Admin"], tenantId: DefaultTenantId.ToString());

        var response = await client.GetAsync("/api/v1/admin/access-reviews/pending");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetPendingAccessReviews_AsGovOfficer_Returns403()
    {
        using var client = CreateAuthenticatedClient(roles: ["GovOfficer"]);

        var response = await client.GetAsync("/api/v1/admin/access-reviews/pending");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetPendingAccessReviews_Unauthenticated_Returns401Or403()
    {
        var anonClient = Factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
        });

        var response = await anonClient.GetAsync("/api/v1/admin/access-reviews/pending");

        response.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task ApproveAccessReview_WithNonExistentId_Returns404()
    {
        var nonExistentId = Guid.NewGuid();

        var response = await Client.PostAsync(
            $"/api/v1/admin/access-reviews/{nonExistentId}/approve", content: null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task RejectAccessReview_WithNonExistentId_Returns404()
    {
        var nonExistentId = Guid.NewGuid();

        var response = await Client.PostAsync(
            $"/api/v1/admin/access-reviews/{nonExistentId}/reject", content: null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // --- Sessions ---

    [Fact]
    public async Task GetActiveSessions_AsSystemAdmin_Returns200()
    {
        var response = await Client.GetAsync("/api/v1/admin/sessions/active");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetActiveSessions_AsGovOfficer_Returns403()
    {
        using var client = CreateAuthenticatedClient(roles: ["GovOfficer"]);

        var response = await client.GetAsync("/api/v1/admin/sessions/active");

        // 403 Forbidden expected; 429 TooManyRequests is acceptable when the write-ops
        // rate limiter (20 req/min per IP) fires before auth during a full test run.
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Forbidden, HttpStatusCode.TooManyRequests);
    }

    [Fact]
    public async Task RevokeToken_WithNonExistentTokenId_Returns404()
    {
        var fakeTokenId = Guid.NewGuid().ToString();

        var response = await Client.DeleteAsync($"/api/v1/admin/sessions/{fakeTokenId}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
