---
title: "GSDT Phase 2 - PPP, DNNN, NDT, FDI Project Types"
description: "Add 4 new investment project types (PPP, DNNN, NDT trong nuoc, FDI) to GSDT"
status: in-progress (P01-P05 complete, P06 pending)
priority: P1
effort: 28-39d
branch: feature/gsdt-phase2
tags: [gsdt, phase2, ppp, dnnn, ndt, fdi, crud]
created: 2026-04-08
updated: 2026-04-08
red-team-score: 7/10
red-team-date: 2026-04-08
red-team-notes: "11 findings addressed (3 critical fixed, 4 high fixed, 4 medium noted)"
---

# GSDT Phase 2 -- PPP / DNNN / NDT / FDI CRUD

**SRS:** `03426.HT GSDT_SRS_1.2.docx`
**SRS Analysis:** `plans/reports/srs-analysis-260408-1527-gsdt-v12-delta.md`
**Phase 1 Reference:** `plans/260407-1007-gsdt-phase1-dtc-crud/plan.md`
**Effort:** ~28-39 days (1 fullstack dev)
**Branch:** `feature/gsdt-phase2`

---

## Phases

| # | Phase | Status | Effort | Commit | Date |
|---|-------|--------|--------|---------|------|
| 1 | [Catalogs & Migration](phase-01-catalogs-migration.md) | Complete | 2-3d | 0e6533c | 2026-04-08 |
| 2 | [PPP Backend Domain](phase-02-ppp-be-domain.md) | Complete | 5-6d | ca0383b | 2026-04-08 |
| 3 | [PPP Frontend](phase-03-ppp-fe.md) | Complete | 6-8d | 4fa1612 | 2026-04-08 |
| 4 | [DNNN Backend Domain](phase-04-dnnn-be-domain.md) | Complete | 3-4d | (merged) | 2026-04-08 |
| 5 | [DNNN Frontend](phase-05-dnnn-fe.md) | Complete | 4-5d | 2d4f6cb | 2026-04-08 |
| 6 | [NDT + FDI (BE+FE)](phase-06-ndt-fdi-be-fe.md) | Pending | 3-5d | -- | -- |
| 7 | [Testing](phase-07-testing.md) | Pending | 3-5d | -- | -- |
| 8 | [Buffer & Polish](phase-08-buffer-polish.md) | Pending | 2-3d | -- | -- |

## Dependency Graph

```
P1 (Catalogs) --> P2 (PPP BE) --> P3 (PPP FE)
P1 (Catalogs) --> P4 (DNNN BE) --> P5 (DNNN FE)
P4 + P5 --> P6 (NDT+FDI)
P3 + P5 + P6 --> P7 (Testing) --> P8 (Buffer)
```

**Critical path:** P1 -> P2 -> P3 -> P7 -> P8 (18-25d)
**Parallel opportunity:** P4 can start after P1 (parallel to P2/P3)

## Entity Map (Phase 2 additions)

```
InvestmentProject (existing TPT base)
+-- DomesticProject (P1 done)
+-- OdaProject (P1 done)
+-- PppProject ................... NEW TPT child (Phase 2)
|   +-- PppInvestmentDecision[]    Split: PPP capital (VonNN/CSH/Vay)
|   +-- PppCapitalPlan[]           "KH bo tri von NN"
|   +-- PppDisbursementRecord[]    3-source: VonNN/CSH/Vay
|   +-- PppExecutionRecord[]       THTH with sub-project fields
|   +-- InvestorSelection          1-to-1: NDT multiselect, QD
|   +-- PppContractInfo            1-to-1: TMDT, tiến độ, HĐ ký kết
|   +-- RevenueReport[]            PPP-only: periodic revenue
|   +-- DesignEstimate[]           Shared with DNNN (TKTT popup)
+-- DnnnProject .................. NEW TPT child (Phase 4)
|   +-- DnnnInvestmentDecision[]   Split: CSH/ODA/TCTD
|   +-- RegistrationCertificate[]  GCNĐKĐT
|   +-- DesignEstimate[]           Reuse from PPP
+-- NdtProject ................... NEW TPT child (Phase 6)
|   +-- RegistrationCertificate[]  Reuse from DNNN
+-- FdiProject ................... NEW TPT child (Phase 6)
|   +-- RegistrationCertificate[]  Reuse from NDT

# Extended existing entities
+-- ProjectLocation[]             ADD: IndustrialZoneName (DNNN/NDT/FDI)
+-- OperationInfo                 ADD: PPP settlement checkboxes

# New catalogs (MasterData module)
+-- GovernmentAgency              Hierarchical, 13 fields, T27
+-- Investor                      Flat, 5 fields, T28

# Extended catalogs
+-- Province                      ADD: EffectiveDate, Status
+-- Ward                          ADD: EffectiveDate, Status
```

**Total new entities:** ~12 | **Extended:** ~3 | **New catalogs:** 2

## Key Design Decisions (Approved 2026-04-08)

| # | Decision | Rationale |
|---|----------|-----------|
| 1 | NDT = DNNN clone minus 3 tabs | SRS: "tuong tu DNNN" |
| 2 | FDI = NDT clone | SRS: "tuong tu NDT trong nuoc" |
| 3 | PPP contract types: 9-value enum | BOT/BT variants/BTO/BOO/O&M/BTL/BLT/hon hop |
| 4 | Revenue: PPP-only | DNNN/NDT/FDI use TN operation pattern |
| 5 | Investor: system-wide catalog | Admin-managed in MasterData module |
| 6 | DesignEstimate: shared PPP+DNNN | Extract as base entity, FK to InvestmentProject |
| 7 | GovernmentAgency: hierarchical | Self-referencing ParentId, reuse OrgUnit tree FE pattern |
| 8 | DNNN "Nghia vu TC": DEFERRED | No field spec — placeholder only |
| 9 | ProjectType enum: extend to 6 | Ppp=3, Dnnn=4, Ndt=5, Fdi=6 |

## Backwards Compatibility

- Phase 1 TN/ODA entities: **ZERO changes** to existing fields or tables
- ProjectType enum: additive (new values only, no renumbering)
- Shared tabs (inspection, operation, documents): remain unchanged for TN/ODA
- Province/Ward: additive columns (nullable, no data loss)
- ProjectLocation: additive column (nullable IndustrialZoneName)
- API routes: new routes only, existing unchanged

## Risk: TPT Query Performance Fallback

If 6 TPT children cause list query degradation > 1s on the base `InvestmentProjects` table:
- **Contingency A:** Switch to TPC (Table-per-Concrete) via EF Core 10 support — each type gets its own full table, no base-table joins.
- **Contingency B:** Add a denormalized indexed view for cross-type listing queries.
- **Mandatory rule:** All list endpoints MUST filter by `ProjectType` to avoid base-table full joins across 6 child tables. Never query base table without a `ProjectType` predicate in list queries.

## Rollback Strategy

Each phase generates a separate EF migration. Rollback = `dotnet ef migrations remove` + revert git commits per phase. No cross-phase migration dependencies except Phase 1 catalogs.
