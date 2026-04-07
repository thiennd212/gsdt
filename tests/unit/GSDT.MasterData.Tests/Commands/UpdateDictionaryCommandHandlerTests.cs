using GSDT.MasterData.Commands.UpdateDictionary;
using GSDT.MasterData.Persistence;
using GSDT.SharedKernel.Application;
using GSDT.SharedKernel.Domain;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Xunit;

namespace GSDT.MasterData.Tests.Commands;

/// <summary>
/// Tests for UpdateDictionaryCommandHandler.
/// Verifies handler structure and dependencies.
/// </summary>
public sealed class UpdateDictionaryCommandHandlerTests
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

        var handler = new UpdateDictionaryCommandHandler(mockDbContext);

        // Assert
        handler.Should().NotBeNull();
    }

    [Fact]
    public void ValidCommand_CanBeCreated()
    {
        // Arrange & Act
        var cmd = new UpdateDictionaryCommand(
            DictionaryId,
            "Updated Name",
            "Tên Cập Nhật",
            "Updated description",
            ActorId);

        // Assert
        cmd.Id.Should().Be(DictionaryId);
        cmd.Name.Should().Be("Updated Name");
        cmd.NameVi.Should().Be("Tên Cập Nhật");
        cmd.ActorId.Should().Be(ActorId);
    }
}
