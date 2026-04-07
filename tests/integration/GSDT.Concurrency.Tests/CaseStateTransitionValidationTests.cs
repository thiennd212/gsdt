using GSDT.Cases.Domain.Entities;
using GSDT.Cases.Domain.Exceptions;
using GSDT.Cases.Infrastructure.Persistence;
using GSDT.Concurrency.Tests.Fixtures;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace GSDT.Concurrency.Tests;

/// <summary>
/// Verifies that CaseWorkflow domain rules reject illegal state transitions.
/// These tests exercise the guard clauses in Case domain methods without
/// needing two concurrent contexts — the domain throws before any DB write.
/// </summary>
[Collection(ConcurrencyCollection.CollectionName)]
public sealed class CaseStateTransitionValidationTests
{
    private readonly ConcurrencyFixture _fixture;

    private static int _seq;
    private static int NextSeq() => Interlocked.Increment(ref _seq);

    private static readonly Guid Actor = Guid.NewGuid();

    public CaseStateTransitionValidationTests(ConcurrencyFixture fixture)
        => _fixture = fixture;

    // -------------------------------------------------------------------------
    // Scenario 3 — Cannot double-submit (Draft → Submitted → Submitted again)
    // -------------------------------------------------------------------------

    [Fact]
    public async Task Submit_WhenAlreadySubmitted_ThrowsInvalidCaseTransitionException()
    {
        var tenantId = Guid.NewGuid();
        var caseId = await SeedCaseWithState(tenantId, CaseStatus.Submitted);

        await using var ctx = _fixture.CreateContext(new TestTenantContext(tenantId));
        var @case = await ctx.Cases.SingleAsync(c => c.Id == caseId);

        var act = () => @case.Submit(Actor);

        act.Should().Throw<InvalidCaseTransitionException>(
            "Submitted → Submit is not a valid transition in CaseWorkflow");
    }

    [Fact]
    public async Task Submit_WhenInUnderReview_ThrowsInvalidCaseTransitionException()
    {
        var tenantId = Guid.NewGuid();
        var caseId = await SeedCaseWithState(tenantId, CaseStatus.UnderReview);

        await using var ctx = _fixture.CreateContext(new TestTenantContext(tenantId));
        var @case = await ctx.Cases.SingleAsync(c => c.Id == caseId);

        var act = () => @case.Submit(Actor);

        act.Should().Throw<InvalidCaseTransitionException>(
            "UnderReview → Submit is not a valid transition in CaseWorkflow");
    }

    // -------------------------------------------------------------------------
    // Scenario 4 — Cannot approve a Draft (must reach UnderReview first)
    // -------------------------------------------------------------------------

    [Fact]
    public async Task Approve_WhenInDraft_ThrowsInvalidCaseTransitionException()
    {
        var tenantId = Guid.NewGuid();
        var caseId = await SeedCaseWithState(tenantId, CaseStatus.Draft);

        await using var ctx = _fixture.CreateContext(new TestTenantContext(tenantId));
        var @case = await ctx.Cases.SingleAsync(c => c.Id == caseId);

        var act = () => @case.Approve("Premature approval", Actor);

        act.Should().Throw<InvalidCaseTransitionException>(
            "Draft → Approve is not a valid transition; must be UnderReview first");
    }

    [Fact]
    public async Task Approve_WhenInSubmitted_ThrowsInvalidCaseTransitionException()
    {
        var tenantId = Guid.NewGuid();
        var caseId = await SeedCaseWithState(tenantId, CaseStatus.Submitted);

        await using var ctx = _fixture.CreateContext(new TestTenantContext(tenantId));
        var @case = await ctx.Cases.SingleAsync(c => c.Id == caseId);

        var act = () => @case.Approve("Premature approval", Actor);

        act.Should().Throw<InvalidCaseTransitionException>(
            "Submitted → Approve skips the Assign step; workflow must prevent it");
    }

