using System.Reflection;
using GSDT.SharedKernel.Domain.Events;
using FluentAssertions;

namespace GSDT.Tests.Contracts.Events;

/// <summary>
/// Auto-discovery guard: ensures every IExternalDomainEvent type has a contract test.
/// When a new external event is added without a corresponding contract test, this fails.
/// </summary>
public sealed class EventSchemaDiscoveryTests
{
    /// <summary>
    /// Scans all loaded assemblies for IExternalDomainEvent implementations
    /// and verifies each has a serialization round-trip test in this project.
    /// </summary>
    [Fact]
    [Trait("Category", "Contract")]
    public void AllExternalDomainEvents_MustHaveContractTest()
    {
        // Load domain assemblies that may contain IExternalDomainEvent implementations
        var domainAssemblies = new[]
        {
            typeof(GSDT.Notifications.Domain.Events.NotificationSentEvent).Assembly,
            typeof(GSDT.Cases.Domain.Events.CaseCreatedEvent).Assembly,
            typeof(GSDT.Files.Domain.Events.FileUploadedEvent).Assembly,
            typeof(GSDT.Workflow.Domain.Events.SlaBreachedEvent).Assembly,
        };

        var eventTypes = domainAssemblies
            .SelectMany(a => a.GetTypes())
            .Where(t => t is { IsAbstract: false, IsInterface: false }
                        && typeof(IExternalDomainEvent).IsAssignableFrom(t))
            .ToList();

        // Current known external events — update this list when adding new ones
        var testedTypes = new HashSet<string>
        {
            "NotificationSentEvent",
        };

        foreach (var eventType in eventTypes)
        {
            testedTypes.Should().Contain(eventType.Name,
                $"IExternalDomainEvent '{eventType.FullName}' must have a contract test in this project. " +
                "Add a serialization round-trip test and update this set.");
        }
    }

    /// <summary>
    /// Verifies all IExternalDomainEvent types follow the IDs-only convention (no PII fields).
    /// Properties must be: Guid, string (for enum-like), DateTimeOffset, int, or bool.
    /// </summary>
    [Fact]
    [Trait("Category", "Contract")]
    public void AllExternalDomainEvents_MustContainOnlyIdFields()
    {
        var allowedTypes = new HashSet<Type>
        {
            typeof(Guid), typeof(string), typeof(DateTimeOffset),
            typeof(int), typeof(bool), typeof(long),
        };

        var domainAssemblies = new[]
        {
            typeof(GSDT.Notifications.Domain.Events.NotificationSentEvent).Assembly,
            typeof(GSDT.Cases.Domain.Events.CaseCreatedEvent).Assembly,
            typeof(GSDT.Files.Domain.Events.FileUploadedEvent).Assembly,
            typeof(GSDT.Workflow.Domain.Events.SlaBreachedEvent).Assembly,
        };

        var eventTypes = domainAssemblies
            .SelectMany(a => a.GetTypes())
            .Where(t => t is { IsAbstract: false, IsInterface: false }
                        && typeof(IExternalDomainEvent).IsAssignableFrom(t));

        foreach (var eventType in eventTypes)
        {
            var properties = eventType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (var prop in properties)
            {
                allowedTypes.Should().Contain(prop.PropertyType,
                    $"IExternalDomainEvent '{eventType.Name}.{prop.Name}' has type '{prop.PropertyType.Name}' " +
                    "which may contain PII. External events must use IDs only.");
            }
        }
    }
}
