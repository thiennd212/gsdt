using GSDT.Cases.Domain.Entities;
using GSDT.Cases.Infrastructure.Persistence;
using GSDT.Concurrency.Tests.Fixtures;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace GSDT.Concurrency.Tests;

/// <summary>
/// Verifies that the RowVersion concurrency token on Case prevents silent
/// overwrites when two DbContext instances load and modify the same row.
///
/// Design:
///   - Context A and Context B each load the *same* Case independently
///     (AsNoTracking=false so EF attaches the RowVersion token).
///   - Context A saves first — succeeds and increments the SQL ROWVERSION.
///   - Context B saves second — its stale token no longer matches the DB value,
///     so EF throws DbUpdateConcurrencyException.
/// </summary>
[Collection(ConcurrencyCollection.CollectionName)]
public sealed class CaseOptimisticConcurrencyTests
{
    private readonly ConcurrencyFixture _fixture;

    // Thread-safe sequence counter — each test needs a unique CaseNumber.
    private static int _seq;
    private static int NextSeq() => Interlocked.Increment(ref _seq);

    private static readonly Guid Actor = Guid.NewGuid();

    public CaseOptimisticConcurrencyTests(ConcurrencyFixture fixture)
        => _fixture = fixture;

    // -------------------------------------------------------------------------
    // Scenario 1 — Two approvals race: second context must be rejected
    // -------------------------------------------------------------------------

    [Fact]
    public async Task Approve_SecondContextWithStaleRowVersion_ThrowsDbUpdateConcurrencyException()
    {
        var tenantId = Guid.NewGuid();
        var caseId = await CreateCaseInUnderReview(tenantId);

        // Load the same case into two independent contexts (each gets current RowVersion).
        await using var ctxA = _fixture.CreateContext(new TestTenantContext(tenantId));
        await using var ctxB = _fixture.CreateContext(new TestTenantContext(tenantId));

        var caseInA = await ctxA.Cases.SingleAsync(c => c.Id == caseId);
        var caseInB = await ctxB.Cases.SingleAsync(c => c.Id == caseId);

        // Context A approves first — succeeds, DB ROWVERSION increments.
        caseInA.Approve("Approved by reviewer A", Actor);
        await ctxA.SaveChangesAsync();

        // Context B holds a now-stale RowVersion token.
        caseInB.Approve("Approved by reviewer B", Actor);
        var act = () => ctxB.SaveChangesAsync();

        await act.Should()
            .ThrowAsync<DbUpdateConcurrencyException>(
                "Context B has a stale RowVersion — the row was already updated by Context A");
    }

    // -------------------------------------------------------------------------
    // Scenario 2 — Approve vs Reject race: reject with stale token must fail
    // -------------------------------------------------------------------------

    [Fact]
    public async Task ApproveVsReject_SecondContextWithStaleRowVersion_ThrowsDbUpdateConcurrencyException()
    {
        var tenantId = Guid.NewGuid();
        var caseId = await CreateCaseInUnderReview(tenantId);

        await using var ctxA = _fixture.CreateContext(new TestTenantContext(tenantId));
        await using var ctxB = _fixture.CreateContext(new TestTenantContext(tenantId));

        var caseInA = await ctxA.Cases.SingleAsync(c => c.Id == caseId);
        var caseInB = await ctxB.Cases.SingleAsync(c => c.Id == caseId);

        // Context A approves — changes RowVersion in DB.
        caseInA.Approve("Looks good", Actor);
        await ctxA.SaveChangesAsync();

        // Context B tries to reject with the old RowVersion — must be blocked.
        caseInB.Reject("Does not meet criteria", Actor);
        var act = () => ctxB.SaveChangesAsync();

        await act.Should()
            .ThrowAsync<DbUpdateConcurrencyException>(
                "Reject must fail: RowVersion changed after Context A approved the case");
    }

    // -------------------------------------------------------------------------
    // Scenario 3 — RowVersion is populated after insert (sanity check)
    // -------------------------------------------------------------------------

    [Fact]
    public async Task RowVersion_IsPopulatedByDatabaseAfterInsert()
    {
        var tenantId = Guid.NewGuid();

        await using var ctx = _fixture.CreateContext(new TestTenantContext(tenantId));
        var @case = MakeCase(tenantId, "RowVersion-Sanity");
        ctx.Cases.Add(@case);
        await ctx.SaveChangesAsync();

        @case.RowVersion.Should().NotBeNull("SQL Server ROWVERSION must be assigned by DB on insert");
        @case.RowVersion.Should().NotBeEmpty();
    }

    // -------------------------------------------------------------------------
    // Scenario 4 — RowVersion changes after each state transition
    // -------------------------------------------------------------------------

    [Fact]
    public async Task RowVersion_ChangesAfterEachStateTransition()
    {
        var tenantId = Guid.NewGuid();
        var caseId = await CreateCaseInUnderReview(tenantId);

        await using var ctx = _fixture.CreateContext(new TestTenantContext(tenantId));
        var @case = await ctx.Cases.SingleAsync(c => c.Id == caseId);
        var versionBeforeApprove = @case.RowVersion.ToArray();  // copy current bytes

        @case.Approve("Token rotation check", Actor);
        await ctx.SaveChangesAsync();

        @case.RowVersion.Should().NotEqual(versionBeforeApprove,
            "every SaveChanges on a ROWVERSION column increments the token");
    }

    // -------------------------------------------------------------------------
    // Private helpers
    // -------------------------------------------------------------------------

    /// <summary>
    /// Creates a Case and advances it to UnderReview using a system-admin context.
    /// Returns the new Case Id.
    /// </summary>
    private async Task<Guid> CreateCaseInUnderReview(Guid tenantId)
    {
        await using var ctx = _fixture.CreateContext(new TestTenantContext(null, isSystemAdmin: true));

        var @case = MakeCase(tenantId, $"Concurrency-Test-{Guid.NewGuid():N}");
        ctx.Cases.Add(@case);
        await ctx.SaveChangesAsync();

        @case.Submit(Actor);
        await ctx.SaveChangesAsync();

        @case.Assign(Guid.NewGuid(), "ReviewDept", Actor);
        await ctx.SaveChangesAsync();

        return @case.Id;
    }

    private static Case MakeCase(Guid tenantId, string title)
        => Case.Create(
            tenantId,
            title,
            description: "Concurrency integration test case",
            type: CaseType.Application,
            priority: CasePriority.Medium,
            createdBy: Actor,
            yearSequence: NextSeq());
}
