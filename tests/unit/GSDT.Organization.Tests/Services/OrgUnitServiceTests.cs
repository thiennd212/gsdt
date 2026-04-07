using GSDT.Organization.DTOs;
using GSDT.Organization.Entities;
using GSDT.Organization.Persistence;
using GSDT.SharedKernel.Application.Caching;
using GSDT.SharedKernel.Application.Data;
using GSDT.SharedKernel.Domain;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NSubstitute;

namespace GSDT.Organization.Tests.Services;

/// <summary>
/// Unit tests for OrgUnitService.
/// TC-ORG-014: GetTreeAsync returns cached result on second call (cache-aside).
/// TC-ORG-015: GetAncestorsAsync walks ancestor chain to root.
/// TC-ORG-016: GetAncestorsAsync stops at max depth 20.
/// </summary>
public sealed class OrgUnitServiceTests : IDisposable
{
    private static readonly Guid TenantId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    private readonly OrgDbContext _db;
    private readonly ICacheService _cache;
    private readonly IReadDbConnection _readDb;
    private readonly OrgUnitService _sut;

    public OrgUnitServiceTests()
    {
        var tenantCtx = Substitute.For<ITenantContext>();
        tenantCtx.TenantId.Returns((Guid?)TenantId);
        tenantCtx.IsSystemAdmin.Returns(true);

        var options = new DbContextOptionsBuilder<OrgDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _db = new OrgDbContext(options, tenantCtx);
        _cache = Substitute.For<ICacheService>();
        _readDb = Substitute.For<IReadDbConnection>();
        _sut = new OrgUnitService(_db, _readDb, _cache);
    }

    public void Dispose() => _db.Dispose();

    // --- TC-ORG-014: GetTreeAsync cache-aside ---

