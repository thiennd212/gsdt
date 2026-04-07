using System.Text.Json;
using GSDT.Forms.Application.DTOs;

namespace GSDT.Tests.Contracts.Api;

/// <summary>
/// Contract tests for Forms module DTOs — TC-CON-API-006.
/// </summary>
public sealed class FormsApiContractTests
{
    [Fact]
    [Trait("Category", "Contract")]
    public void FormTemplateDto_JsonShape_AllFieldsPresent()
    {
        var field = new FormFieldDto(
            Guid.NewGuid(), "full_name", "Text", "Họ tên", "Full Name",
            1, true, true, 1, null, null);

        var dto = new FormTemplateDto(
            Guid.NewGuid(), Guid.NewGuid(), "Citizen Request",
            "Đơn yêu cầu", "FORM-001", "Json", "Published",
            1, DateTimeOffset.UtcNow, [field]);

        var json = JsonSerializer.Serialize(dto);
        var root = JsonDocument.Parse(json).RootElement;

        root.TryGetProperty("Id", out _).Should().BeTrue();
        root.TryGetProperty("Name", out _).Should().BeTrue();
        root.TryGetProperty("Code", out _).Should().BeTrue();
        root.TryGetProperty("Status", out _).Should().BeTrue();
        root.TryGetProperty("Version", out _).Should().BeTrue();
        root.TryGetProperty("Fields", out var fields).Should().BeTrue();
        fields.ValueKind.Should().Be(JsonValueKind.Array);
        fields.GetArrayLength().Should().Be(1);
    }

    [Fact]
    [Trait("Category", "Contract")]
    public void FormTemplateDto_RoundTrip_Lossless()
    {
        var dto = new FormTemplateDto(
            Guid.NewGuid(), Guid.NewGuid(), "Test", "Test",
            "T-001", "Json", "Draft", 1, DateTimeOffset.UtcNow, []);

        var json = JsonSerializer.Serialize(dto);
        var d = JsonSerializer.Deserialize<FormTemplateDto>(json);

        d.Should().NotBeNull();
        d!.Code.Should().Be(dto.Code);
        d.Version.Should().Be(dto.Version);
        d.StorageMode.Should().Be(dto.StorageMode);
    }
}
