using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Xunit;

namespace GSDT.Tests.Integration.Forms;

/// <summary>
/// Integration tests for FormTemplatesController — /api/v1/forms/templates
///
/// Routes:
///   GET  /api/v1/forms/templates?tenantId={guid}         → List (paged, any authenticated)
///   GET  /api/v1/forms/templates/{id}?tenantId={guid}    → GetById (any authenticated)
///   POST /api/v1/forms/templates                          → Create [Admin,SystemAdmin]
///         body: { TenantId, Name, NameVi, SchemaJson, CreatedBy }
///   POST /api/v1/forms/templates/{id}/publish             → Publish [Admin,SystemAdmin]
///         body: { TenantId, UpdatedBy }
///
/// SchemaJson must be a valid JSON array of field objects with fieldId + type.
/// Templates start inactive (IsActive=false) until published.
/// </summary>
[Collection("Integration")]
public class FormTemplatesApiTests(DatabaseFixture db) : IntegrationTestBase(db)
{
    private const string BaseUrl = "/api/v1/forms/templates";

    // Minimal valid schema JSON required by FormValidationService
    private static readonly string ValidSchemaJson =
        """[{"fieldId":"applicantName","type":"text","label":"Applicant Name","required":true}]""";

    // -----------------------------------------------------------------
    // LIST
    // -----------------------------------------------------------------

    [Fact]
    public async Task ListFormTemplates_AsAuthenticatedUser_Returns200()
    {
        var tenantId = Guid.NewGuid();
        var response = await Client.GetAsync($"{BaseUrl}?tenantId={tenantId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ListFormTemplates_ResponseBody_WrappedInApiResponseEnvelope()
    {
        var tenantId = Guid.NewGuid();
        var response = await Client.GetAsync($"{BaseUrl}?tenantId={tenantId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.TryGetProperty("data", out _).Should().BeTrue("response must be ApiResponse envelope");
    }

    [Fact]
    public async Task ListFormTemplates_Unauthenticated_Returns401Or403()
    {
        var tenantId = Guid.NewGuid();
        var anonClient = Factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
        });

        var response = await anonClient.GetAsync($"{BaseUrl}?tenantId={tenantId}");

        response.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden);
    }

    // -----------------------------------------------------------------
    // CREATE
    // -----------------------------------------------------------------

