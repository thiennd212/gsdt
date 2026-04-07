using GSDT.Cases.Domain.Entities;
using GSDT.Cases.Infrastructure.Persistence;
using GSDT.TenantIsolation.Tests.Fixtures;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace GSDT.TenantIsolation.Tests;

/// <summary>
/// Verifies that the EF Core global TenantId query filter enforced by ModuleDbContext
/// prevents cross-tenant data access against a real SQL Server instance.
///
/// Design notes:
///   - Each test inserts its own data and operates on an independent DbContext scope.
///   - "Seed" contexts use IsSystemAdmin=true (NullTenantContext behaviour) to bypass the
///     filter when inserting data for multiple tenants in a single test.
///   - Raw SQL / Dapper bypasses EF filters — see Scenario 5 for documented risk.
/// </summary>
[Collection(TenantIsolationCollection.CollectionName)]
public sealed class TenantQueryFilterTests
{
    private readonly TenantIsolationFixture _fixture;

    // Thread-safe counter so every test gets unique CaseNumber sequences.
    private static int _seq;
    private static int NextSeq() => Interlocked.Increment(ref _seq);

    private static readonly Guid Actor = Guid.NewGuid();

    public TenantQueryFilterTests(TenantIsolationFixture fixture)
    {
        _fixture = fixture;
    }

    // -------------------------------------------------------------------------
    // Scenario 1: Tenant A cannot see Tenant B's data
    // -------------------------------------------------------------------------

    [Fact]
    public async Task Query_AsTenantA_DoesNotReturnCasesOwnedByTenantB()
    {
        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();

        // Seed one case per tenant using an admin (filter-bypassing) context.
        await using (var adminCtx = _fixture.CreateContext(new TestTenantContext(null, isSystemAdmin: true)))
        {
            adminCtx.Cases.Add(MakeCase(tenantA, "TenantA-Case-Sc1"));
            adminCtx.Cases.Add(MakeCase(tenantB, "TenantB-Case-Sc1"));
            await adminCtx.SaveChangesAsync();
        }

        // Query as TenantA — filter should hide TenantB's row.
        await using var ctxA = _fixture.CreateContext(new TestTenantContext(tenantA));
        var results = await ctxA.Cases.AsNoTracking().ToListAsync();

        results.Should().OnlyContain(c => c.TenantId == tenantA,
            "TenantA context must not surface rows belonging to TenantB");
        results.Should().NotContain(c => c.Title == "TenantB-Case-Sc1");
    }

    [Fact]
    public async Task Query_AsTenantB_DoesNotReturnCasesOwnedByTenantA()
    {
        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();

        await using (var adminCtx = _fixture.CreateContext(new TestTenantContext(null, isSystemAdmin: true)))
        {
            adminCtx.Cases.Add(MakeCase(tenantA, "TenantA-Case-Sc1b"));
            adminCtx.Cases.Add(MakeCase(tenantB, "TenantB-Case-Sc1b"));
            await adminCtx.SaveChangesAsync();
        }

        await using var ctxB = _fixture.CreateContext(new TestTenantContext(tenantB));
        var results = await ctxB.Cases.AsNoTracking().ToListAsync();

        results.Should().OnlyContain(c => c.TenantId == tenantB);
        results.Should().NotContain(c => c.Title == "TenantA-Case-Sc1b");
    }

    // -------------------------------------------------------------------------
    // Scenario 2: SystemAdmin sees all tenants
    // -------------------------------------------------------------------------

    [Fact]
    public async Task Query_AsSystemAdmin_ReturnsDataFromAllTenants()
    {
        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();

        await using (var adminCtx = _fixture.CreateContext(new TestTenantContext(null, isSystemAdmin: true)))
        {
            adminCtx.Cases.Add(MakeCase(tenantA, "Sc2-TenantA"));
            adminCtx.Cases.Add(MakeCase(tenantB, "Sc2-TenantB"));
            await adminCtx.SaveChangesAsync();
        }

        // Fresh admin context to query.
        await using var queryCtx = _fixture.CreateContext(new TestTenantContext(null, isSystemAdmin: true));
        var results = await queryCtx.Cases.AsNoTracking().ToListAsync();

        // Both tenants' cases must be visible to SystemAdmin.
        results.Should().Contain(c => c.TenantId == tenantA && c.Title == "Sc2-TenantA");
        results.Should().Contain(c => c.TenantId == tenantB && c.Title == "Sc2-TenantB");
    }

    // -------------------------------------------------------------------------
    // Scenario 3: Filter applies correctly when using Include (navigation)
    // -------------------------------------------------------------------------

    [Fact]
    public async Task Query_WithIncludeComments_AsTenantA_OnlyLoadsTenantACaseWithItsComments()
    {
        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();

        Guid caseAId, caseBId;

        await using (var adminCtx = _fixture.CreateContext(new TestTenantContext(null, isSystemAdmin: true)))
        {
            var caseA = MakeCase(tenantA, "Sc3-TenantA");
            caseA.AddComment("CommentA", Actor);

            var caseB = MakeCase(tenantB, "Sc3-TenantB");
            caseB.AddComment("CommentB", Actor);

            adminCtx.Cases.Add(caseA);
            adminCtx.Cases.Add(caseB);
            await adminCtx.SaveChangesAsync();

            caseAId = caseA.Id;
            caseBId = caseB.Id;
        }

        // Query as TenantA with eager-loaded comments.
        await using var ctxA = _fixture.CreateContext(new TestTenantContext(tenantA));
        var results = await ctxA.Cases
            .AsNoTracking()
            .Include(c => c.Comments)
            .ToListAsync();

        results.Should().ContainSingle(c => c.Id == caseAId,
            "TenantA query should return exactly TenantA's case");
        results.Should().NotContain(c => c.Id == caseBId,
            "TenantA query must not return TenantB's case via Include");

        var caseA2 = results.Single(c => c.Id == caseAId);
        caseA2.Comments.Should().ContainSingle(cm => cm.Content == "CommentA");
    }

