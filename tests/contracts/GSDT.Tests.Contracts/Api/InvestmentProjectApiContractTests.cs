using System.Text.Json;
using GSDT.InvestmentProjects.Application.DTOs;
using GSDT.SharedKernel.Api;
using GSDT.SharedKernel.Application.Pagination;

namespace GSDT.Tests.Contracts.Api;

/// <summary>
/// Contract tests for InvestmentProjects module DTOs — TC-CON-INV-001 through TC-CON-INV-006.
/// Pure serialization tests — no DB, no HTTP.
/// Ensures JSON shape stability for DomesticProject, OdaProject, and PppProject DTOs.
/// </summary>
public sealed class InvestmentProjectApiContractTests
{
    // ── DomesticProjectDetailDto ──────────────────────────────────────────────

    [Fact]
    [Trait("Category", "Contract")]
    public void DomesticProjectDetailDto_JsonShape_RequiredFieldsPresent()
    {
        var dto = CreateDomesticDetailDto();
        var json = JsonSerializer.Serialize(dto);
        var root = JsonDocument.Parse(json).RootElement;

        // Core identifiers
        root.TryGetProperty("Id", out _).Should().BeTrue();
        root.TryGetProperty("ProjectCode", out _).Should().BeTrue();
        root.TryGetProperty("ProjectName", out _).Should().BeTrue();
        root.TryGetProperty("StatusId", out _).Should().BeTrue();
        root.TryGetProperty("PrelimTotalInvestment", out _).Should().BeTrue();

        // Child arrays
        root.TryGetProperty("Locations", out var locs).Should().BeTrue();
        locs.ValueKind.Should().Be(JsonValueKind.Array);

        root.TryGetProperty("Decisions", out var decs).Should().BeTrue();
        decs.ValueKind.Should().Be(JsonValueKind.Array);

        root.TryGetProperty("Documents", out var docs).Should().BeTrue();
        docs.ValueKind.Should().Be(JsonValueKind.Array);
    }

    [Fact]
    [Trait("Category", "Contract")]
    public void DomesticProjectDetailDto_RoundTrip_Lossless()
    {
        var dto = CreateDomesticDetailDto();
        var json = JsonSerializer.Serialize(dto);
        var d = JsonSerializer.Deserialize<DomesticProjectDetailDto>(json);

        d.Should().NotBeNull();
        d!.Id.Should().Be(dto.Id);
        d.ProjectCode.Should().Be(dto.ProjectCode);
        d.ProjectName.Should().Be(dto.ProjectName);
        d.PrelimTotalInvestment.Should().Be(dto.PrelimTotalInvestment);
        d.Locations.Should().HaveCount(1);
        d.Decisions.Should().HaveCount(1);
        d.Documents.Should().HaveCount(1);
    }

    // ── DomesticProjectListItemDto ────────────────────────────────────────────

    [Fact]
    [Trait("Category", "Contract")]
    public void DomesticProjectListItemDto_JsonShape_RequiredFieldsPresent()
    {
        var dto = new DomesticProjectListItemDto(
            Guid.NewGuid(),
            "PRJ-DOM-001",
            "Du an trong nuoc thu nghiem",
            "QD-001",
            DateTime.UtcNow,
            "Ban quan ly du an A",
            "Dang thuc hien");

        var json = JsonSerializer.Serialize(dto);
        var root = JsonDocument.Parse(json).RootElement;

        root.TryGetProperty("Id", out _).Should().BeTrue();
        root.TryGetProperty("ProjectCode", out _).Should().BeTrue();
        root.TryGetProperty("ProjectName", out _).Should().BeTrue();
        root.TryGetProperty("LatestDecisionNumber", out _).Should().BeTrue();
        root.TryGetProperty("ProjectOwnerName", out _).Should().BeTrue();
        root.TryGetProperty("StatusName", out _).Should().BeTrue();
    }

    // ── ApiResponse<PagedResult<T>> envelope ─────────────────────────────────

    [Fact]
    [Trait("Category", "Contract")]
    public void ApiResponse_PagedResult_JsonShape_HasCorrectStructure()
    {
        var items = new List<DomesticProjectListItemDto>
        {
            new(Guid.NewGuid(), "PRJ-001", "Test Project", null, null, null, null)
        };
        var meta = new PaginationMeta(1, 10, 1, null, null, false);
        var paged = new PagedResult<DomesticProjectListItemDto>(items, 1, meta);
        var response = ApiResponse<PagedResult<DomesticProjectListItemDto>>.Ok(paged);

        var json = JsonSerializer.Serialize(response);
        var root = JsonDocument.Parse(json).RootElement;

        // Envelope structure
        root.TryGetProperty("Success", out var success).Should().BeTrue();
        success.GetBoolean().Should().BeTrue();
        root.TryGetProperty("Data", out var data).Should().BeTrue();

        // PagedResult structure
        data.TryGetProperty("Items", out var itemsEl).Should().BeTrue();
        itemsEl.ValueKind.Should().Be(JsonValueKind.Array);
        itemsEl.GetArrayLength().Should().Be(1);
        data.TryGetProperty("TotalCount", out var tc).Should().BeTrue();
        tc.GetInt32().Should().Be(1);
    }

