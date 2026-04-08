# Red-Team Review -- GSDT Phase 2 Plan

**Date:** 2026-04-08
**Reviewer:** Red-team adversarial agent
**Plan:** plans/260408-1555-gsdt-phase2-ppp-dnnn-ndt-fdi/
**Files reviewed:** plan.md + phase-01 through phase-08 (9 files) + SRS analysis

## Overall Score: 7/10

Solid plan with clear entity models, good dependency graph, and realistic architecture. However, several critical technical inaccuracies (SQL Server vs jsonb), structural oversights (dual MasterData modules), and incomplete SRS function mapping could derail implementation.

## Category Scores

| Category | Score | Key Issue |
|----------|-------|-----------|
| Completeness | 6/10 | No explicit R47-R127 function traceability; DNNN THTH tab unspecified |
| Technical Accuracy | 6/10 | **jsonb on SQL Server is wrong**; Province in wrong module path; dual MasterData confusion |
| Reuse Claims | 8/10 | ~60% reuse is realistic; tab5-operation refactor correctly identified |
| Effort Estimates | 7/10 | PPP FE (6-8d) may be optimistic; Phase 6 copy-paste approach will create tech debt |
| Risk Gaps | 6/10 | Missing: migration ordering across 2 DbContexts, GovernmentAgency → existing catalog migration, IDOR on shared entities |
| Implementation Order | 8/10 | Dependency graph is correct; parallel P2/P4 opportunity valid |
| Security & Compliance | 7/10 | Auth pattern consistent but no granular permission model for PPP-specific roles |

---

## Critical Findings (MUST fix before implementation)

### F1: InvestorIds as Guid[] with jsonb -- SQL Server does not support jsonb
**Severity:** Critical
**Location:** phase-02-ppp-be-domain.md, InvestorSelection entity (line 132), Risk Assessment (line 499)
**Issue:** Plan specifies `Guid[] InvestorIds` stored as `jsonb` column. The codebase uses **Microsoft.EntityFrameworkCore.SqlServer** exclusively (confirmed in 12 `.csproj` files). SQL Server has no `jsonb` type. Migration snapshot confirms `uniqueidentifier`, `nvarchar`, `datetimeoffset` column types throughout. The plan's risk assessment even says "Use `HasColumnType("jsonb")` + value converter if needed" -- this will fail at migration generation time.
**Recommendation:** Use `nvarchar(max)` with a JSON value converter (`HasConversion<string>()`) or, better, create a proper junction table `InvestorSelectionInvestor(InvestorSelectionId, InvestorId)` for referential integrity and queryability. Junction table is preferred for a GOV system where data integrity matters and you need to query "which projects reference this investor."

### F2: Province/Ward entity lives in GSDT.MasterData combined project, not where plan references
**Severity:** Critical
**Location:** phase-01-catalogs-migration.md, Related Code Files section (lines 120-121)
**Issue:** Plan says to modify `src/modules/masterdata/GSDT.MasterData/Entities/Province.cs` and `Ward.cs`. This is correct for the **combined project** (`GSDT.MasterData`). However, there are **two** Province classes in the codebase:
- `src/modules/masterdata/GSDT.MasterData/Entities/Province.cs` (combined project, used by `MasterDataDbContext` in `GSDT.MasterData/Persistence/`)
- `src/modules/masterdata/GSDT.MasterData.Domain/Entities/Province.cs` (4-layer split project)

Both exist with identical content but in different namespaces (`GSDT.MasterData.Entities` vs `GSDT.MasterData.Domain.Entities`). The plan does not acknowledge this duplication. Modifying only one will create a divergence. The active `MasterDataDbContext` (the one with migrations at `20260407143900_AddGsdtCatalogs`) is in `GSDT.MasterData/Persistence/` -- meaning the combined project is the live one.
**Recommendation:** Clarify which MasterData project is authoritative. If `GSDT.MasterData` (combined) is the active module, all Phase 1 catalog additions (GovernmentAgency, Investor) must go there, not in the 4-layer split. Either consolidate or document which is active and deprecate the other.

