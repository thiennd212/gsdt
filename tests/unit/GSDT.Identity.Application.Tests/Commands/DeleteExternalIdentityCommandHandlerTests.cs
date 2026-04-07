using GSDT.Identity.Application.Commands.DeleteExternalIdentity;
using GSDT.Identity.Domain.Entities;
using GSDT.Identity.Domain.Repositories;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace GSDT.Identity.Application.Tests.Commands;

/// <summary>
/// Tests for DeleteExternalIdentityCommandHandler.
/// Verifies: success, not found error.
/// </summary>
public sealed class DeleteExternalIdentityCommandHandlerTests
{
    private readonly IExternalIdentityRepository _repo = Substitute.For<IExternalIdentityRepository>();
    private readonly DeleteExternalIdentityCommandHandler _sut;

    private static readonly Guid ExternalIdentityId = Guid.NewGuid();
    private static readonly Guid UserId = Guid.NewGuid();
    private static readonly Guid ActorId = Guid.NewGuid();

    public DeleteExternalIdentityCommandHandlerTests()
    {
        _sut = new DeleteExternalIdentityCommandHandler(_repo);
    }

    [Fact]
    public async Task Handle_ValidCommand_ReturnsSuccess()
    {
        // Arrange
        var cmd = new DeleteExternalIdentityCommand(ExternalIdentityId, ActorId);

        var entity = ExternalIdentity.Create(
            UserId,
            ExternalIdentityProvider.OAuth,
            "google-user-123",
            "John Doe",
            "john@example.com",
            ActorId);

        _repo.GetByIdAsync(ExternalIdentityId, Arg.Any<CancellationToken>()).Returns(entity);

        // Act
        var result = await _sut.Handle(cmd, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _repo.Received(1).DeleteAsync(Arg.Any<ExternalIdentity>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_NotFound_ReturnsFailResult()
    {
        // Arrange
        var cmd = new DeleteExternalIdentityCommand(ExternalIdentityId, ActorId);

        _repo.GetByIdAsync(ExternalIdentityId, Arg.Any<CancellationToken>()).Returns((ExternalIdentity?)null);

        // Act
        var result = await _sut.Handle(cmd, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e.Message.Contains("not found"));
    }

    [Fact]
    public async Task Handle_InvokesRepositoryDelete()
    {
        // Arrange
        var cmd = new DeleteExternalIdentityCommand(ExternalIdentityId, ActorId);

        var entity = ExternalIdentity.Create(
            UserId,
            ExternalIdentityProvider.VNeID,
            "octocat",
            "The Octocat",
            null,
            ActorId);

        _repo.GetByIdAsync(ExternalIdentityId, Arg.Any<CancellationToken>()).Returns(entity);

        // Act
        await _sut.Handle(cmd, CancellationToken.None);

        // Assert
        await _repo.Received(1).DeleteAsync(entity, Arg.Any<CancellationToken>());
    }
}
