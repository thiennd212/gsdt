using System.Text.Json;
using GSDT.Identity.Application.DTOs;
using GSDT.SharedKernel.Domain;

namespace GSDT.Tests.Contracts.Api;

/// <summary>
/// Contract tests for Identity module DTOs — TC-CON-API-007.
/// Ensures no sensitive fields leak to API consumers.
/// </summary>
public sealed class IdentityApiContractTests
{
    [Fact]
    [Trait("Category", "Contract")]
    public void UserDto_JsonShape_RequiredFieldsPresent()
    {
        var dto = new UserDto(
            Guid.NewGuid(), "Nguyen Van A", "a@gov.vn", "CNTT",
            ClassificationLevel.Internal, true, Guid.NewGuid(),
            DateTime.UtcNow, null, null, ["Admin"]);

        var json = JsonSerializer.Serialize(dto);
        var root = JsonDocument.Parse(json).RootElement;

        root.TryGetProperty("Id", out _).Should().BeTrue();
        root.TryGetProperty("FullName", out _).Should().BeTrue();
        root.TryGetProperty("Email", out _).Should().BeTrue();
        root.TryGetProperty("IsActive", out _).Should().BeTrue();
        root.TryGetProperty("Roles", out var roles).Should().BeTrue();
        roles.ValueKind.Should().Be(JsonValueKind.Array);
    }

    [Fact]
    [Trait("Category", "Contract")]
    public void UserDto_NoSensitiveFieldsExposed()
    {
        var dto = new UserDto(
            Guid.NewGuid(), "Test", "t@gov.vn", null,
            ClassificationLevel.Public, true, null,
            DateTime.UtcNow, null, null, []);

        var json = JsonSerializer.Serialize(dto);
        var root = JsonDocument.Parse(json).RootElement;

        // These internal fields must NEVER appear in serialized output
        root.TryGetProperty("PasswordHash", out _).Should().BeFalse();
        root.TryGetProperty("SecurityStamp", out _).Should().BeFalse();
        root.TryGetProperty("NormalizedEmail", out _).Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Contract")]
    public void UserDto_RoundTrip_Lossless()
    {
        var dto = new UserDto(
            Guid.NewGuid(), "Tran B", "b@gov.vn", "TCCB",
            ClassificationLevel.Confidential, true, Guid.NewGuid(),
            DateTime.UtcNow, DateTime.UtcNow, DateTime.UtcNow.AddDays(90),
            ["User", "Manager"]);

        var json = JsonSerializer.Serialize(dto);
        var d = JsonSerializer.Deserialize<UserDto>(json);

        d.Should().NotBeNull();
        d!.FullName.Should().Be(dto.FullName);
        d.Email.Should().Be(dto.Email);
        d.Roles.Should().HaveCount(2);
    }
}