### F3: InvestorSelection shared PK pattern -- FK target ambiguity
**Severity:** Critical
**Location:** phase-02-ppp-be-domain.md (line 129), phase-04-dnnn-be-domain.md (lines 136-139)
**Issue:** Phase 2 defines `InvestorSelection` with `Guid ProjectId` as shared PK + FK. The entity spec (line 129) says "PK + FK (shared PK pattern)" and navigation is `PppProject Project`. But Phase 4 says "InvestorSelection was created in Phase 2 with FK to InvestmentProject base... verify during implementation." There is a direct contradiction:
- Phase 2 entity definition shows nav as `PppProject Project` -- this creates an FK to PppProjects table
- Phase 4 assumes FK to base `InvestmentProject` -- this would be a different FK target

If Phase 2 implements FK to PppProject, DNNN cannot reuse it without schema refactoring (migration to change FK target). This is acknowledged as a risk in Phase 4 line 274 but not resolved in the plan.
**Recommendation:** Explicitly resolve NOW: change Phase 2 `InvestorSelection.Project` navigation to `InvestmentProject Project` (base entity). Document this decision in Phase 2. This is not a "verify during implementation" item -- it is a design decision that must be made before coding starts.

---

## High Priority Findings

### F4: DesignEstimate TotalEstimate auto-sum -- server-side vs client-side mismatch
**Severity:** High
**Location:** phase-02-ppp-be-domain.md (line 259), phase-03-ppp-fe.md (line 109)
**Issue:** BE entity has `decimal TotalEstimate` as a stored field with comment "Auto-sum (enforced in handler)". FE has `design-estimate-popup.tsx` doing auto-sum of 7 cost items client-side. If both compute independently, there is a race: client sends pre-computed total, server recomputes and potentially disagrees due to rounding or stale data. Neither phase specifies who is authoritative. The handler should be authoritative (server-side), but then the FE auto-sum is purely cosmetic -- yet the plan does not distinguish display-only from submitted values.
**Recommendation:** Make server handler authoritative: ignore client-sent TotalEstimate, always recompute. FE auto-sum is for UX preview only. Document this in both phase files.

### F5: 6 TPT children -- EF Core query degradation is real but unquantified
**Severity:** High
**Location:** plan.md line 97 (Entity Map), phase-08-buffer-polish.md (Performance Checklist)
**Issue:** EF Core TPT with 6 children means every `DbSet<InvestmentProject>.Where(...)` query generates a LEFT JOIN across 6 child tables. This is a known EF Core TPT penalty. The plan mentions "benchmark after adding PPP+DNNN" (phase-02 risk) and Phase 8 has performance checklist. But there is no contingency plan if performance is unacceptable. With 6 children, a base-table query to list "all projects" joins 7 tables. Indexes alone won't fix this.
**Recommendation:** Add explicit fallback strategy: if TPT queries exceed 1s on base table, switch to TPC (Table-per-Concrete-Type) or add a denormalized view/materialized query for cross-type listing. EF Core 10 supports TPC -- evaluate as contingency. Also: ensure list endpoints ALWAYS filter by ProjectType to avoid base-table scans.

### F6: No explicit SRS function traceability (R47-R127)
**Severity:** High
**Location:** All phase files
**Issue:** SRS analysis identifies 81 new functions (R47-R127). The plan does NOT provide a mapping of which function is covered by which phase/step. For example: R78 (HĐ lựa chọn NĐT for DNNN) is mentioned only in the SRS analysis as "no dedicated field table" but has no explicit implementation step in any phase file. R80 (Nghĩa vụ TC) is deferred but only mentioned in plan.md Decision 8 -- not in any phase checklist. If a developer follows the phase files strictly, they will miss these functions.
**Recommendation:** Add a traceability matrix appendix to plan.md: `| SRS Function | Phase | Step | Status |` for all 81 functions. At minimum, tag deferred items explicitly in the relevant phase checklists.

### F7: DNNN THTH tab (Tab3) has no field specification anywhere
**Severity:** High
**Location:** phase-04-dnnn-be-domain.md (line 24), phase-05-dnnn-fe.md (line 21)
**Issue:** DNNN Tab3 (THTH) is described as "assumed similar to PPP Tab3 with DNNN capital structure" in Phase 4 and "bid packages, execution records -- no capital plan section" in Phase 5. But there is no field spec table for this tab in the SRS (the analysis itself says "THTH tab references exist in function list R79-R80 but no dedicated field table"). The plan proceeds on assumption without flagging this as a blocker. If BA provides different fields, the entire Tab3 implementation is thrown away.
**Recommendation:** Flag DNNN THTH tab as a **BA confirmation required** item in Phase 4 prerequisites. Add a TODO item: "Confirm DNNN THTH tab field spec with BA before implementing Tab3." Implement PPP Tab3 first (which has a spec) then adapt.