    // -------------------------------------------------------------------------
    // Scenario 4: Soft-delete + tenant filter combined
    // -------------------------------------------------------------------------

    [Fact]
    public async Task Query_AsTenantA_DoesNotSeeDeletedCasesFromOwnTenant()
    {
        var tenantA = Guid.NewGuid();

        Guid deletedId;

        await using (var seedCtx = _fixture.CreateContext(new TestTenantContext(tenantA)))
        {
            var @case = MakeCase(tenantA, "Sc4-ToBeDeleted");
            seedCtx.Cases.Add(@case);
            await seedCtx.SaveChangesAsync();
            deletedId = @case.Id;
        }

        // Soft-delete via domain method. Use tracking context so EF picks up the change.
        await using (var deleteCtx = _fixture.CreateContext(new TestTenantContext(tenantA)))
        {
            var @case = await deleteCtx.Cases.SingleAsync(c => c.Id == deletedId);
            @case.MarkDeleted();
            await deleteCtx.SaveChangesAsync();
        }

        // Query as TenantA — soft-deleted case must be hidden by the combined filter.
        await using var queryCtx = _fixture.CreateContext(new TestTenantContext(tenantA));
        var found = await queryCtx.Cases.AsNoTracking().AnyAsync(c => c.Id == deletedId);

        found.Should().BeFalse("soft-deleted case must be excluded by the global IsDeleted filter");
    }

    [Fact]
    public async Task Query_AsTenantB_NeverSeesDeletedCaseFromTenantA()
    {
        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();

        await using (var adminCtx = _fixture.CreateContext(new TestTenantContext(null, isSystemAdmin: true)))
        {
            var @case = MakeCase(tenantA, "Sc4b-TenantA-Deleted");
            @case.MarkDeleted();
            adminCtx.Cases.Add(@case);
            await adminCtx.SaveChangesAsync();
        }

        // TenantB must see neither the deleted row (soft-delete) nor the TenantA row (tenant filter).
        await using var ctxB = _fixture.CreateContext(new TestTenantContext(tenantB));
        var found = await ctxB.Cases.AsNoTracking()
            .AnyAsync(c => c.Title == "Sc4b-TenantA-Deleted");

        found.Should().BeFalse("both tenant filter and soft-delete filter apply; TenantB sees nothing");
    }

    // -------------------------------------------------------------------------
    // Scenario 5: Raw SQL bypasses EF filter — documented risk
    // -------------------------------------------------------------------------

    [Fact]
    public async Task RawSql_BypassesEfTenantFilter_DocumentedRisk()
    {
        // RISK: Dapper / SqlCommand / FromSqlRaw without an explicit WHERE TenantId = @id
        // will return rows across all tenants.  This test documents the behaviour — it is NOT
        // a regression test to fix, but a contract test that reminds implementers to add
        // manual WHERE TenantId clauses to every raw SQL query.
        //
        // Mitigation checklist (ArchUnit Phase 10 will enforce these):
        //   1. Dapper queries MUST include WHERE TenantId = @tenantId.
        //   2. SqlCommand / ADO.NET queries are treated like Dapper — same rule.
        //   3. EF FromSqlRaw used as a sub-query inherits the filter only when wrapped in
        //      .Where(e => EF.Property<Guid>(e, "TenantId") == tenantId) afterward.
        //   4. Stored procedures must enforce tenant scoping internally.

        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();

        await using (var adminCtx = _fixture.CreateContext(new TestTenantContext(null, isSystemAdmin: true)))
        {
            adminCtx.Cases.Add(MakeCase(tenantA, "Sc5-RawSql-TenantA"));
            adminCtx.Cases.Add(MakeCase(tenantB, "Sc5-RawSql-TenantB"));
            await adminCtx.SaveChangesAsync();
        }

        // FromSqlRaw without a WHERE clause returns rows from all tenants — filter bypassed.
        await using var ctxA = _fixture.CreateContext(new TestTenantContext(tenantA));
        var rawResults = await ctxA.Cases
            .FromSqlRaw("SELECT * FROM cases.Cases WHERE IsDeleted = 0")
            .AsNoTracking()
            .IgnoreQueryFilters()   // explicitly bypass to demonstrate the raw SQL risk
            .ToListAsync();

        rawResults.Should().Contain(c => c.TenantId == tenantB,
            "raw SQL without WHERE TenantId returns cross-tenant rows — " +
            "always add an explicit tenant predicate to raw queries");
    }

    // -------------------------------------------------------------------------
    // Private helpers
    // -------------------------------------------------------------------------

    private static Case MakeCase(Guid tenantId, string title)
        => Case.Create(
            tenantId,
            title,
            description: $"Integration test case for {title}",
            type: CaseType.Application,
            priority: CasePriority.Medium,
            createdBy: Actor,
            yearSequence: NextSeq());
}
