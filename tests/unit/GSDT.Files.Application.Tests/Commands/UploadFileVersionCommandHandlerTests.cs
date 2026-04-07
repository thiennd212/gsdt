using GSDT.Files.Application.Commands.UploadFileVersion;
using GSDT.Files.Application.DTOs;
using GSDT.Files.Domain.Entities;
using GSDT.SharedKernel.Application.Data;
using GSDT.SharedKernel.Domain.Repositories;
using FluentResults;
using NSubstitute;

namespace GSDT.Files.Application.Tests.Commands;

/// <summary>
/// Unit tests for UploadFileVersionCommandHandler.
/// Validates: success version creation, version numbering, persistence.
/// </summary>
public sealed class UploadFileVersionCommandHandlerTests
{
    private readonly IRepository<FileVersion, Guid> _repository;
    private readonly IReadDbConnection _db;
    private readonly UploadFileVersionCommandHandler _handler;

    private static readonly Guid TenantId = Guid.NewGuid();
    private static readonly Guid FileRecordId = Guid.NewGuid();
    private static readonly Guid UploadedBy = Guid.NewGuid();

    public UploadFileVersionCommandHandlerTests()
    {
        _repository = Substitute.For<IRepository<FileVersion, Guid>>();
        _db = Substitute.For<IReadDbConnection>();
        _handler = new UploadFileVersionCommandHandler(_repository, _db);
    }

    // --- Success path ---

    [Fact]
    public async Task Handle_ValidCommand_ReturnsSuccessWithDto()
    {
        _db.QuerySingleAsync<int>(
                Arg.Any<string>(),
                Arg.Any<object>(),
                Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(1)); // Next version is 1

        var command = new UploadFileVersionCommand(
            FileRecordId: FileRecordId,
            StoragePath: "/storage/file-v1.pdf",
            FileSize: 1024000,
            ContentHash: "abc123def456",
            UploadedBy: UploadedBy,
            TenantId: TenantId,
            Comment: "Initial upload");

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.VersionNumber.Should().Be(1);
    }

    [Fact]
    public async Task Handle_CalculatesNextVersionNumber()
    {
        _db.QuerySingleAsync<int>(
                Arg.Any<string>(),
                Arg.Any<object>(),
                Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(3)); // Next version is 3

        var command = new UploadFileVersionCommand(
            FileRecordId: FileRecordId,
            StoragePath: "/storage/file-v3.pdf",
            FileSize: 2048000,
            ContentHash: "hash-v3",
            UploadedBy: UploadedBy,
            TenantId: TenantId,
            Comment: "Version 3 update");

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Value.VersionNumber.Should().Be(3);
    }

    [Fact]
    public async Task Handle_ValidCommand_PersistsViaRepository()
    {
        _db.QuerySingleAsync<int>(
                Arg.Any<string>(),
                Arg.Any<object>(),
                Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(1));

        var command = new UploadFileVersionCommand(
            FileRecordId: FileRecordId,
            StoragePath: "/storage/file-v1.pdf",
            FileSize: 1024000,
            ContentHash: "hash123",
            UploadedBy: UploadedBy,
            TenantId: TenantId,
            Comment: null);

        await _handler.Handle(command, CancellationToken.None);

        await _repository.Received(1).AddAsync(
            Arg.Is<FileVersion>(v => v.VersionNumber == 1 && v.FileRecordId == FileRecordId),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_MapsAllProperties()
    {
        _db.QuerySingleAsync<int>(
                Arg.Any<string>(),
                Arg.Any<object>(),
                Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(2));

        var storagePath = "/storage/document-v2.pdf";
        var fileSize = 2048000L;
        var contentHash = "sha256-hash-value";
        var comment = "Updated document";

        var command = new UploadFileVersionCommand(
            FileRecordId: FileRecordId,
            StoragePath: storagePath,
            FileSize: fileSize,
            ContentHash: contentHash,
            UploadedBy: UploadedBy,
            TenantId: TenantId,
            Comment: comment);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Value.FileRecordId.Should().Be(FileRecordId);
        result.Value.StoragePath.Should().Be(storagePath);
        result.Value.FileSize.Should().Be(fileSize);
        result.Value.ContentHash.Should().Be(contentHash);
        result.Value.UploadedBy.Should().Be(UploadedBy);
        result.Value.TenantId.Should().Be(TenantId);
        result.Value.Comment.Should().Be(comment);
    }

    [Fact]
    public async Task Handle_WithoutComment_CreatesVersionSuccessfully()
    {
        _db.QuerySingleAsync<int>(
                Arg.Any<string>(),
                Arg.Any<object>(),
                Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(1));

        var command = new UploadFileVersionCommand(
            FileRecordId: FileRecordId,
            StoragePath: "/storage/file-v1.pdf",
            FileSize: 1024000,
            ContentHash: "hash",
            UploadedBy: UploadedBy,
            TenantId: TenantId,
            Comment: null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Comment.Should().BeNull();
    }

    [Fact]
    public async Task Handle_SetsUploadedAtTimestamp()
    {
        _db.QuerySingleAsync<int>(
                Arg.Any<string>(),
                Arg.Any<object>(),
                Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(1));

        var command = new UploadFileVersionCommand(
            FileRecordId: FileRecordId,
            StoragePath: "/storage/file-v1.pdf",
            FileSize: 1024000,
            ContentHash: "hash",
            UploadedBy: UploadedBy,
            TenantId: TenantId,
            Comment: null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Value.UploadedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    // --- Failure scenarios ---

    [Fact]
    public async Task Handle_RepositoryException_Propagates()
    {
        _db.QuerySingleAsync<int>(
                Arg.Any<string>(),
                Arg.Any<object>(),
                Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(1));

        _repository
            .AddAsync(Arg.Any<FileVersion>(), Arg.Any<CancellationToken>())
            .Returns<Task>(_ => Task.FromException(new InvalidOperationException("DB error")));

        var command = new UploadFileVersionCommand(
            FileRecordId: FileRecordId,
            StoragePath: "/storage/file-v1.pdf",
            FileSize: 1024000,
            ContentHash: "hash",
            UploadedBy: UploadedBy,
            TenantId: TenantId,
            Comment: null);

        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*DB error*");
    }

    [Fact]
    public async Task Handle_VersionNumberQueryException_Propagates()
    {
        _db.QuerySingleAsync<int>(
                Arg.Any<string>(),
                Arg.Any<object>(),
                Arg.Any<CancellationToken>())
            .Returns(Task.FromException<int>(
                new InvalidOperationException("Query failed")));

        var command = new UploadFileVersionCommand(
            FileRecordId: FileRecordId,
            StoragePath: "/storage/file-v1.pdf",
            FileSize: 1024000,
            ContentHash: "hash",
            UploadedBy: UploadedBy,
            TenantId: TenantId,
            Comment: null);

        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*Query failed*");
    }
}