---

## Medium Priority Findings

### F8: tab5-operation.tsx refactor -- breaking change risk to ODA underestimated
**Severity:** Medium
**Location:** phase-03-ppp-fe.md (line 179, step 5)
**Issue:** Phase 3 step 5 says "Refactor tab5-operation.tsx -- accept `projectQueryHook` prop instead of hardcoded `useDomesticProject`". Current code (confirmed): `const { data: project } = useDomesticProject(projectId);`. This is a shared tab used by both TN and ODA tabs containers. Changing the API from zero-prop data fetching to prop-injected hook means:
1. Every existing consumer (domestic-project-tabs-container.tsx, oda-project-tabs-container.tsx) must pass the new prop
2. The ODA tabs container currently imports this shared tab and it "just works" because ODA uses same detail shape -- but the hook name `useDomesticProject` suggests it may not actually work for ODA currently (it fetches domestic-specific data)

The risk assessment says "Add backward-compatible prop default" but doesn't specify how. A default to `useDomesticProject` maintains backward compatibility for TN but breaks ODA (if ODA was relying on this hook working for ODA projects -- which is suspicious).
**Recommendation:** Before refactoring, verify: does `useDomesticProject(odaProjectId)` return ODA data? If not, ODA tab5 is already broken. If yes, the hook name is misleading and should be `useProjectDetail(id)` at the base level. This refactor is larger than 0.5d.

### F9: GovernmentAgency migration for existing TN/ODA CQQL is deferred but has data integrity implications
**Severity:** Medium
**Location:** phase-08-buffer-polish.md (lines 46, 111-114), plan.md Decision Q10
**Issue:** Decision Q10 says "CQCQ = GovernmentAgency? Yes... Migrate TN/ODA refs." Phase 8 defers this. But: existing TN/ODA projects reference `ManagingAuthorityId` (a Guid to `ManagingAuthority` catalog). PPP/DNNN will reference `CompetentAuthorityId` pointing to `GovernmentAgency` catalog. These are two different catalogs for conceptually the same data. This means:
1. Cross-type queries filtering by "managing authority" must join two different catalog tables
2. Reporting will show inconsistent authority names
3. If GovernmentAgency migration for TN/ODA is later cancelled, the system permanently has two authority sources

This is not a polish item -- it is an architectural decision with schema implications.
**Recommendation:** Either: (a) implement GovernmentAgency migration for TN/ODA in Phase 1 (catalogs), or (b) explicitly document that TN/ODA will NOT migrate and will permanently use ManagingAuthority while PPP/DNNN/NĐT/FDI use GovernmentAgency. Don't defer -- decide.

### F10: Phase 6 copy-paste creates massive code duplication (NĐT ≈ FDI ≈ DNNN)
**Severity:** Medium
**Location:** phase-06-ndt-fdi-be-fe.md (entire file)
**Issue:** Phase 6 creates ~32 new files (12 BE + 20 FE) that are nearly identical copies of DNNN. The plan acknowledges this ("DRY Consideration" section, line 140) but accepts it. The result: 4 project types (PPP, DNNN, NĐT, FDI) each have their own InvestmentDecision entity with identical fields except navigation type. That is 4 copies of the same capital validation logic, 4 copies of the same decision DTO, 4 controllers with identical endpoint patterns.

With 6 TPT children total, every future change to "how decisions work" requires updating 6 entity classes, 6 configurations, 6 command handlers. This is a maintenance time bomb.
**Recommendation:** Consider extracting a generic `ProjectInvestmentDecision<TProject>` base or using a shared command handler with discriminator. At minimum, extract the capital validation logic into a shared validator utility. The FE should use a shared `project-decision-zone` component parameterized by capital field configuration, not 4 copies.