    [Fact]
    public async Task CreateFormTemplate_WithValidData_Returns200()
    {
        var tenantId = Guid.NewGuid();
        var createdBy = Guid.NewGuid();
        var client = CreateAuthenticatedClient(
            userId: createdBy.ToString(),
            roles: ["SystemAdmin"],
            tenantId: tenantId.ToString());

        var response = await client.PostAsJsonAsync(BaseUrl, new
        {
            TenantId = tenantId,
            Name = "Building Permit Application Form",
            NameVi = "Đơn xin cấp phép xây dựng",
            Code = "building_permit_form",
            StorageMode = 0, // Json
            SchemaJson = ValidSchemaJson,
            CreatedBy = createdBy,
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.TryGetProperty("data", out var data).Should().BeTrue();
        data.GetProperty("id").GetGuid().Should().NotBe(Guid.Empty);
    }

    [Fact]
    public async Task CreateFormTemplate_WithoutAdminRole_Returns403()
    {
        var tenantId = Guid.NewGuid();
        // CaseOfficer is not in [Authorize(Roles = "Admin,SystemAdmin")]
        var client = CreateAuthenticatedClient(
            userId: Guid.NewGuid().ToString(),
            roles: ["CaseOfficer"],
            tenantId: tenantId.ToString());

        var response = await client.PostAsJsonAsync(BaseUrl, new
        {
            TenantId = tenantId,
            Name = "Unauthorized Form",
            NameVi = "Form không có quyền",
            SchemaJson = ValidSchemaJson,
            CreatedBy = Guid.NewGuid(),
        });

        response.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden);
    }

    // -----------------------------------------------------------------
    // GET BY ID
    // -----------------------------------------------------------------

    [Fact]
    public async Task GetFormTemplateById_ForNonExistentId_Returns404()
    {
        var tenantId = Guid.NewGuid();
        var nonExistentId = Guid.NewGuid();

        var response = await Client.GetAsync($"{BaseUrl}/{nonExistentId}?tenantId={tenantId}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetFormTemplateById_ForExistingTemplate_Returns200WithData()
    {
        var tenantId = Guid.NewGuid();
        var createdBy = Guid.NewGuid();
        var adminClient = CreateAuthenticatedClient(
            userId: createdBy.ToString(),
            roles: ["SystemAdmin"],
            tenantId: tenantId.ToString());

        // Create template first
        var createResp = await adminClient.PostAsJsonAsync(BaseUrl, new
        {
            TenantId = tenantId,
            Name = "Complaint Form Template",
            NameVi = "Mẫu đơn khiếu nại",
            Code = "complaint_form_template",
            StorageMode = 0, // Json
            SchemaJson = ValidSchemaJson,
            CreatedBy = createdBy,
        });
        createResp.StatusCode.Should().Be(HttpStatusCode.OK);

        var createBody = await createResp.Content.ReadFromJsonAsync<JsonElement>();
        var templateId = createBody.GetProperty("data").GetProperty("id").GetGuid();

        // Publish it so GetById returns it (templates start inactive — GetById may require IsActive=true)
        // If GetById returns inactive too, this publish step still ensures the template is accessible
        await adminClient.PostAsJsonAsync(
            $"{BaseUrl}/{templateId}/publish",
            new { TenantId = tenantId, UpdatedBy = createdBy });

        // GetById — any authenticated user can read
        var getResp = await adminClient.GetAsync($"{BaseUrl}/{templateId}?tenantId={tenantId}");
        getResp.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await getResp.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("data").GetProperty("id").GetGuid().Should().Be(templateId);
    }

    // -----------------------------------------------------------------
    // PUBLISH
    // -----------------------------------------------------------------

    [Fact]
    public async Task PublishFormTemplate_WithValidData_Returns200Or204()
    {
        var tenantId = Guid.NewGuid();
        var createdBy = Guid.NewGuid();
        var adminClient = CreateAuthenticatedClient(
            userId: createdBy.ToString(),
            roles: ["SystemAdmin"],
            tenantId: tenantId.ToString());

        // Create
        var createResp = await adminClient.PostAsJsonAsync(BaseUrl, new
        {
            TenantId = tenantId,
            Name = "Report Submission Form",
            NameVi = "Mẫu báo cáo",
            Code = "report_submission_form",
            StorageMode = 0, // Json
            SchemaJson = ValidSchemaJson,
            CreatedBy = createdBy,
        });
        var createBody = await createResp.Content.ReadFromJsonAsync<JsonElement>();
        var templateId = createBody.GetProperty("data").GetProperty("id").GetGuid();

        // Add a field first — Publish requires at least one active field (GOV_FRM_003)
        var addFieldResp = await adminClient.PostAsJsonAsync(
            $"{BaseUrl}/{templateId}/fields",
            new
            {
                TenantId = tenantId,
                FieldKey = "applicantName",
                Type = 0, // FormFieldType.Text
                LabelVi = "Tên người nộp đơn",
                LabelEn = "Applicant Name",
                DisplayOrder = 1,
                Required = true,
                ValidationRulesJson = (string?)null,
                Options = (object?)null,
                UpdatedBy = createdBy,
            });
        addFieldResp.StatusCode.Should().BeOneOf(
            [HttpStatusCode.OK, HttpStatusCode.NoContent],
            "AddField must succeed before publish");

        // Publish
        var publishResp = await adminClient.PostAsJsonAsync(
            $"{BaseUrl}/{templateId}/publish",
            new { TenantId = tenantId, UpdatedBy = createdBy });

        // Controller ProducesResponseType(204) for publish, but ToApiResponse wraps it — accept 200 or 204
        publishResp.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task PublishFormTemplate_WithoutAdminRole_Returns403()
    {
        var tenantId = Guid.NewGuid();
        var restrictedClient = CreateAuthenticatedClient(
            userId: Guid.NewGuid().ToString(),
            roles: ["CaseOfficer"],
            tenantId: tenantId.ToString());

        var response = await restrictedClient.PostAsJsonAsync(
            $"{BaseUrl}/{Guid.NewGuid()}/publish",
            new { TenantId = tenantId, UpdatedBy = Guid.NewGuid() });

        response.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden);
    }
}
