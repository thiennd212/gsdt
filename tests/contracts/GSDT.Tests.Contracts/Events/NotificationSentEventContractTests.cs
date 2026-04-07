using System.Text.Json;
using GSDT.Notifications.Domain.Events;
using FluentAssertions;

namespace GSDT.Tests.Contracts.Events;

/// <summary>
/// Contract test: NotificationSentEvent serialization round-trip.
/// Ensures JSON shape is stable — any breaking change fails this test.
/// </summary>
public sealed class NotificationSentEventContractTests
{
    [Fact]
    [Trait("Category", "Contract")]
    public void NotificationSentEvent_Serialization_RoundTrip_IsStable()
    {
        var evt = new NotificationSentEvent(
            NotificationId: Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
            TenantId: Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
            RecipientUserId: Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc"),
            Channel: "Email");

        var json = JsonSerializer.Serialize(evt);
        var deserialized = JsonSerializer.Deserialize<NotificationSentEvent>(json);

        deserialized.Should().NotBeNull();
        deserialized!.NotificationId.Should().Be(evt.NotificationId);
        deserialized.TenantId.Should().Be(evt.TenantId);
        deserialized.RecipientUserId.Should().Be(evt.RecipientUserId);
        deserialized.Channel.Should().Be(evt.Channel);
    }

    [Fact]
    [Trait("Category", "Contract")]
    public void NotificationSentEvent_JsonShape_ContainsExpectedProperties()
    {
        var evt = new NotificationSentEvent(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Sms");

        var json = JsonSerializer.Serialize(evt);
        var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        root.TryGetProperty("NotificationId", out _).Should().BeTrue();
        root.TryGetProperty("TenantId", out _).Should().BeTrue();
        root.TryGetProperty("RecipientUserId", out _).Should().BeTrue();
        root.TryGetProperty("Channel", out _).Should().BeTrue();
        root.TryGetProperty("EventId", out _).Should().BeTrue();
        root.TryGetProperty("OccurredAt", out _).Should().BeTrue();
    }

    [Theory]
    [Trait("Category", "Contract")]
    [InlineData("Email")]
    [InlineData("Sms")]
    [InlineData("InApp")]
    public void NotificationSentEvent_Channel_SurvivesRoundTrip(string channel)
    {
        var evt = new NotificationSentEvent(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), channel);

        var json = JsonSerializer.Serialize(evt);
        var deserialized = JsonSerializer.Deserialize<NotificationSentEvent>(json);

        deserialized!.Channel.Should().Be(channel);
    }
}
