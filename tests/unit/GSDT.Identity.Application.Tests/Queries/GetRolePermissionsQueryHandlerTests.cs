using GSDT.Identity.Application.Queries.GetPermissions;
using GSDT.Identity.Application.Queries.GetRolePermissions;
using GSDT.SharedKernel.Application.Data;
using NSubstitute;
using Xunit;
using FluentAssertions;

namespace GSDT.Identity.Application.Tests.Queries;

/// <summary>
/// Handler tests for GetRolePermissionsQuery.
/// Mocks IReadDbConnection — verifies existing role returns permissions, and empty result scenario.
/// </summary>
public sealed class GetRolePermissionsQueryHandlerTests
{
    private readonly IReadDbConnection _db;
    private readonly GetRolePermissionsQueryHandler _sut;

    public GetRolePermissionsQueryHandlerTests()
    {
        _db = Substitute.For<IReadDbConnection>();
        _sut = new GetRolePermissionsQueryHandler(_db);
    }

    [Fact]
    public async Task ExistingRole_ReturnsPermissions()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        var permId = Guid.NewGuid();

        var rows = new List<PermissionRow>
        {
            new()
            {
                Id           = permId,
                Code         = "HOSO.HOSO.APPROVE",
                Name         = "Phê duyệt hồ sơ",
                Description  = "Quyền phê duyệt",
                ModuleCode   = "HOSO",
                ResourceCode = "HOSO",
                ActionCode   = "APPROVE"
            }
        };

        _db.QueryAsync<PermissionRow>(
                Arg.Any<string>(),
                Arg.Any<object?>(),
                Arg.Any<CancellationToken>())
            .Returns(rows);

        // Act
        var result = await _sut.Handle(new GetRolePermissionsQuery(roleId), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
        result.Value[0].Id.Should().Be(permId);
        result.Value[0].Code.Should().Be("HOSO.HOSO.APPROVE");
        result.Value[0].ModuleCode.Should().Be("HOSO");
    }

    [Fact]
    public async Task EmptyResult_ReturnsSuccessWithEmptyList()
    {
        // Arrange
        var roleId = Guid.NewGuid();

        _db.QueryAsync<PermissionRow>(
                Arg.Any<string>(),
                Arg.Any<object?>(),
                Arg.Any<CancellationToken>())
            .Returns(new List<PermissionRow>());

        // Act
        var result = await _sut.Handle(new GetRolePermissionsQuery(roleId), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }
}
