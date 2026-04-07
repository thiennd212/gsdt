# Phase 3: Admin UI Browser Tests

## Overview
- **Priority:** P1 — largest coverage gap (29 admin + 16 user routes untested in browser)
- **Effort:** 4h
- **Status:** Done
- **Dependencies:** Phase 1 (POM + Fixtures)
- **Blocks:** Phase 5 (Error Scenarios)

## Context
- `browser-ui.spec.ts` covers only 5 routes: dashboard, cases, audit, files, notifications + 3 admin
- `full-route-rendering.spec.ts` is API-only (no browser rendering)
- 29 admin routes + 16 user-facing routes need browser-level verification
- All use Ant Design components: tables, forms, modals, drawers

## Route Inventory

### Admin Routes (29 total, 3 already tested)

| Route | Path | Group | Has Table | Has CRUD |
|-------|------|-------|-----------|----------|
| Dashboard | `/admin` | System | No | No |
| Users | `/admin/users` | Identity | Yes | Yes |
| Groups | `/admin/groups` | Identity | Yes | Yes |
| Roles | (user-level `/roles`) | Identity | Yes | Yes |
| Sessions | `/admin/sessions` | Identity | Yes | No (revoke) |
| External Identities | `/admin/external-identities` | Identity | Yes | Yes |
| Credential Policies | `/admin/credential-policies` | Identity | Yes | Yes |
| ABAC Rules | `/admin/abac-rules` | Identity | Yes | Yes |
| SoD Rules | `/admin/sod-rules` | Identity | Yes | Yes |
| Policy Rules | `/admin/policy-rules` | Identity | Yes | Yes |
| Data Scopes | `/admin/data-scopes` | Identity | Yes | Yes |
| Access Reviews | `/admin/access-reviews` | Identity | Yes | Yes |
| Delegations | `/admin/delegations` | Identity | Yes | Yes |
| JIT Provider Configs | `/admin/jit-provider-configs` | Identity | Yes | Yes |
| RTBF Requests | `/admin/rtbf` | Identity | Yes | Yes |
| System Params | `/admin/system-params` | System | Yes | Yes |
| API Keys | `/admin/api-keys` | System | Yes | Yes |
| Feature Flags | (via system-params) | System | Yes | Yes |
| Health | `/admin/health` | System | No | No |
| Backup | `/admin/backup` | System | No | Yes |
| Jobs | `/admin/jobs` | System | Yes | No |
| Menus | `/admin/menus` | Content | Yes | Yes |
| Templates | `/admin/templates` | Content | Yes | Yes |
| Notification Templates | `/admin/notification-templates` | Content | Yes | Yes |
| Master Data | `/admin/master-data` | Content | Yes | Yes |
| Organization | `/admin/organization` | Content | Tree | Yes |
| Workflow Definitions | `/admin/workflow` | Integration | Yes | Yes |
| Workflow Instances | `/admin/workflow/instances` | Integration | Yes | No |
| Workflow Assignments | `/admin/workflow-assignments` | Integration | Yes | No |
| Webhook Deliveries | `/admin/webhooks` | Integration | Yes | No |
| AI Config | `/admin/ai` | Integration | Yes | Yes |
| Rules | `/admin/rules` | Integration | Yes | Yes |

### User-Facing Routes (16 total, 5 already tested)

| Route | Path | Has Table |
|-------|------|-----------|
| Dashboard | `/` | No (cards) |
| Cases | `/cases` | Yes |
| Case Detail | `/cases/$caseId` | No (detail) |
| Forms | `/forms` | Yes |
| Form Detail | `/forms/$id` | No (detail) |
| Files | `/files` | Yes |
| Notifications | `/notifications` | Yes |
| Audit Logs | `/audit/logs` | Yes |
| Workflow Inbox | `/workflow-inbox` | Yes |
| Reports Definitions | `/reports/definitions` | Yes |
| Report Executions | `/reports/executions` | Yes |
| Search | `/search` | Yes |
| AI Search | `/ai/search` | No |
| Copilot | `/copilot` | No |
| Chat | `/chat` | No |
| Signatures | `/signatures` | Yes |
| Profile | `/profile` | No (form) |
| Consent | `/consent` | Yes |
| Integration Partners | `/integration/partners` | Yes |
| Integration Contracts | `/integration/contracts` | Yes |
| Integration Message Logs | `/integration/message-logs` | Yes |