    // -------------------------------------------------------------------------
    // Scenario 4b — Cannot reject a Draft
    // -------------------------------------------------------------------------

    [Fact]
    public async Task Reject_WhenInDraft_ThrowsInvalidCaseTransitionException()
    {
        var tenantId = Guid.NewGuid();
        var caseId = await SeedCaseWithState(tenantId, CaseStatus.Draft);

        await using var ctx = _fixture.CreateContext(new TestTenantContext(tenantId));
        var @case = await ctx.Cases.SingleAsync(c => c.Id == caseId);

        var act = () => @case.Reject("Invalid from draft", Actor);

        act.Should().Throw<InvalidCaseTransitionException>(
            "Draft → Reject is not a valid workflow transition");
    }

    // -------------------------------------------------------------------------
    // Scenario 4c — Cannot close a Draft or Submitted case
    // -------------------------------------------------------------------------

    [Fact]
    public async Task Close_WhenInDraft_ThrowsInvalidCaseTransitionException()
    {
        var tenantId = Guid.NewGuid();
        var caseId = await SeedCaseWithState(tenantId, CaseStatus.Draft);

        await using var ctx = _fixture.CreateContext(new TestTenantContext(tenantId));
        var @case = await ctx.Cases.SingleAsync(c => c.Id == caseId);

        var act = () => @case.Close(Actor);

        act.Should().Throw<InvalidCaseTransitionException>(
            "Draft → Close is not a valid workflow transition");
    }

    // -------------------------------------------------------------------------
    // Happy path — full lifecycle succeeds without concurrency conflict
    // -------------------------------------------------------------------------

    [Fact]
    public async Task FullLifecycle_DraftToApprovedToClosed_Succeeds()
    {
        var tenantId = Guid.NewGuid();

        await using var ctx = _fixture.CreateContext(new TestTenantContext(tenantId));

        var @case = MakeCase(tenantId, "FullLifecycle-Happy");
        ctx.Cases.Add(@case);
        await ctx.SaveChangesAsync();

        @case.Submit(Actor);
        await ctx.SaveChangesAsync();
        @case.Status.Should().Be(CaseStatus.Submitted);

        @case.Assign(Guid.NewGuid(), "LegalDept", Actor);
        await ctx.SaveChangesAsync();
        @case.Status.Should().Be(CaseStatus.UnderReview);

        @case.Approve("All criteria met", Actor);
        await ctx.SaveChangesAsync();
        @case.Status.Should().Be(CaseStatus.Approved);

        @case.Close(Actor);
        await ctx.SaveChangesAsync();
        @case.Status.Should().Be(CaseStatus.Closed);
    }

    // -------------------------------------------------------------------------
    // Private helpers
    // -------------------------------------------------------------------------

    /// <summary>
    /// Seeds a Case advanced to <paramref name="targetStatus"/> using an admin context.
    /// Returns the Case Id.
    /// </summary>
    private async Task<Guid> SeedCaseWithState(Guid tenantId, CaseStatus targetStatus)
    {
        await using var ctx = _fixture.CreateContext(new TestTenantContext(null, isSystemAdmin: true));

        var @case = MakeCase(tenantId, $"Seed-{targetStatus}-{Guid.NewGuid():N}");
        ctx.Cases.Add(@case);
        await ctx.SaveChangesAsync();

        if (targetStatus == CaseStatus.Draft)
            return @case.Id;

        @case.Submit(Actor);
        await ctx.SaveChangesAsync();

        if (targetStatus == CaseStatus.Submitted)
            return @case.Id;

        @case.Assign(Guid.NewGuid(), "Dept", Actor);
        await ctx.SaveChangesAsync();

        return @case.Id;   // UnderReview
    }

    private static Case MakeCase(Guid tenantId, string title)
        => Case.Create(
            tenantId,
            title,
            description: "State transition validation test case",
            type: CaseType.Application,
            priority: CasePriority.Medium,
            createdBy: Actor,
            yearSequence: NextSeq());
}
