using System.Text.Json;
using GSDT.Cases.Domain.Events;
using FluentAssertions;

namespace GSDT.Tests.Contracts.Events;

/// <summary>
/// Contract tests for Cases module domain events — serialization round-trip stability.
/// These events are consumed cross-module by Audit and Notifications handlers.
/// </summary>
public sealed class CaseDomainEventsContractTests
{
    [Fact]
    [Trait("Category", "Contract")]
    public void CaseCreatedEvent_RoundTrip()
    {
        var evt = new CaseCreatedEvent(
            Guid.Parse("11111111-1111-1111-1111-111111111111"),
            Guid.Parse("22222222-2222-2222-2222-222222222222"),
            "CASE-2026-001",
            "TRK-ABC123",
            Guid.Parse("33333333-3333-3333-3333-333333333333"));

        var json = JsonSerializer.Serialize(evt);
        var d = JsonSerializer.Deserialize<CaseCreatedEvent>(json);

        d.Should().NotBeNull();
        d!.CaseId.Should().Be(evt.CaseId);
        d.TenantId.Should().Be(evt.TenantId);
        d.CaseNumber.Should().Be(evt.CaseNumber);
        d.TrackingCode.Should().Be(evt.TrackingCode);
        d.CreatedBy.Should().Be(evt.CreatedBy);
    }

    [Fact]
    [Trait("Category", "Contract")]
    public void CaseSubmittedEvent_RoundTrip()
    {
        var evt = new CaseSubmittedEvent(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
        var json = JsonSerializer.Serialize(evt);
        var d = JsonSerializer.Deserialize<CaseSubmittedEvent>(json);

        d.Should().NotBeNull();
        d!.CaseId.Should().Be(evt.CaseId);
        d.SubmittedBy.Should().Be(evt.SubmittedBy);
    }

    [Fact]
    [Trait("Category", "Contract")]
    public void CaseAssignedEvent_RoundTrip()
    {
        var evt = new CaseAssignedEvent(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "IT Department");
        var json = JsonSerializer.Serialize(evt);
        var d = JsonSerializer.Deserialize<CaseAssignedEvent>(json);

        d.Should().NotBeNull();
        d!.Department.Should().Be("IT Department");
    }

    [Fact]
    [Trait("Category", "Contract")]
    public void CaseApprovedEvent_RoundTrip()
    {
        var evt = new CaseApprovedEvent(Guid.NewGuid(), Guid.NewGuid(), "All good", Guid.NewGuid());
        var json = JsonSerializer.Serialize(evt);
        var d = JsonSerializer.Deserialize<CaseApprovedEvent>(json);

        d.Should().NotBeNull();
        d!.Reason.Should().Be("All good");
    }

    [Fact]
    [Trait("Category", "Contract")]
    public void CaseRejectedEvent_RoundTrip()
    {
        var evt = new CaseRejectedEvent(Guid.NewGuid(), Guid.NewGuid(), "Missing docs", Guid.NewGuid());
        var json = JsonSerializer.Serialize(evt);
        var d = JsonSerializer.Deserialize<CaseRejectedEvent>(json);

        d.Should().NotBeNull();
        d!.Reason.Should().Be("Missing docs");
    }

    [Fact]
    [Trait("Category", "Contract")]
    public void CaseClosedEvent_RoundTrip()
    {
        var evt = new CaseClosedEvent(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
        var json = JsonSerializer.Serialize(evt);
        var d = JsonSerializer.Deserialize<CaseClosedEvent>(json);

        d.Should().NotBeNull();
        d!.ClosedBy.Should().Be(evt.ClosedBy);
    }
}
