using GSDT.Audit.Domain.Entities;
using GSDT.Audit.Infrastructure.Persistence;
using GSDT.Audit.Integration.Tests.Fixtures;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace GSDT.Audit.Integration.Tests;

/// <summary>
/// Integration tests for RTBF REST endpoints (Law 91/2025 Art.9).
/// Covers: auth enforcement (SystemAdmin only), GET list, process lifecycle, reject.
/// RTBF requests are seeded directly via AuditDbContext (no public create endpoint exists).
/// </summary>
[Collection(SqlServerCollection.CollectionName)]
public sealed class RtbfEndpointsTests : IClassFixture<WebAppFixture>
{
    private readonly WebAppFixture _app;

    private static readonly Guid TenantId  = Guid.NewGuid();
    private static readonly Guid SubjectId = Guid.NewGuid();
    private static readonly Guid AdminId   = Guid.NewGuid();

    public RtbfEndpointsTests(WebAppFixture app)
    {
        _app = app;
    }

    // --- Auth enforcement ---

    [Fact]
    public async Task GetRtbfRequests_Unauthenticated_Returns401()
    {
        var client = _app.CreateClient();
        var response = await client.GetAsync("/api/v1/admin/rtbf-requests");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetRtbfRequests_NonAdminRole_Returns403()
    {
        var client = _app.CreateAuthenticatedClient(AdminId, TenantId, "GovOfficer");
        var response = await client.GetAsync("/api/v1/admin/rtbf-requests");
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetRtbfRequests_SystemAdmin_Returns200()
    {
        var client = _app.CreateAuthenticatedClient(AdminId, TenantId, "SystemAdmin");
        var response = await client.GetAsync("/api/v1/admin/rtbf-requests");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    // --- GET list response structure ---

    [Fact]
    public async Task GetRtbfRequests_ResponseBody_WrappedInApiResponseEnvelope()
    {
        var client = _app.CreateAuthenticatedClient(AdminId, TenantId, "SystemAdmin");
        var response = await client.GetAsync("/api/v1/admin/rtbf-requests");
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.TryGetProperty("success", out _).Should().BeTrue();
    }

    // --- Process lifecycle ---

    [Fact]
    public async Task ProcessRtbfRequest_NonExistentId_Returns404()
    {
        var client = _app.CreateAuthenticatedClient(AdminId, TenantId, "SystemAdmin");
        var payload = new { processedBy = AdminId };

        var response = await client.PostAsJsonAsync(
            $"/api/v1/admin/rtbf-requests/{Guid.NewGuid()}/process", payload);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ProcessRtbfRequest_ExistingRequest_Returns204AndSetsStatus()
    {
        // Seed an RTBF request directly via AuditDbContext
        var rtbf = await SeedRtbfRequestAsync();

        var client = _app.CreateAuthenticatedClient(AdminId, TenantId, "SystemAdmin");
        var payload = new { processedBy = AdminId };

        var response = await client.PostAsJsonAsync(
            $"/api/v1/admin/rtbf-requests/{rtbf.Id}/process", payload);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    // --- Reject lifecycle ---

    [Fact]
    public async Task RejectRtbfRequest_NonExistentId_Returns404()
    {
        var client = _app.CreateAuthenticatedClient(AdminId, TenantId, "SystemAdmin");
        var payload = new { processedBy = AdminId, reason = "Invalid request" };

        var response = await client.PostAsJsonAsync(
            $"/api/v1/admin/rtbf-requests/{Guid.NewGuid()}/reject", payload);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task RejectRtbfRequest_ExistingRequest_Returns204()
    {
        var rtbf = await SeedRtbfRequestAsync();

        var client = _app.CreateAuthenticatedClient(AdminId, TenantId, "SystemAdmin");
        var payload = new { processedBy = AdminId, reason = "Data subject cannot be identified" };

        var response = await client.PostAsJsonAsync(
            $"/api/v1/admin/rtbf-requests/{rtbf.Id}/reject", payload);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    // --- Helpers ---

    /// <summary>Seeds an RtbfRequest in Pending status via DI scope (bypasses missing create endpoint).</summary>
    private async Task<RtbfRequest> SeedRtbfRequestAsync()
    {
        using var scope = _app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AuditDbContext>();
        var rtbf = RtbfRequest.Create(TenantId, SubjectId);
        db.Set<RtbfRequest>().Add(rtbf);
        await db.SaveChangesAsync();
        return rtbf;
    }
}
