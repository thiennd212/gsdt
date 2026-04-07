using GSDT.MasterData.Commands.PublishDictionary;
using GSDT.MasterData.Persistence;
using GSDT.SharedKernel.Application;
using GSDT.SharedKernel.Domain;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Xunit;

namespace GSDT.MasterData.Tests.Commands;

/// <summary>
/// Tests for PublishDictionaryCommandHandler.
/// Verifies handler structure and dependencies.
/// </summary>
public sealed class PublishDictionaryCommandHandlerTests
{
    private static readonly Guid DictionaryId = Guid.NewGuid();
    private static readonly Guid ActorId = Guid.NewGuid();

    [Fact]
    public void Handler_HasCorrectDependencies()
    {
        // Arrange & Act
        var mockDbContext = Substitute.For<MasterDataDbContext>(
            new DbContextOptions<MasterDataDbContext>(),
            Substitute.For<ITenantContext>());
        var mockBus = Substitute.For<IMessageBus>();

        var handler = new PublishDictionaryCommandHandler(mockDbContext, mockBus);

        // Assert
        handler.Should().NotBeNull();
    }

    [Fact]
    public void ValidCommand_CanBeCreated()
    {
        // Arrange & Act
        var cmd = new PublishDictionaryCommand(DictionaryId, "Publishing notes", ActorId);

        // Assert
        cmd.Id.Should().Be(DictionaryId);
        cmd.ActorId.Should().Be(ActorId);
        cmd.Notes.Should().Be("Publishing notes");
    }
}
