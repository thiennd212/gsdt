using System.Text.Json;
using GSDT.Cases.Application.DTOs;

namespace GSDT.Tests.Contracts.Api;

/// <summary>
/// Contract tests for Cases module DTOs — TC-CON-API-003, TC-CON-API-004.
/// </summary>
public sealed class CasesApiContractTests
{
    private static CaseDto CreateSample() => new(
        Guid.NewGuid(), Guid.NewGuid(), "CASE-2026-001", "TRK-ABC",
        "Test Case", "Description", "KhieuNai", "High", "Draft",
        null, null, null, null, Guid.NewGuid(),
        DateTimeOffset.UtcNow, null, []);

    [Fact]
    [Trait("Category", "Contract")]
    public void CaseDto_JsonShape_AllRequiredFieldsPresent()
    {
        var dto = CreateSample();
        var json = JsonSerializer.Serialize(dto);
        var root = JsonDocument.Parse(json).RootElement;

        root.TryGetProperty("Id", out _).Should().BeTrue();
        root.TryGetProperty("TenantId", out _).Should().BeTrue();
        root.TryGetProperty("CaseNumber", out _).Should().BeTrue();
        root.TryGetProperty("TrackingCode", out _).Should().BeTrue();
        root.TryGetProperty("Title", out _).Should().BeTrue();
        root.TryGetProperty("Status", out _).Should().BeTrue();
        root.TryGetProperty("Type", out _).Should().BeTrue();
        root.TryGetProperty("Priority", out _).Should().BeTrue();
        root.TryGetProperty("CreatedBy", out _).Should().BeTrue();
        root.TryGetProperty("Comments", out _).Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Contract")]
    public void CaseDto_Status_SerializesAsString()
    {
        var dto = CreateSample();
        var json = JsonSerializer.Serialize(dto);
        var root = JsonDocument.Parse(json).RootElement;

        root.TryGetProperty("Status", out var status).Should().BeTrue();
        status.ValueKind.Should().Be(JsonValueKind.String);
        status.GetString().Should().Be("Draft");
    }

    [Fact]
    [Trait("Category", "Contract")]
    public void CaseDto_RoundTrip_Lossless()
    {
        var dto = CreateSample();
        var json = JsonSerializer.Serialize(dto);
        var d = JsonSerializer.Deserialize<CaseDto>(json);

        d.Should().NotBeNull();
        d!.Id.Should().Be(dto.Id);
        d.TenantId.Should().Be(dto.TenantId);
        d.CaseNumber.Should().Be(dto.CaseNumber);
        d.TrackingCode.Should().Be(dto.TrackingCode);
        d.Title.Should().Be(dto.Title);
        d.Description.Should().Be(dto.Description);
        d.Type.Should().Be(dto.Type);
        d.Priority.Should().Be(dto.Priority);
        d.Status.Should().Be(dto.Status);
        d.CreatedBy.Should().Be(dto.CreatedBy);
        d.Comments.Should().BeEmpty();
    }
}
