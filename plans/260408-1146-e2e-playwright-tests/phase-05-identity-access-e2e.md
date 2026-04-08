# Phase 05 — Identity & Access E2E

**Status:** Pending | **Effort:** 1d | **Tests:** 12 | **Depends on:** P01

## Test Cases

### Login (3 tests)
| # | Test | Expected |
|---|------|----------|
| 1 | `should redirect unauthenticated to login` | / → redirects to OIDC login page |
| 2 | `should login with valid credentials` | Login → dashboard visible |
| 3 | `should show forbidden page for unauthorized route` | Non-admin → /admin/users → 403 page |

### User Management (4 tests)
| # | Test | Expected |
|---|------|----------|
| 4 | `should list users with pagination` | Table renders, pagination works |
| 5 | `should create user via modal` | Fill form → save → user appears |
| 6 | `should assign role to user` | Role dropdown → assign → updated |
| 7 | `should search users` | Type name → filtered results |

### Role & Access (3 tests)
| # | Test | Expected |
|---|------|----------|
| 8 | `should show roles page` | Roles table visible |
| 9 | `should show ABAC rules page` | ABAC table visible |
| 10 | `should manage delegations` | Create delegation → visible in list |

### Profile (2 tests)
| # | Test | Expected |
|---|------|----------|
| 11 | `should view own profile` | Profile page with user info |
| 12 | `should update profile fields` | Change display name → save → updated |

## Files to Create
- `e2e/fixtures/page-objects/user-management.po.ts`
- `e2e/identity/login-flow.spec.ts`
- `e2e/identity/user-management.spec.ts`
- `e2e/identity/role-assignment.spec.ts`
- `e2e/identity/profile.spec.ts`