### F11: Migration across two DbContexts (MasterData + InvestmentProjects) -- ordering risk
**Severity:** Medium
**Location:** phase-01-catalogs-migration.md (line 139), phase-02-ppp-be-domain.md (line 431)
**Issue:** Phase 1 generates a migration on `MasterDataDbContext`. Phase 2 generates a migration on `InvestmentProjectsDbContext`. Both must be applied in order. But: EF Core migrations are per-DbContext -- there is no cross-context ordering guarantee. If a deployment script runs InvestmentProjects migration before MasterData, the GovernmentAgency and Investor tables won't exist yet, and any FK references will fail. The plan's rollback strategy says "No cross-phase migration dependencies except Phase 1 catalogs" -- but this IS a cross-context dependency.
**Recommendation:** Document migration application order explicitly. Add a deployment script that runs MasterData migrations first, then InvestmentProjects. Add a startup check that verifies catalog tables exist before InvestmentProjects module initializes.

---

## Observations (nice-to-fix, not blocking)

### O1: PppContractType enum has 10 values but SRS cascading UI suggests 3 groups
The enum flattens the cascading structure (BOT / BT variants / Other variants) into a single enum. This means the FE must reverse-map enum values to radio+combo state. Would be cleaner as two fields: `ContractCategory` (BOT/BT/Other) + `ContractSubType` (nullable, for BT/Other subtypes). But current approach works -- just more FE logic.

### O2: Revenue cumulative calculation is vague
Phase 2 BE (RevenueReport entity, line 293) stores `RevenueCumulative` as a persisted field. Phase 3 FE (line 109) says "auto-sum of all RevenueReport.RevenuePeriod for selected year range." If cumulative is persisted, it must be recomputed on every insert/update/delete of any revenue record for that project. The handler needs to update all subsequent records' cumulative values -- this is not mentioned in any implementation step.

### O3: RegistrationCertificate EquityRatio auto-calc edge case
Phase 4 line 125: `decimal? EquityRatio // Tỷ lệ CSH (auto-calc)`. If `InvestmentCapital` is 0, division by zero. Phase 7 tests mention this for EquityRatio on decisions but not for certificates specifically.

### O4: Phase 7 estimates ~95-105 new tests but Phase 1 had 120 tests for 2 project types
Phase 2 adds 4 project types with more complexity (PPP Tab2 is entirely new, revenue reporting is new, GCNĐKĐT is new). Yet test count is lower. This may be intentional (thinner NĐT/FDI coverage) but PPP alone warrants ~50+ E2E tests given its 7 tabs and unique patterns.

### O5: No FE unit test plan
Phase 7 covers BE unit tests and E2E tests. There are no unit tests planned for FE business logic: TMĐT validation (Total == VonNN + CSH + Vay), contract type cascading logic, cumulative auto-calc. These are prime candidates for Jest/Vitest unit tests.

### O6: GovernmentAgency tree endpoint -- recursive CTE vs in-memory
Phase 1 step 10 (line 143) mentions "recursive CTE or in-memory build" for the tree endpoint. With SQL Server, a recursive CTE on a self-referencing table works but can be slow for deep hierarchies. For 4 levels and likely <500 nodes, in-memory build is fine. But the plan should pick one approach, not leave it as "or."

### O7: Missing PUT endpoints for sub-entities
Phase 2 API table has `PUT` for design-estimates and revenue-reports but not for decisions, capital plans, or disbursements. If a user adds a decision with wrong data, they must delete and re-add. This is a UX gap the plan should acknowledge.

---

## Verdict: REVISE

The plan is structurally sound with good entity modeling, clear dependency graph, and realistic reuse estimates. However, three critical findings (F1: jsonb on SQL Server, F2: dual MasterData modules, F3: InvestorSelection FK ambiguity) must be resolved before implementation starts. They represent real implementation blockers that will cause migration failures or runtime errors.

Fix the 3 critical issues, resolve F4 (auto-sum authority), add SRS traceability (F6), and clarify the GovernmentAgency migration strategy (F9). Then the plan is ready to execute.

---

## Unresolved Questions

1. Which MasterData project is authoritative -- `GSDT.MasterData` (combined) or `GSDT.MasterData.Domain/Infrastructure/etc` (4-layer)? Both have Province entities.
2. Does `useDomesticProject(odaProjectId)` actually return ODA data? If not, ODA tab5-operation is already broken in Phase 1.
3. Is there a deployment script that enforces cross-DbContext migration ordering?
4. Has BA been formally asked to confirm DNNN THTH tab field spec (R79-R80)?
5. What is the contingency if 6 TPT children cause unacceptable query performance on base-table operations?