    // ── OdaProjectListItemDto ─────────────────────────────────────────────────

    [Fact]
    [Trait("Category", "Contract")]
    public void OdaProjectListItemDto_JsonShape_RequiredFieldsPresent()
    {
        var dto = new OdaProjectListItemDto(
            Guid.NewGuid(),
            "PRJ-ODA-001",
            "Du an ODA thu nghiem",
            "ODA-SHORT",
            "ODA vay",
            DateTimeOffset.UtcNow,
            "Dang thuc hien");

        var json = JsonSerializer.Serialize(dto);
        var root = JsonDocument.Parse(json).RootElement;

        root.TryGetProperty("Id", out _).Should().BeTrue();
        root.TryGetProperty("ProjectCode", out _).Should().BeTrue();
        root.TryGetProperty("ProjectName", out _).Should().BeTrue();
        root.TryGetProperty("ShortName", out _).Should().BeTrue();
        root.TryGetProperty("StatusName", out _).Should().BeTrue();
    }

    // ── PppProjectListItemDto ─────────────────────────────────────────────────

    [Fact]
    [Trait("Category", "Contract")]
    public void PppProjectListItemDto_JsonShape_RequiredFieldsPresent()
    {
        var dto = new PppProjectListItemDto(
            Guid.NewGuid(),
            "PRJ-PPP-001",
            "Du an PPP thu nghiem",
            1,
            "Co quan co tham quyen A",
            "Don vi chuan bi du an B",
            500_000_000m,
            "Dang thuc hien",
            DateTimeOffset.UtcNow);

        var json = JsonSerializer.Serialize(dto);
        var root = JsonDocument.Parse(json).RootElement;

        root.TryGetProperty("Id", out _).Should().BeTrue();
        root.TryGetProperty("ProjectCode", out _).Should().BeTrue();
        root.TryGetProperty("ProjectName", out _).Should().BeTrue();
        root.TryGetProperty("ContractType", out _).Should().BeTrue();
        root.TryGetProperty("PrelimTotalInvestment", out _).Should().BeTrue();
        root.TryGetProperty("StatusName", out _).Should().BeTrue();
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static DomesticProjectDetailDto CreateDomesticDetailDto() =>
        new(
            Id: Guid.NewGuid(),
            ProjectCode: "PRJ-DOM-001",
            ProjectName: "Du an trong nuoc thu nghiem",
            ManagingAuthorityId: Guid.NewGuid(),
            IndustrySectorId: Guid.NewGuid(),
            ProjectOwnerId: Guid.NewGuid(),
            ProjectManagementUnitId: null,
            PmuDirectorName: null,
            PmuPhone: null,
            PmuEmail: null,
            ImplementationPeriod: "2024-2026",
            PolicyDecisionNumber: "QD-2024-001",
            PolicyDecisionDate: DateTime.UtcNow.AddMonths(-6),
            PolicyDecisionAuthority: "Bo truong BTC",
            PolicyDecisionPerson: null,
            PolicyDecisionFileId: null,
            SubProjectType: 1,
            TreasuryCode: "TC-001",
            ProjectGroupId: Guid.NewGuid(),
            PrelimCentralBudget: 300_000_000m,
            PrelimLocalBudget: 100_000_000m,
            PrelimOtherPublicCapital: 50_000_000m,
            PrelimPublicInvestment: 450_000_000m,
            PrelimOtherCapital: 50_000_000m,
            PrelimTotalInvestment: 500_000_000m,
            StatusId: Guid.NewGuid(),
            NationalTargetProgramId: null,
            StopContent: null,
            StopDecisionNumber: null,
            StopDecisionDate: null,
            StopFileId: null,
            Locations:
            [
                new ProjectLocationDto(Guid.NewGuid(), Guid.NewGuid(), null, null, "Ha Noi")
            ],
            Decisions:
            [
                new DecisionDto(
                    Guid.NewGuid(), 1, "QD-001", DateTime.UtcNow, "BTC",
                    500_000_000m, 300_000_000m, 100_000_000m, 50_000_000m,
                    50_000_000m, null, null, null)
            ],
            CapitalPlans: [],
            BidPackages: [],
            Executions: [],
            Disbursements: [],
            Inspections: [],
            Evaluations: [],
            Audits: [],
            Violations: [],
            Operation: null,
            Documents:
            [
                new ProjectDocumentDto(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Quyet dinh dau tu", DateTime.UtcNow, null)
            ]);
}
