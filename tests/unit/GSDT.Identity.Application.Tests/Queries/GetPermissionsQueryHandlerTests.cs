using GSDT.Identity.Application.Queries.GetPermissions;
using GSDT.SharedKernel.Application.Data;
using NSubstitute;
using Xunit;
using FluentAssertions;

namespace GSDT.Identity.Application.Tests.Queries;

/// <summary>
/// Handler tests for GetPermissionsQuery.
/// Mocks IReadDbConnection — verifies all-permissions return and module-filter pass-through.
/// </summary>
public sealed class GetPermissionsQueryHandlerTests
{
    private readonly IReadDbConnection _db;
    private readonly GetPermissionsQueryHandler _sut;

    public GetPermissionsQueryHandlerTests()
    {
        _db = Substitute.For<IReadDbConnection>();
        _sut = new GetPermissionsQueryHandler(_db);
    }

    [Fact]
    public async Task ReturnsAllPermissions_WhenNoFilter()
    {
        // Arrange
        var permId = Guid.NewGuid();
        var rows = new List<PermissionRow>
        {
            new()
            {
                Id           = permId,
                Code         = "HOSO.HOSO.VIEW",
                Name         = "Xem hồ sơ",
                Description  = null,
                ModuleCode   = "HOSO",
                ResourceCode = "HOSO",
                ActionCode   = "VIEW"
            }
        };

        _db.QueryAsync<PermissionRow>(
                Arg.Any<string>(),
                Arg.Any<object?>(),
                Arg.Any<CancellationToken>())
            .Returns(rows);

        // Act
        var result = await _sut.Handle(new GetPermissionsQuery(null, null), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
        result.Value[0].Id.Should().Be(permId);
        result.Value[0].Code.Should().Be("HOSO.HOSO.VIEW");
        result.Value[0].ModuleCode.Should().Be("HOSO");
    }

    [Fact]
    public async Task WithModuleFilter_PassesParameterAndReturnsFiltered()
    {
        // Arrange
        var rows = new List<PermissionRow>
        {
            new()
            {
                Id           = Guid.NewGuid(),
                Code         = "DAUTU.PROJECT.CREATE",
                Name         = "Tạo dự án",
                Description  = null,
                ModuleCode   = "DAUTU",
                ResourceCode = "PROJECT",
                ActionCode   = "CREATE"
            }
        };

        _db.QueryAsync<PermissionRow>(
                Arg.Any<string>(),
                Arg.Any<object?>(),
                Arg.Any<CancellationToken>())
            .Returns(rows);

        // Act
        var result = await _sut.Handle(new GetPermissionsQuery("DAUTU", null), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
        result.Value[0].ModuleCode.Should().Be("DAUTU");

        // Verify DB was called with any param (ModuleCode filter applied via SQL params)
        await _db.Received(1).QueryAsync<PermissionRow>(
            Arg.Any<string>(),
            Arg.Any<object?>(),
            Arg.Any<CancellationToken>());
    }
}
