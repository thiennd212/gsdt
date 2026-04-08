# Phase 08 — Admin Modules CRUD E2E (Expanded Scope)

**Status:** Pending | **Effort:** 3d | **Tests:** 30 | **Depends on:** P00, P01
**Validation:** User requested expanded coverage for all admin modules

## Overview
CRUD E2E tests for admin identity, admin content, admin system, and admin integration modules. Each module gets: navigate → list renders → CRUD flow → auth check.

## Identity Admin (12 tests)

| # | Module | Test | Expected |
|---|--------|------|----------|
| 1 | Users | `should list users with search` | Table renders, search filters |
| 2 | Users | `should create user via modal` | Modal → fill → save → in list |
| 3 | Users | `should assign role to user` | Role dropdown → save |
| 4 | Groups | `should CRUD user group` | Create + edit + delete group |
| 5 | ABAC Rules | `should list and create ABAC rule` | Table + create form |
| 6 | SoD Rules | `should list SoD rules` | Table renders |
| 7 | Policy Rules | `should list access policies` | Table renders |
| 8 | Credential Policies | `should edit credential policy` | Form → save |
| 9 | Access Reviews | `should list access reviews` | Table renders |
| 10 | Delegations | `should create delegation` | Modal → save → in list |
| 11 | Sessions | `should list active sessions` | Table with timestamps |
| 12 | External Identities | `should list external identities` | Table renders |

## Content Admin (6 tests)

| # | Module | Test | Expected |
|---|--------|------|----------|
| 13 | Templates | `should list document templates` | Table renders |
| 14 | Templates | `should create template` | Form → save |
| 15 | Notification Templates | `should list notification templates` | Table renders |
| 16 | Notification Templates | `should edit template` | Form → save |
| 17 | Menus | `should list menu items` | Table/tree renders |
| 18 | Menus | `should create menu item` | Form → save |

## System Admin (6 tests)

| # | Module | Test | Expected |
|---|--------|------|----------|
| 19 | Jobs | `should list background jobs` | Job table renders |
| 20 | Health | `should show system health status` | Health indicators green/red |
| 21 | Backup | `should list backup history` | Table renders |
| 22 | RTBF | `should list data erasure requests` | Table renders |
| 23 | Admin Dashboard | `should show admin overview stats` | KPI cards render |
| 24 | JIT SSO Config | `should list JIT provider configs` | Table renders |

## Integration Admin (6 tests)

| # | Module | Test | Expected |
|---|--------|------|----------|
| 25 | API Keys | `should list and create API key` | Create → key displayed |
| 26 | API Keys | `should revoke API key` | Click revoke → status changed |
| 27 | Webhooks | `should list webhook deliveries` | Table with status badges |
| 28 | AI Admin | `should show AI admin config` | Page renders |
| 29 | Data Scopes | `should list data scopes` | Table renders |
| 30 | Data Scopes | `should create data scope` | Form → save |

## Files to Create
- `e2e/admin/identity-admin.spec.ts` (tests 1-12)
- `e2e/admin/content-admin.spec.ts` (tests 13-18)
- `e2e/admin/system-admin.spec.ts` (tests 19-24)
- `e2e/admin/integration-admin.spec.ts` (tests 25-30)

## Auth Requirement
All tests login as **Admin/SystemAdmin** role. Verify non-admin gets 403 (1 test per spec file = 4 auth tests included in counts above).
