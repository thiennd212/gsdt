using GSDT.MasterData.Commands.CreateDictionaryItem;
using GSDT.MasterData.Persistence;
using GSDT.SharedKernel.Application;
using GSDT.SharedKernel.Domain;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Xunit;

namespace GSDT.MasterData.Tests.Commands;

/// <summary>
/// Tests for CreateDictionaryItemCommandHandler.
/// Verifies handler structure and command validation.
/// </summary>
public sealed class CreateDictionaryItemCommandHandlerTests
{
    private static readonly Guid DictionaryId = Guid.NewGuid();
    private static readonly Guid TenantId = Guid.NewGuid();
    private static readonly Guid ActorId = Guid.NewGuid();

    [Fact]
    public void Handler_HasCorrectDependencies()
    {
        // Arrange & Act
        var mockDbContext = Substitute.For<MasterDataDbContext>(
            new DbContextOptions<MasterDataDbContext>(),
            Substitute.For<ITenantContext>());

        var handler = new CreateDictionaryItemCommandHandler(mockDbContext);

        // Assert
        handler.Should().NotBeNull();
    }

    [Fact]
    public void ValidCommand_CanBeCreated()
    {
        // Arrange & Act
        var cmd = new CreateDictionaryItemCommand(
            DictionaryId,
            "ITEM_CODE",
            "Item Name",
            "Tên Mục",
            null,
            1,
            DateTime.UtcNow,
            null,
            null,
            TenantId,
            ActorId);

        // Assert
        cmd.DictionaryId.Should().Be(DictionaryId);
        cmd.Code.Should().Be("ITEM_CODE");
        cmd.Name.Should().Be("Item Name");
        cmd.NameVi.Should().Be("Tên Mục");
        cmd.SortOrder.Should().Be(1);
    }

    [Fact]
    public void CommandWithParent_CanBeCreated()
    {
        // Arrange
        var parentId = Guid.NewGuid();

        // Act
        var cmd = new CreateDictionaryItemCommand(
            DictionaryId,
            "CHILD_CODE",
            "Child Item",
            "Mục Con",
            parentId,
            2,
            DateTime.UtcNow,
            null,
            null,
            TenantId,
            ActorId);

        // Assert
        cmd.ParentId.Should().Be(parentId);
        cmd.Code.Should().Be("CHILD_CODE");
    }
}
