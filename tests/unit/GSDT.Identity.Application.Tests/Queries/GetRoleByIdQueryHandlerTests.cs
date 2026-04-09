using GSDT.Identity.Application.Queries.GetRoleById;
using GSDT.SharedKernel.Application.Data;
using NSubstitute;
using Xunit;
using FluentAssertions;

namespace GSDT.Identity.Application.Tests.Queries;

/// <summary>
/// Handler tests for GetRoleByIdQuery.
/// Mocks IReadDbConnection at the <c>QueryAsync&lt;RoleDetailRow&gt;</c> level — the row type
/// is exposed as an <c>internal</c> class from the handler's namespace so tests can reference it.
/// Verifies: existing role returns detail with permissions, non-existent role returns fail.
/// </summary>
public sealed class GetRoleByIdQueryHandlerTests
{
    private readonly IReadDbConnection _db;
    private readonly GetRoleByIdQueryHandler _sut;

    public GetRoleByIdQueryHandlerTests()
    {
        _db = Substitute.For<IReadDbConnection>();
        _sut = new GetRoleByIdQueryHandler(_db);
    }

    [Fact]
    public async Task ExistingRole_ReturnsDetailWithPermissions()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        var permId = Guid.NewGuid();

        var rows = new List<RoleDetailRow>
        {
            new()
            {
                Id = roleId,
                Code = "CHUYEN_VIEN",
                Name = "Chuyên viên xử lý",
                Description = "Mô tả",
                RoleType = "Business",
                IsActive = true,
                PermissionId = permId,
                PermissionCode = "HOSO.HOSO.VIEW",
                PermissionName = "Xem hồ sơ"
            }
        };

        _db.QueryAsync<RoleDetailRow>(
                Arg.Any<string>(),
                Arg.Any<object?>(),
                Arg.Any<CancellationToken>())
            .Returns(rows);

        // Act
        var result = await _sut.Handle(new GetRoleByIdQuery(roleId), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var dto = result.Value;
        dto.Id.Should().Be(roleId);
        dto.Code.Should().Be("CHUYEN_VIEN");
        dto.Name.Should().Be("Chuyên viên xử lý");
        dto.RoleType.Should().Be("Business");
        dto.IsActive.Should().BeTrue();
        dto.Permissions.Should().HaveCount(1);
        dto.Permissions[0].PermissionId.Should().Be(permId);
        dto.Permissions[0].Code.Should().Be("HOSO.HOSO.VIEW");
    }

    [Fact]
    public async Task NonExistentRole_ReturnsFail()
    {
        // Arrange
        var roleId = Guid.NewGuid();

        _db.QueryAsync<RoleDetailRow>(
                Arg.Any<string>(),
                Arg.Any<object?>(),
                Arg.Any<CancellationToken>())
            .Returns(new List<RoleDetailRow>());

        // Act
        var result = await _sut.Handle(new GetRoleByIdQuery(roleId), CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e.Message.Contains(roleId.ToString()));
    }
}
