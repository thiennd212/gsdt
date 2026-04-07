# Debugger Report — Fix Flaky Reporting Tests

**Date:** 2026-03-24
**Scope:** `GSDT.Reporting.Infrastructure.Tests` · `GSDT.Reporting.Application.Tests`

---

## Root Cause

**NSubstitute `Arg` matcher thread-local queue leak under parallel xUnit execution.**

xUnit v2 runs test *collections* in parallel by default. Both assemblies had no `xunit.runner.json`, so xUnit ran their test classes concurrently on the .NET thread-pool. NSubstitute stores pending `Arg.Any<>` / `Arg.Do<>` matchers in a **thread-local queue**. When the thread-pool re-uses a thread across test class instances (which happens constantly at scale), a matcher queued by one test's assertion is silently consumed by a *different* test's mock setup.

### Per-test failure chain

| Test | Trigger | Symptom |
|------|---------|---------|
| `ExecuteAsync_NonSelectSql_MarksExecutionFailed` | `ExecuteAsync_DangerousSql_MarksExecutionFailed` ran on same thread first; its 4× `Arg.Any<>` inside `DidNotReceive(...)` left matchers in the queue | Next test's `_executionRepo.GetByIdAsync(Arg.Any<Guid>(), ...)` setup consumed the leaked matchers → mock returned `null` → job returned early → `execution.Status` stayed `Queued` → assertion `Should().Be(Failed)` failed |
| `Handle_ValidCommand_CreatesDefinition` | `Arg.Do<ReportDefinition>` in the Arrange block enqueued a pending action-matcher **before** the SUT was called | Under concurrency, the matcher could be consumed by a sibling test's setup, causing the capture callback to never fire; subsequent `captured.Should().NotBeNull()` failed |

Both tests passed in isolation because single-test runs never experience cross-test thread reuse.

---

## Fixes Applied

### 1. `xunit.runner.json` — serialise test collections within each assembly

Created in both test project roots:

```json
{
  "$schema": "https://xunit.net/schema/current/xunit.runner.schema.json",
  "parallelizeTestCollections": false
}
```

`parallelizeTestCollections: false` forces xUnit to run each `[Collection]` sequentially within the assembly. Because all tests in these assemblies are in the *default* (implicit) collection, this serialises all test-class execution, eliminating thread reuse between tests.

Both `.csproj` files updated to include the JSON as `<Content CopyToOutputDirectory="PreserveNewest" />` so xUnit's runner picks it up at test host startup.

### 2. Replace `Arg.Do` capture with `ReceivedCalls()` inspection

`CreateReportDefinitionCommandHandlerTests.Handle_ValidCommand_CreatesDefinition`:

**Before** — `Arg.Do` enqueues a side-effect matcher during Arrange, before Act, leaving a pending matcher in the queue:
```csharp
await _repository.AddAsync(
    Arg.Do<ReportDefinition>(d => captured = d),
    Arg.Any<CancellationToken>());
```

**After** — post-hoc inspection via `ReceivedCalls()`, zero pending matchers during Arrange:
```csharp
var captured = (ReportDefinition)_repository.ReceivedCalls()
    .Single(c => c.GetMethodInfo().Name == nameof(_repository.AddAsync))
    .GetArguments()[0]!;
```

This is the correct NSubstitute pattern for capturing call arguments without side-effect matcher pollution.

---

## Files Changed

| File | Change |
|------|--------|
| `tests/unit/GSDT.Reporting.Infrastructure.Tests/xunit.runner.json` | Created — `parallelizeTestCollections: false` |
| `tests/unit/GSDT.Reporting.Application.Tests/xunit.runner.json` | Created — `parallelizeTestCollections: false` |
| `tests/unit/GSDT.Reporting.Infrastructure.Tests/GSDT.Reporting.Infrastructure.Tests.csproj` | Added `<Content>` entry for xunit.runner.json |
| `tests/unit/GSDT.Reporting.Application.Tests/GSDT.Reporting.Application.Tests.csproj` | Added `<Content>` entry for xunit.runner.json |
| `tests/unit/GSDT.Reporting.Application.Tests/Commands/CreateReportDefinitionCommandHandlerTests.cs` | Replaced `Arg.Do` with `ReceivedCalls()` capture |

---

## Verification

```
Passed!  - Failed: 0, Passed: 26, Skipped: 0 — GSDT.Reporting.Infrastructure.Tests
Passed!  - Failed: 0, Passed: 29, Skipped: 0 — GSDT.Reporting.Application.Tests
```

Both previously-failing tests confirmed passing individually and in full assembly run.

---

## Unresolved Questions

- Other test assemblies using `Arg.Do` in Arrange blocks (not in Reporting) may have the same latent issue. A codebase-wide grep for `Arg.Do` in test projects would identify candidates. Not addressed here — YAGNI until a flake is observed.
