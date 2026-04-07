using GSDT.Identity.Application.Commands.CreateExternalIdentity;
using GSDT.Identity.Domain.Entities;
using GSDT.Identity.Domain.Repositories;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using NSubstitute;
using Xunit;

namespace GSDT.Identity.Application.Tests.Commands;

/// <summary>
/// Tests for CreateExternalIdentityCommandHandler.
/// Verifies: success, duplicate provider check, user existence validation.
/// </summary>
public sealed class CreateExternalIdentityCommandHandlerTests
{
    private readonly IExternalIdentityRepository _repo = Substitute.For<IExternalIdentityRepository>();
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly CreateExternalIdentityCommandHandler _sut;

    private static readonly Guid UserId = Guid.NewGuid();
    private static readonly Guid ActorId = Guid.NewGuid();

    public CreateExternalIdentityCommandHandlerTests()
    {
        var store = Substitute.For<IUserStore<ApplicationUser>>();
        _userManager = Substitute.For<UserManager<ApplicationUser>>(
            store,
            Substitute.For<Microsoft.Extensions.Options.IOptions<IdentityOptions>>(),
            Substitute.For<IPasswordHasher<ApplicationUser>>(),
            Array.Empty<IUserValidator<ApplicationUser>>(),
            Array.Empty<IPasswordValidator<ApplicationUser>>(),
            Substitute.For<ILookupNormalizer>(),
            Substitute.For<IdentityErrorDescriber>(),
            Substitute.For<IServiceProvider>(),
            Substitute.For<Microsoft.Extensions.Logging.ILogger<UserManager<ApplicationUser>>>());

        _sut = new CreateExternalIdentityCommandHandler(_repo, _userManager);
    }

    [Fact]
    public async Task Handle_ValidCommand_ReturnsSuccess()
    {
        // Arrange
        var cmd = new CreateExternalIdentityCommand(
            UserId,
            ExternalIdentityProvider.OAuth,
            "google-user-123",
            "John Doe",
            "john@example.com",
            ActorId);

        var user = new ApplicationUser { Id = UserId, Email = "john@example.com" };
        _userManager.FindByIdAsync(UserId.ToString()).Returns(user);
        _repo.GetByUserAndProviderAsync(UserId, ExternalIdentityProvider.OAuth, Arg.Any<CancellationToken>())
            .Returns((ExternalIdentity?)null);

        // Act
        var result = await _sut.Handle(cmd, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBe(Guid.Empty);
        await _repo.Received(1).AddAsync(Arg.Any<ExternalIdentity>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_UserNotFound_ReturnsFailResult()
    {
        // Arrange
        var cmd = new CreateExternalIdentityCommand(
            UserId,
            ExternalIdentityProvider.OAuth,
            "google-user-123",
            "John Doe",
            "john@example.com",
            ActorId);

        _userManager.FindByIdAsync(Arg.Any<string>()).Returns((ApplicationUser?)null);

        // Act
        var result = await _sut.Handle(cmd, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e.Message.Contains("not found"));
    }

    [Fact]
    public async Task Handle_DuplicateProvider_ReturnsConflictError()
    {
        // Arrange
        var cmd = new CreateExternalIdentityCommand(
            UserId,
            ExternalIdentityProvider.OAuth,
            "google-user-123",
            "John Doe",
            "john@example.com",
            ActorId);

        var user = new ApplicationUser { Id = UserId, Email = "john@example.com" };
        _userManager.FindByIdAsync(UserId.ToString()).Returns(user);

        var existingIdentity = ExternalIdentity.Create(
            UserId,
            ExternalIdentityProvider.OAuth,
            "google-user-456",
            "Existing User",
            "existing@example.com",
            ActorId);

        _repo.GetByUserAndProviderAsync(UserId, ExternalIdentityProvider.OAuth, Arg.Any<CancellationToken>())
            .Returns(existingIdentity);

        // Act
        var result = await _sut.Handle(cmd, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e.Message.Contains("already has a linked"));
    }

    [Fact]
    public async Task Handle_ValidCommand_InvokesRepositoryAdd()
    {
        // Arrange
        var cmd = new CreateExternalIdentityCommand(
            UserId,
            ExternalIdentityProvider.LDAP,
            "octocat",
            "The Octocat",
            null,
            ActorId);

        var user = new ApplicationUser { Id = UserId, Email = "octocat@github.com" };
        _userManager.FindByIdAsync(UserId.ToString()).Returns(user);
        _repo.GetByUserAndProviderAsync(UserId, ExternalIdentityProvider.LDAP, Arg.Any<CancellationToken>())
            .Returns((ExternalIdentity?)null);

        // Act
        await _sut.Handle(cmd, CancellationToken.None);

        // Assert
        await _repo.Received(1).AddAsync(
            Arg.Is<ExternalIdentity>(e =>
                e.UserId == UserId &&
                e.Provider == ExternalIdentityProvider.LDAP &&
                e.ExternalId == "octocat"),
            Arg.Any<CancellationToken>());
    }
}
