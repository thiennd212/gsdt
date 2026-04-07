using System.Text.Json;
using GSDT.Files.Domain.Entities;
using GSDT.Files.Domain.Events;
using FluentAssertions;

namespace GSDT.Tests.Contracts.Events;

/// <summary>
/// Contract tests for Files module domain events — serialization round-trip stability.
/// FileId is a strongly-typed value object (record struct with Guid Value).
/// </summary>
public sealed class FileEventsContractTests
{
    [Fact]
    [Trait("Category", "Contract")]
    public void FileUploadedEvent_RoundTrip()
    {
        var evt = new FileUploadedEvent(
            FileId.From(Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa")),
            Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
            "document.pdf",
            1024 * 1024);

        var json = JsonSerializer.Serialize(evt);
        var d = JsonSerializer.Deserialize<FileUploadedEvent>(json);

        d.Should().NotBeNull();
        d!.FileId.Value.Should().Be(evt.FileId.Value);
        d.TenantId.Should().Be(evt.TenantId);
        d.OriginalFileName.Should().Be("document.pdf");
        d.SizeBytes.Should().Be(1024 * 1024);
    }

    [Fact]
    [Trait("Category", "Contract")]
    public void FileQuarantinedEvent_RoundTrip()
    {
        var evt = new FileQuarantinedEvent(
            FileId.From(Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc")),
            Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd"),
            "malware.exe");

        var json = JsonSerializer.Serialize(evt);
        var d = JsonSerializer.Deserialize<FileQuarantinedEvent>(json);

        d.Should().NotBeNull();
        d!.FileId.Value.Should().Be(evt.FileId.Value);
        d.OriginalFileName.Should().Be("malware.exe");
    }

    [Fact]
    [Trait("Category", "Contract")]
    public void FileUploadedEvent_JsonShape_ContainsExpectedProperties()
    {
        var evt = new FileUploadedEvent(FileId.New(), Guid.NewGuid(), "test.txt", 100);
        var json = JsonSerializer.Serialize(evt);
        var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        root.TryGetProperty("FileId", out _).Should().BeTrue();
        root.TryGetProperty("TenantId", out _).Should().BeTrue();
        root.TryGetProperty("OriginalFileName", out _).Should().BeTrue();
        root.TryGetProperty("SizeBytes", out _).Should().BeTrue();
        root.TryGetProperty("EventId", out _).Should().BeTrue();
        root.TryGetProperty("OccurredAt", out _).Should().BeTrue();
    }
}
