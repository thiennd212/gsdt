using System.Text.Json;
using GSDT.Notifications.Application.DTOs;

namespace GSDT.Tests.Contracts.Api;

/// <summary>
/// Contract tests for Notifications module DTOs — TC-CON-API-009.
/// </summary>
public sealed class NotificationsApiContractTests
{
    [Fact]
    [Trait("Category", "Contract")]
    public void NotificationDto_JsonShape_AllFieldsPresent()
    {
        var dto = new NotificationDto(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            "Thông báo mới", "Nội dung thông báo", "Email",
            false, null, DateTimeOffset.UtcNow);

        var json = JsonSerializer.Serialize(dto);
        var root = JsonDocument.Parse(json).RootElement;

        root.TryGetProperty("Id", out _).Should().BeTrue();
        root.TryGetProperty("Subject", out _).Should().BeTrue();
        root.TryGetProperty("Body", out _).Should().BeTrue();
        root.TryGetProperty("Channel", out _).Should().BeTrue();
        root.TryGetProperty("IsRead", out _).Should().BeTrue();
        root.TryGetProperty("CreatedAt", out _).Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Contract")]
    public void NotificationDto_RoundTrip_Lossless()
    {
        var dto = new NotificationDto(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            "Subject", "Body", "InApp", true,
            DateTimeOffset.UtcNow, DateTimeOffset.UtcNow);

        var json = JsonSerializer.Serialize(dto);
        var d = JsonSerializer.Deserialize<NotificationDto>(json);

        d.Should().NotBeNull();
        d!.Subject.Should().Be(dto.Subject);
        d.Channel.Should().Be(dto.Channel);
        d.IsRead.Should().BeTrue();
    }
}
