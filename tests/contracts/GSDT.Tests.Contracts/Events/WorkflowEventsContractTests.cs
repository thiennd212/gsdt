using System.Text.Json;
using GSDT.Workflow.Domain.Events;

namespace GSDT.Tests.Contracts.Events;

/// <summary>
/// Contract tests for Workflow module domain events — TC-CON-EVT-007.
/// WorkflowTransitionedEvent consumed by Audit module for NĐ13/Law 91/2025 trail.
/// </summary>
public sealed class WorkflowEventsContractTests
{
    [Fact]
    [Trait("Category", "Contract")]
    public void WorkflowTransitionedEvent_RoundTrip()
    {
        var evt = new WorkflowTransitionedEvent(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());

        var json = JsonSerializer.Serialize(evt);
        var d = JsonSerializer.Deserialize<WorkflowTransitionedEvent>(json);

        d.Should().NotBeNull();
        d!.InstanceId.Should().Be(evt.InstanceId);
        d.NewStateId.Should().Be(evt.NewStateId);
        d.TransitionedBy.Should().Be(evt.TransitionedBy);
    }

    [Fact]
    [Trait("Category", "Contract")]
    public void WorkflowTransitionedEvent_JsonShape_ContainsExpectedProperties()
    {
        var evt = new WorkflowTransitionedEvent(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());

        var json = JsonSerializer.Serialize(evt);
        var root = JsonDocument.Parse(json).RootElement;

        root.TryGetProperty("InstanceId", out _).Should().BeTrue();
        root.TryGetProperty("NewStateId", out _).Should().BeTrue();
        root.TryGetProperty("TransitionedBy", out _).Should().BeTrue();
        root.TryGetProperty("EventId", out _).Should().BeTrue();
        root.TryGetProperty("OccurredAt", out _).Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Contract")]
    public void SlaBreachedEvent_RoundTrip()
    {
        var evt = new SlaBreachedEvent(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            Guid.NewGuid(), DateTimeOffset.UtcNow.AddHours(-48), 48);

        var json = JsonSerializer.Serialize(evt);
        var d = JsonSerializer.Deserialize<SlaBreachedEvent>(json);

        d.Should().NotBeNull();
        d!.InstanceId.Should().Be(evt.InstanceId);
        d.TenantId.Should().Be(evt.TenantId);
        d.ElapsedHours.Should().Be(48);
    }
}