    [Fact]
    public async Task GetTreeAsync_FirstCall_QueriesDbAndPopulatesCache()
    {
        _cache.GetAsync<List<OrgUnitDto>>(Arg.Any<string>(), Arg.Any<CancellationToken>())
              .Returns((List<OrgUnitDto>?)null);

        _db.OrgUnits.Add(OrgUnit.Create("Ministry", "Ministry EN", "MIN", TenantId));
        await _db.SaveChangesAsync();

        var result = await _sut.GetTreeAsync(TenantId);

        result.Should().HaveCount(1);
        await _cache.Received(1).SetAsync(
            Arg.Is<string>(k => k.Contains(TenantId.ToString())),
            Arg.Any<List<OrgUnitDto>>(),
            Arg.Any<TimeSpan?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetTreeAsync_SecondCall_ReturnsCachedResultWithoutDbQuery()
    {
        var cachedList = new List<OrgUnitDto>
        {
            new(Guid.NewGuid(), null, "Cached Ministry", "Cached EN", "MIN-C", 1, true, TenantId, null)
        };

        // First call: cache miss, second call: cache hit
        _cache.GetAsync<List<OrgUnitDto>>(Arg.Any<string>(), Arg.Any<CancellationToken>())
              .Returns(
                  _ => Task.FromResult<List<OrgUnitDto>?>(null),   // first call: miss
                  _ => Task.FromResult<List<OrgUnitDto>?>(cachedList) // second call: hit
              );

        await _sut.GetTreeAsync(TenantId);
        var secondResult = await _sut.GetTreeAsync(TenantId);

        secondResult.Should().HaveCount(1);
        secondResult[0].Name.Should().Be("Cached Ministry");
        // SetAsync called exactly once (only on cache miss)
        await _cache.Received(1).SetAsync(
            Arg.Any<string>(),
            Arg.Any<List<OrgUnitDto>>(),
            Arg.Any<TimeSpan?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetTreeAsync_CacheHit_ReturnsWithoutCallingSet()
    {
        var cached = new List<OrgUnitDto>
        {
            new(Guid.NewGuid(), null, "Cached", "Cached EN", "C1", 1, true, TenantId, null)
        };

        _cache.GetAsync<List<OrgUnitDto>>(Arg.Any<string>(), Arg.Any<CancellationToken>())
              .Returns(Task.FromResult<List<OrgUnitDto>?>(cached));

        var result = await _sut.GetTreeAsync(TenantId);

        result.Should().BeSameAs(cached);
        await _cache.DidNotReceive().SetAsync(
            Arg.Any<string>(),
            Arg.Any<List<OrgUnitDto>>(),
            Arg.Any<TimeSpan?>(),
            Arg.Any<CancellationToken>());
    }

    // --- TC-ORG-015: GetAncestorsAsync walks to root ---

    [Fact]
    public async Task GetAncestorsAsync_ThreeLevelChain_ReturnsChainFromLeafToRoot()
    {
        var root = OrgUnit.Create("Ministry", "Ministry EN", "MIN", TenantId, null, 1);
        var dept = OrgUnit.Create("Department", "Dept EN", "DEPT", TenantId, root.Id, 2);
        var div  = OrgUnit.Create("Division", "Div EN", "DIV", TenantId, dept.Id, 3);
        _db.OrgUnits.AddRange(root, dept, div);
        await _db.SaveChangesAsync();

        // GetAncestorsAsync uses GetTreeAsync internally — return null to force DB read
        _cache.GetAsync<List<OrgUnitDto>>(Arg.Any<string>(), Arg.Any<CancellationToken>())
              .Returns((List<OrgUnitDto>?)null);

        var ancestors = await _sut.GetAncestorsAsync(div.Id, TenantId);

        ancestors.Should().HaveCount(3);
        ancestors[0].Should().Be(div.Id);
        ancestors[1].Should().Be(dept.Id);
        ancestors[2].Should().Be(root.Id);
    }

    [Fact]
    public async Task GetAncestorsAsync_RootUnit_ReturnsSingleElementList()
    {
        var root = OrgUnit.Create("Root", "Root EN", "ROOT", TenantId, null, 1);
        _db.OrgUnits.Add(root);
        await _db.SaveChangesAsync();

        _cache.GetAsync<List<OrgUnitDto>>(Arg.Any<string>(), Arg.Any<CancellationToken>())
              .Returns((List<OrgUnitDto>?)null);

        var ancestors = await _sut.GetAncestorsAsync(root.Id, TenantId);

        ancestors.Should().ContainSingle().Which.Should().Be(root.Id);
    }

    [Fact]
    public async Task GetAncestorsAsync_UnknownId_ReturnsEmptyList()
    {
        _cache.GetAsync<List<OrgUnitDto>>(Arg.Any<string>(), Arg.Any<CancellationToken>())
              .Returns((List<OrgUnitDto>?)null);

        var ancestors = await _sut.GetAncestorsAsync(Guid.NewGuid(), TenantId);

        ancestors.Should().BeEmpty();
    }

    // --- TC-ORG-016: GetAncestorsAsync stops at max depth 20 ---

    [Fact]
    public async Task GetAncestorsAsync_ChainOf25_StopsAtDepth20()
    {
        // Build a flat list of 25 nodes where each points to the previous via ParentId
        // GetAncestorsAsync uses a dictionary walk — we inject cached data directly
        var nodes = new List<OrgUnitDto>();
        Guid? currentParent = null;

        for (int i = 1; i <= 25; i++)
        {
            var id = Guid.NewGuid();
            nodes.Add(new OrgUnitDto(id, currentParent, $"Level {i}", $"Level {i} EN", $"L{i:D2}", i, true, TenantId, null));
            currentParent = id;
        }

        // Return the cached list — deepest node is nodes[24]
        _cache.GetAsync<List<OrgUnitDto>>(Arg.Any<string>(), Arg.Any<CancellationToken>())
              .Returns(Task.FromResult<List<OrgUnitDto>?>(nodes));

        var deepestId = nodes[^1].Id; // node at depth 25
        var ancestors = await _sut.GetAncestorsAsync(deepestId, TenantId);

        // Guard in source: for (var depth = 0; depth < 20 ...) — max 20 iterations
        ancestors.Should().HaveCount(20);
    }
}
