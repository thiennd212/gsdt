using GSDT.Files.Domain.Entities;
using GSDT.Files.Domain.Events;
using GSDT.SharedKernel.Domain;

namespace GSDT.Files.Domain.Tests.Entities;

/// <summary>
/// Unit tests for FileRecord aggregate root.
/// Covers factory method, two-phase status lifecycle, domain events, and soft-delete.
/// </summary>
public sealed class FileRecordTests
{
    private static readonly Guid TenantId = Guid.NewGuid();
    private static readonly Guid UploadedBy = Guid.NewGuid();
    private static readonly Guid CaseId = Guid.NewGuid();

    // --- Factory ---

    private static FileRecord CreateRecord(Guid? caseId = null) =>
        FileRecord.Create(
            TenantId,
            originalFileName: "document.pdf",
            storageKey: $"{TenantId}/{Guid.NewGuid()}.pdf",
            contentType: "application/pdf",
            sizeBytes: 1024,
            checksumSha256: "abc123",
            uploadedBy: UploadedBy,
            caseId: caseId,
            classification: ClassificationLevel.Internal);

    // --- Create ---

    [Fact]
    public void Create_ValidInput_ReturnsQuarantinedStatus()
    {
        var record = CreateRecord();

        record.Status.Should().Be(FileStatus.Quarantined);
    }

    [Fact]
    public void Create_ValidInput_SetsAllProperties()
    {
        var record = CreateRecord();

        record.TenantId.Should().Be(TenantId);
        record.OriginalFileName.Should().Be("document.pdf");
        record.ContentType.Should().Be("application/pdf");
        record.SizeBytes.Should().Be(1024);
        record.ChecksumSha256.Should().Be("abc123");
        record.UploadedBy.Should().Be(UploadedBy);
        record.ClassificationLevel.Should().Be(ClassificationLevel.Internal);
    }

    [Fact]
    public void Create_ValidInput_GeneratesNewFileId()
    {
        var record = CreateRecord();

        record.Id.Value.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void Create_EachCall_GeneratesDifferentFileId()
    {
        var r1 = CreateRecord();
        var r2 = CreateRecord();

        r1.Id.Should().NotBe(r2.Id);
    }

    [Fact]
    public void Create_WithCaseId_SetsCaseId()
    {
        var record = CreateRecord(caseId: CaseId);

        record.CaseId.Should().Be(CaseId);
    }

    [Fact]
    public void Create_WithoutCaseId_CaseIdIsNull()
    {
        var record = CreateRecord(caseId: null);

        record.CaseId.Should().BeNull();
    }

    [Fact]
    public void Create_SetsCreatedByAuditField()
    {
        var record = CreateRecord();

        record.CreatedBy.Should().Be(UploadedBy);
    }

    [Fact]
    public void Create_NoDomainEventsRaisedOnCreation()
    {
        // FileRecord.Create does not raise any domain event — only MarkAvailable/MarkRejected do
        var record = CreateRecord();

        record.DomainEvents.Should().BeEmpty();
    }

    // --- MarkAvailable ---

    [Fact]
    public void MarkAvailable_FromQuarantined_SetsAvailableStatus()
    {
        var record = CreateRecord();

        record.MarkAvailable();

        record.Status.Should().Be(FileStatus.Available);
    }

    [Fact]
    public void MarkAvailable_RaisesExactlyOneFileUploadedEvent()
    {
        var record = CreateRecord();

        record.MarkAvailable();

        record.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<FileUploadedEvent>();
    }

    [Fact]
    public void MarkAvailable_FileUploadedEvent_HasCorrectPayload()
    {
        var record = CreateRecord();

        record.MarkAvailable();

        var evt = record.DomainEvents.OfType<FileUploadedEvent>().Single();
        evt.FileId.Should().Be(record.Id);
        evt.TenantId.Should().Be(TenantId);
        evt.OriginalFileName.Should().Be("document.pdf");
        evt.SizeBytes.Should().Be(1024);
    }

    // --- MarkRejected ---

    [Fact]
    public void MarkRejected_FromQuarantined_SetsRejectedStatus()
    {
        var record = CreateRecord();

        record.MarkRejected();

        record.Status.Should().Be(FileStatus.Rejected);
    }

    [Fact]
    public void MarkRejected_RaisesExactlyOneFileQuarantinedEvent()
    {
        var record = CreateRecord();

        record.MarkRejected();

        record.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<FileQuarantinedEvent>();
    }

    [Fact]
    public void MarkRejected_FileQuarantinedEvent_HasCorrectPayload()
    {
        var record = CreateRecord();

        record.MarkRejected();

        var evt = record.DomainEvents.OfType<FileQuarantinedEvent>().Single();
        evt.FileId.Should().Be(record.Id);
        evt.TenantId.Should().Be(TenantId);
        evt.OriginalFileName.Should().Be("document.pdf");
    }

    // --- Soft delete ---

    [Fact]
    public void Delete_SoftDeletesRecord()
    {
        var record = CreateRecord();

        record.Delete();

        // ISoftDeletable sets IsDeleted = true via MarkDeleted()
        record.IsDeleted.Should().BeTrue();
    }

    // --- ClearDomainEvents ---

    [Fact]
    public void ClearDomainEvents_AfterMarkAvailable_RemovesAllEvents()
    {
        var record = CreateRecord();
        record.MarkAvailable();
        record.DomainEvents.Should().NotBeEmpty();

        record.ClearDomainEvents();

        record.DomainEvents.Should().BeEmpty();
    }
}
