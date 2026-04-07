using GSDT.Identity.Infrastructure.Services;
using GSDT.SharedKernel.Application.Data;
using GSDT.SharedKernel.Contracts.Clients;

namespace GSDT.Identity.Infrastructure.Tests.Services;

/// <summary>
/// Unit tests for InProcessIdentityModuleClient.
/// Mocks IReadDbConnection (Dapper) — no real DB required.
/// Updated for Phase 1: class-based DTOs, tenant params, hard cap.
/// </summary>
public sealed class InProcessIdentityModuleClientTests
{
    private readonly IReadDbConnection _db = Substitute.For<IReadDbConnection>();
    private readonly InProcessIdentityModuleClient _sut;

    public InProcessIdentityModuleClientTests()
    {
        _sut = new InProcessIdentityModuleClient(_db);
    }

    [Fact]
    public async Task GetUserInfoByIdsAsync_EmptyInput_ReturnsEmptyDictionary()
    {
        var result = await _sut.GetUserInfoByIdsAsync([], ct: CancellationToken.None);
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetUserInfoByIdsAsync_NullInput_ReturnsEmptyDictionary()
    {
        var result = await _sut.GetUserInfoByIdsAsync(null!, ct: CancellationToken.None);
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetUserInfoByIdsAsync_ExceedsMaxBatchSize_ThrowsArgumentException()
    {
        // [RT-09] Hard cap 5000 IDs
        var tooManyIds = Enumerable.Range(0, 5001).Select(_ => Guid.NewGuid()).ToList();

        var act = () => _sut.GetUserInfoByIdsAsync(tooManyIds, ct: CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentException>()
            .WithParameterName("userIds");
    }

    [Fact]
    public async Task FindByEmailAsync_NonExistentUser_ReturnsNull()
    {
        // IReadDbConnection.QueryFirstOrDefaultAsync returns default(T) = null for class
        _db.QueryFirstOrDefaultAsync<UserInfoDto>(
            Arg.Any<string>(), Arg.Any<object?>(), Arg.Any<CancellationToken>(), Arg.Any<int?>())
            .Returns((UserInfoDto?)null);

        var result = await _sut.FindByEmailAsync("nobody@gov.vn");
        result.Should().BeNull();
    }

    [Fact]
    public async Task FindByEmailAsync_WithTenantId_IncludesTenantFilter()
    {
        // [RT-05] Verify tenant-scoped lookup passes tenantId in query
        var tenantId = Guid.NewGuid();
        var expected = new UserInfoDto
        {
            UserId = Guid.NewGuid(),
            Email = "test@gov.vn",
            TenantId = tenantId
        };

        _db.QueryFirstOrDefaultAsync<UserInfoDto>(
            Arg.Is<string>(sql => sql.Contains("TenantId")),
            Arg.Any<object?>(), Arg.Any<CancellationToken>(), Arg.Any<int?>())
            .Returns(expected);

        var result = await _sut.FindByEmailAsync("test@gov.vn", tenantId);
        result.Should().NotBeNull();
        result!.TenantId.Should().Be(tenantId);
    }
}