## Files to Create

| File | Routes Covered | LOC est |
|------|----------------|---------|
| `e2e/browser-ui/admin-identity.spec.ts` | Users, Groups, Sessions, ExtIdentities, CredPolicies, ABAC, SoD, Policy, DataScopes, AccessReviews, Delegations, JIT, RTBF | ~180 |
| `e2e/browser-ui/admin-content.spec.ts` | Menus, Templates, NotifTemplates, MasterData, Organization | ~120 |
| `e2e/browser-ui/admin-system.spec.ts` | Dashboard, SystemParams, APIKeys, Health, Backup, Jobs | ~100 |
| `e2e/browser-ui/admin-integration.spec.ts` | Workflow (defs, instances, assignments), Webhooks, AI, Rules | ~120 |
| `e2e/browser-ui/user-pages.spec.ts` | Cases, Forms, Files, Notifications, AuditLogs, WorkflowInbox, Reports, Search, AI, Copilot, Chat, Signatures, Profile, Consent, Integration | ~180 |

## Implementation Pattern

Each spec file follows the same pattern:

```typescript
import { test, expect } from '../../fixtures/auth-fixture';
import { AdminLayoutPage } from '../../pages/admin-layout-page';
import { DataTablePage } from '../../pages/data-table-page';
import { navigateAndWait } from '../../helpers/navigation-helper';

const WEB_URL = process.env.WEB_URL ?? 'http://localhost:3000';

test.describe('Admin Identity Routes', () => {
  test.describe.configure({ mode: 'serial' });
  test.setTimeout(60_000);

  // ---------- Render Tests ----------
  // For each route: navigate, verify no JS errors, verify content renders
  
  test('/admin/users renders user table', async ({ authedPage }) => {
    const { errors } = await navigateAndWait(authedPage, `${WEB_URL}/admin/users`);
    expect(errors).toHaveLength(0);
    
    const table = new DataTablePage(authedPage);
    const rowCount = await table.getRowCount();
    expect(rowCount).toBeGreaterThan(0); // dev env has admin user
  });

  // ---------- Basic CRUD Tests ----------
  // For routes with CRUD: open create modal, fill, submit, verify row added
  
  test('/admin/groups CRUD — create + verify + delete', async ({ authedPage, testData }) => {
    await navigateAndWait(authedPage, `${WEB_URL}/admin/groups`);
    
    // Click create button
    await authedPage.getByRole('button', { name: /thêm|tạo|create|add/i }).click();
    
    const modal = new FormModalPage(authedPage);
    await modal.waitForOpen();
    await modal.fillField('Tên nhóm', `E2E Test Group ${Date.now()}`);
    await modal.submit();
    
    // Verify success notification
    await expect(authedPage.locator('.ant-notification-notice, .ant-message')).toBeVisible();
  });
});
```

## Implementation Steps

### 1. Create `e2e/browser-ui/admin-identity.spec.ts`

Tests for 13 identity-related admin routes:

| Test Name | Route | Assertions |
|-----------|-------|------------|
| renders-users | /admin/users | Table visible, >0 rows |
| renders-groups | /admin/groups | Table visible |
| renders-sessions | /admin/sessions | Table visible |
| renders-external-identities | /admin/external-identities | Page renders (may need userId param) |
| renders-credential-policies | /admin/credential-policies | Table visible |
| renders-abac-rules | /admin/abac-rules | Table visible |
| renders-sod-rules | /admin/sod-rules | Table visible |
| renders-policy-rules | /admin/policy-rules | Table visible |
| renders-data-scopes | /admin/data-scopes | Table visible |
| renders-access-reviews | /admin/access-reviews | Table visible |
| renders-delegations | /admin/delegations | Table visible |
| renders-jit-providers | /admin/jit-provider-configs | Table visible |
| renders-rtbf | /admin/rtbf | Table visible |

### 2. Create `e2e/browser-ui/admin-content.spec.ts`

Tests for 5 content-related admin routes:

