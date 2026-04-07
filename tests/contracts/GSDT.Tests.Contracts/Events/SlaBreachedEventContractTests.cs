using System.Text.Json;
using GSDT.Workflow.Domain.Events;
using FluentAssertions;

namespace GSDT.Tests.Contracts.Events;

/// <summary>
/// Contract test for SlaBreachedEvent — serialization round-trip stability.
/// TenantId is Guid (aligns with all other modules).
/// ElapsedHours is int (not double).
/// </summary>
public sealed class SlaBreachedEventContractTests
{
    private static readonly Guid SampleTenantId = Guid.Parse("44444444-4444-4444-4444-444444444444");

    [Fact]
    [Trait("Category", "Contract")]
    public void SlaBreachedEvent_RoundTrip()
    {
        var evt = new SlaBreachedEvent(
            InstanceId: Guid.Parse("11111111-1111-1111-1111-111111111111"),
            CurrentStateId: Guid.Parse("22222222-2222-2222-2222-222222222222"),
            DefinitionId: Guid.Parse("33333333-3333-3333-3333-333333333333"),
            TenantId: SampleTenantId,
            StateEnteredAt: DateTimeOffset.Parse("2026-03-17T10:00:00Z"),
            ElapsedHours: 48);

        var json = JsonSerializer.Serialize(evt);
        var d = JsonSerializer.Deserialize<SlaBreachedEvent>(json);

        d.Should().NotBeNull();
        d!.InstanceId.Should().Be(evt.InstanceId);
        d.CurrentStateId.Should().Be(evt.CurrentStateId);
        d.DefinitionId.Should().Be(evt.DefinitionId);
        d.TenantId.Should().Be(SampleTenantId);
        d.ElapsedHours.Should().Be(48);
    }

    [Fact]
    [Trait("Category", "Contract")]
    public void SlaBreachedEvent_JsonShape_ContainsExpectedProperties()
    {
        var evt = new SlaBreachedEvent(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            Guid.NewGuid(), DateTimeOffset.UtcNow, 24);

        var json = JsonSerializer.Serialize(evt);
        var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        root.TryGetProperty("InstanceId", out _).Should().BeTrue();
        root.TryGetProperty("TenantId", out _).Should().BeTrue();
        root.TryGetProperty("ElapsedHours", out _).Should().BeTrue();
        root.TryGetProperty("StateEnteredAt", out _).Should().BeTrue();
    }
}
