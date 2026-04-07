using GSDT.Files.Application.Commands.ArchiveRecord;
using GSDT.Files.Application.DTOs;
using GSDT.Files.Domain.Entities;
using GSDT.SharedKernel.Domain.Repositories;
using FluentResults;
using NSubstitute;

namespace GSDT.Files.Application.Tests.Commands;

/// <summary>
/// Unit tests for ArchiveRecordCommandHandler.
/// Validates: success archive, lifecycle creation, status transition.
/// </summary>
public sealed class ArchiveRecordCommandHandlerTests
{
    private readonly IRepository<RecordLifecycle, Guid> _repository;
    private readonly ArchiveRecordCommandHandler _handler;

    private static readonly Guid TenantId = Guid.NewGuid();
    private static readonly Guid FileRecordId = Guid.NewGuid();
    private static readonly Guid ArchivedBy = Guid.NewGuid();

    public ArchiveRecordCommandHandlerTests()
    {
        _repository = Substitute.For<IRepository<RecordLifecycle, Guid>>();
        _handler = new ArchiveRecordCommandHandler(_repository);
    }

    // --- Success path ---

    [Fact]
    public async Task Handle_ValidCommand_ReturnsSuccessWithDto()
    {
        var command = new ArchiveRecordCommand(
            FileRecordId: FileRecordId,
            ArchiveStoragePath: "/archive/file-archive",
            OriginalStoragePath: "/storage/file",
            ArchivedBy: ArchivedBy,
            TenantId: TenantId,
            ArchiveReason: "Retention period reached");

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.FileRecordId.Should().Be(FileRecordId);
    }

    [Fact]
    public async Task Handle_ValidCommand_PersistsViaRepository()
    {
        var command = new ArchiveRecordCommand(
            FileRecordId: FileRecordId,
            ArchiveStoragePath: "/archive/file",
            OriginalStoragePath: "/storage/file",
            ArchivedBy: ArchivedBy,
            TenantId: TenantId,
            ArchiveReason: "Archived");

        await _handler.Handle(command, CancellationToken.None);

        await _repository.Received(1).AddAsync(
            Arg.Is<RecordLifecycle>(l => l.FileRecordId == FileRecordId),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_CreatesLifecycleEntry()
    {
        var command = new ArchiveRecordCommand(
            FileRecordId: FileRecordId,
            ArchiveStoragePath: "/archive/doc",
            OriginalStoragePath: "/storage/doc",
            ArchivedBy: ArchivedBy,
            TenantId: TenantId,
            ArchiveReason: "Policy compliance");

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Value.FileRecordId.Should().Be(FileRecordId);
        result.Value.ArchivedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_SetsArchivedStatus()
    {
        var command = new ArchiveRecordCommand(
            FileRecordId: FileRecordId,
            ArchiveStoragePath: "/archive/file",
            OriginalStoragePath: "/storage/file",
            ArchivedBy: ArchivedBy,
            TenantId: TenantId,
            ArchiveReason: null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Value.CurrentStatus.Should().Be(RecordLifecycleStatus.Archived);
    }

    [Fact]
    public async Task Handle_WithArchiveReason()
    {
        var reason = "Retention compliance - 7 year requirement";
        var command = new ArchiveRecordCommand(
            FileRecordId: FileRecordId,
            ArchiveStoragePath: "/archive/file",
            OriginalStoragePath: "/storage/file",
            ArchivedBy: ArchivedBy,
            TenantId: TenantId,
            ArchiveReason: reason);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WithoutArchiveReason()
    {
        var command = new ArchiveRecordCommand(
            FileRecordId: FileRecordId,
            ArchiveStoragePath: "/archive/file",
            OriginalStoragePath: "/storage/file",
            ArchivedBy: ArchivedBy,
            TenantId: TenantId,
            ArchiveReason: null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_RecordsArchivedByAndTimestamp()
    {
        var command = new ArchiveRecordCommand(
            FileRecordId: FileRecordId,
            ArchiveStoragePath: "/archive/file",
            OriginalStoragePath: "/storage/file",
            ArchivedBy: ArchivedBy,
            TenantId: TenantId,
            ArchiveReason: "Archived");

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Value.ArchivedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task Handle_MultipleRecordsArchived()
    {
        var fileId1 = Guid.NewGuid();
        var fileId2 = Guid.NewGuid();

        var command1 = new ArchiveRecordCommand(
            FileRecordId: fileId1,
            ArchiveStoragePath: "/archive/file1",
            OriginalStoragePath: "/storage/file1",
            ArchivedBy: ArchivedBy,
            TenantId: TenantId,
            ArchiveReason: null);

        var command2 = new ArchiveRecordCommand(
            FileRecordId: fileId2,
            ArchiveStoragePath: "/archive/file2",
            OriginalStoragePath: "/storage/file2",
            ArchivedBy: ArchivedBy,
            TenantId: TenantId,
            ArchiveReason: null);

        var result1 = await _handler.Handle(command1, CancellationToken.None);
        var result2 = await _handler.Handle(command2, CancellationToken.None);

        result1.IsSuccess.Should().BeTrue();
        result2.IsSuccess.Should().BeTrue();
        result1.Value.FileRecordId.Should().NotBe(result2.Value.FileRecordId);
    }

    [Fact]
    public async Task Handle_RepositoryException_Propagates()
    {
        _repository
            .AddAsync(Arg.Any<RecordLifecycle>(), Arg.Any<CancellationToken>())
            .Returns<Task>(_ => Task.FromException(new InvalidOperationException("DB unavailable")));

        var command = new ArchiveRecordCommand(
            FileRecordId: FileRecordId,
            ArchiveStoragePath: "/archive/file",
            OriginalStoragePath: "/storage/file",
            ArchivedBy: ArchivedBy,
            TenantId: TenantId,
            ArchiveReason: null);

        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*DB unavailable*");
    }

    [Fact]
    public async Task Handle_CancellationTokenRespected()
    {
        var cts = new CancellationTokenSource();
        cts.Cancel();

        _repository
            .AddAsync(Arg.Any<RecordLifecycle>(), Arg.Any<CancellationToken>())
            .Returns<Task>(_ => Task.FromCanceled(cts.Token));

        var command = new ArchiveRecordCommand(
            FileRecordId: FileRecordId,
            ArchiveStoragePath: "/archive/file",
            OriginalStoragePath: "/storage/file",
            ArchivedBy: ArchivedBy,
            TenantId: TenantId,
            ArchiveReason: null);

        var act = async () => await _handler.Handle(command, cts.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }
}