| Test Name | Route | Assertions |
|-----------|-------|------------|
| renders-menus | /admin/menus | Table visible |
| renders-templates | /admin/templates | Table visible |
| renders-notification-templates | /admin/notification-templates | Table visible |
| renders-master-data | /admin/master-data | Table visible |
| renders-organization | /admin/organization | Tree or table visible |
| crud-notification-template | /admin/notification-templates | Create modal, fill, submit |

### 3. Create `e2e/browser-ui/admin-system.spec.ts`

Tests for 6 system-related admin routes:

| Test Name | Route | Assertions |
|-----------|-------|------------|
| renders-dashboard | /admin | Dashboard content visible |
| renders-system-params | /admin/system-params | Table visible |
| renders-api-keys | /admin/api-keys | Table visible |
| renders-health | /admin/health | Health status visible |
| renders-backup | /admin/backup | Backup page visible |
| renders-jobs | /admin/jobs | Jobs table visible |
| crud-system-param | /admin/system-params | Inline edit, save |

### 4. Create `e2e/browser-ui/admin-integration.spec.ts`

Tests for 6 integration-related admin routes:

| Test Name | Route | Assertions |
|-----------|-------|------------|
| renders-workflow | /admin/workflow | Table visible |
| renders-workflow-instances | /admin/workflow/instances | Table visible |
| renders-workflow-assignments | /admin/workflow-assignments | Table visible |
| renders-webhooks | /admin/webhooks | Table visible |
| renders-ai | /admin/ai | AI config visible |
| renders-rules | /admin/rules | Table visible |

### 5. Create `e2e/browser-ui/user-pages.spec.ts`

Tests for untested user-facing routes:

| Test Name | Route | Assertions |
|-----------|-------|------------|
| renders-workflow-inbox | /workflow-inbox | Table or empty state |
| renders-forms | /forms | Table visible |
| renders-reports-definitions | /reports/definitions | Table visible |
| renders-report-executions | /reports/executions | Table visible |
| renders-search | /search | Search input visible |
| renders-ai-search | /ai/search | Search interface visible |
| renders-copilot | /copilot | Chat interface visible |
| renders-chat | /chat | Chat interface visible |
| renders-signatures | /signatures | Table visible |
| renders-profile | /profile | Form visible |
| renders-consent | /consent | Table or list visible |
| renders-roles | /roles | Table visible |
| renders-integration-partners | /integration/partners | Table visible |
| renders-integration-contracts | /integration/contracts | Table visible |
| renders-integration-message-logs | /integration/message-logs | Table visible |

### 6. Update `playwright.config.ts`

Add project entry for new browser-ui subfolder:
```typescript
{
  name: 'admin-browser',
  testMatch: /browser-ui\//,
  use: { ...devices['Desktop Chrome'] },
},
```

## Test Matrix Summary

| Category | Render Tests | CRUD Tests | Total |
|----------|-------------|------------|-------|
| Admin Identity | 13 | 2 (groups, users) | 15 |
| Admin Content | 5 | 1 (notif-templates) | 6 |
| Admin System | 6 | 1 (system-params) | 7 |
| Admin Integration | 6 | 0 | 6 |
| User Pages | 15 | 0 | 15 |
| **Total** | **45** | **4** | **49** |

## Success Criteria
- [x] All 45 route render tests pass (page loads, no JS errors, content visible)
- [x] 4 CRUD tests pass (create entity via modal, verify in table)
- [x] All tests use POM from Phase 1 (no inline selectors)
- [x] Each spec file under 200 lines
- [x] Tests run in serial mode per file (share authenticated page)
- [x] No flaky tests (3 consecutive green runs)

## Risk Assessment
| Risk | Mitigation |
|------|------------|
| Some routes need URL params ($caseId, $instanceId) | Skip detail routes or use API to get first valid ID |
| Routes may require specific data to render table | Accept empty-state as valid render |
| 49 tests may be slow | Serial within group, parallel across groups |
| Some admin routes may 403 for dev admin | Assert 403 shows access-denied page, not crash |

## Security Considerations
- All admin routes guarded by `adminRoute` layout (role check)
- Test uses admin@dev.local which has Admin role
- No credential exposure beyond existing auth-helper.ts patterns
