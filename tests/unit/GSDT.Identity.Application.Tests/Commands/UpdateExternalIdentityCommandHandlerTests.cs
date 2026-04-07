using GSDT.Identity.Application.Commands.UpdateExternalIdentity;
using GSDT.Identity.Domain.Entities;
using GSDT.Identity.Domain.Repositories;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace GSDT.Identity.Application.Tests.Commands;

/// <summary>
/// Tests for UpdateExternalIdentityCommandHandler.
/// Verifies: success, not found error.
/// </summary>
public sealed class UpdateExternalIdentityCommandHandlerTests
{
    private readonly IExternalIdentityRepository _repo = Substitute.For<IExternalIdentityRepository>();
    private readonly UpdateExternalIdentityCommandHandler _sut;

    private static readonly Guid ExternalIdentityId = Guid.NewGuid();
    private static readonly Guid UserId = Guid.NewGuid();
    private static readonly Guid ActorId = Guid.NewGuid();

    public UpdateExternalIdentityCommandHandlerTests()
    {
        _sut = new UpdateExternalIdentityCommandHandler(_repo);
    }

    [Fact]
    public async Task Handle_ValidCommand_ReturnsSuccess()
    {
        // Arrange
        var cmd = new UpdateExternalIdentityCommand(
            ExternalIdentityId,
            "Updated Name",
            "updated@example.com",
            null,
            ActorId);

        var entity = ExternalIdentity.Create(
            UserId,
            ExternalIdentityProvider.OAuth,
            "google-user-123",
            "Old Name",
            "old@example.com",
            ActorId);

        _repo.GetByIdAsync(ExternalIdentityId, Arg.Any<CancellationToken>()).Returns(entity);

        // Act
        var result = await _sut.Handle(cmd, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _repo.Received(1).UpdateAsync(Arg.Any<ExternalIdentity>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_NotFound_ReturnsFailResult()
    {
        // Arrange
        var cmd = new UpdateExternalIdentityCommand(
            ExternalIdentityId,
            "Updated Name",
            "updated@example.com",
            null,
            ActorId);

        _repo.GetByIdAsync(ExternalIdentityId, Arg.Any<CancellationToken>()).Returns((ExternalIdentity?)null);

        // Act
        var result = await _sut.Handle(cmd, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e.Message.Contains("not found"));
    }

    [Fact]
    public async Task Handle_UpdatesDisplayName()
    {
        // Arrange
        var newDisplayName = "New Display Name";
        var cmd = new UpdateExternalIdentityCommand(
            ExternalIdentityId,
            newDisplayName,
            "test@example.com",
            null,
            ActorId);

        var entity = ExternalIdentity.Create(
            UserId,
            ExternalIdentityProvider.SSO,
            "github-user",
            "Old Name",
            "old@example.com",
            ActorId);

        _repo.GetByIdAsync(ExternalIdentityId, Arg.Any<CancellationToken>()).Returns(entity);

        // Act
        await _sut.Handle(cmd, CancellationToken.None);

        // Assert
        await _repo.Received(1).UpdateAsync(
            Arg.Is<ExternalIdentity>(e => e.DisplayName == newDisplayName),
            Arg.Any<CancellationToken>());
    }
}
