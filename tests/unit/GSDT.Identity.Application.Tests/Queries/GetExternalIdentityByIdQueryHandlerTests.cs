using GSDT.Identity.Application.DTOs;
using GSDT.Identity.Application.Queries.GetExternalIdentityById;
using GSDT.Identity.Domain.Entities;
using GSDT.Identity.Domain.Repositories;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace GSDT.Identity.Application.Tests.Queries;

/// <summary>
/// Tests for GetExternalIdentityByIdQueryHandler.
/// Verifies: found, not found scenarios.
/// </summary>
public sealed class GetExternalIdentityByIdQueryHandlerTests
{
    private readonly IExternalIdentityRepository _repo = Substitute.For<IExternalIdentityRepository>();
    private readonly GetExternalIdentityByIdQueryHandler _sut;

    private static readonly Guid ExternalIdentityId = Guid.NewGuid();
    private static readonly Guid UserId = Guid.NewGuid();
    private static readonly Guid ActorId = Guid.NewGuid();

    public GetExternalIdentityByIdQueryHandlerTests()
    {
        _sut = new GetExternalIdentityByIdQueryHandler(_repo);
    }

    [Fact]
    public async Task Handle_ExternalIdentityExists_ReturnsDto()
    {
        // Arrange
        var query = new GetExternalIdentityByIdQuery(ExternalIdentityId);

        var entity = ExternalIdentity.Create(
            UserId,
            ExternalIdentityProvider.OAuth,
            "google-user-123",
            "John Doe",
            "john@example.com",
            ActorId);

        _repo.GetByIdAsync(ExternalIdentityId, Arg.Any<CancellationToken>()).Returns(entity);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var dto = result.Value;
        dto.Id.Should().Be(entity.Id);
        dto.UserId.Should().Be(UserId);
        dto.Provider.Should().Be(ExternalIdentityProvider.OAuth);
        dto.ExternalId.Should().Be("google-user-123");
        dto.DisplayName.Should().Be("John Doe");
        dto.Email.Should().Be("john@example.com");
    }

    [Fact]
    public async Task Handle_ExternalIdentityNotFound_ReturnsFailResult()
    {
        // Arrange
        var query = new GetExternalIdentityByIdQuery(ExternalIdentityId);

        _repo.GetByIdAsync(ExternalIdentityId, Arg.Any<CancellationToken>()).Returns((ExternalIdentity?)null);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e.Message.Contains("not found"));
    }

    [Fact]
    public async Task Handle_ReturnsMappedDto()
    {
        // Arrange
        var query = new GetExternalIdentityByIdQuery(ExternalIdentityId);

        var entity = ExternalIdentity.Create(
            UserId,
            ExternalIdentityProvider.SSO,
            "octocat",
            "The Octocat",
            null,
            ActorId);

        _repo.GetByIdAsync(ExternalIdentityId, Arg.Any<CancellationToken>()).Returns(entity);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var dto = result.Value;
        dto.Should().BeOfType<ExternalIdentityDto>();
        dto.Id.Should().Be(entity.Id);
        dto.Provider.Should().Be(ExternalIdentityProvider.SSO);
        dto.ExternalId.Should().Be("octocat");
        dto.IsActive.Should().Be(entity.IsActive);
    }
}
