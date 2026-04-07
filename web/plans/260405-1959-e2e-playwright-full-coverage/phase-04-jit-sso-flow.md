# Phase 4: JIT SSO Flow

## Overview
- **Priority:** P1 — backlog priority (JIT SSO is top priority feature)
- **Effort:** 2h
- **Status:** Done
- **Dependencies:** Phase 1 (POM + Fixtures)
- **Blocks:** None

## Context
- JIT SSO = Just-In-Time user provisioning via external SSO provider
- Admin configures provider → external user logs in via SSO → system auto-creates local user
- Route: `/admin/jit-provider-configs` — admin manages provider configs
- Related: `/admin/external-identities` — linked external accounts
- No real external IdP in dev — must mock via Playwright route interception

## Key Insights
- JIT SSO flow depends on OIDC authorization_code grant (not ROPC)
- Provider config has: name, issuer URL, clientId, clientSecret, domain whitelist, auto-approve flag
- When auto-approve=false, admin must manually approve JIT-provisioned users
- Domain whitelist rejects emails from non-allowed domains
- External identity links to local user after provisioning

## Data Flow

```
Admin                          Mock IdP                    System
  |                              |                           |
  |-- create provider config     |                           |
  |   (issuer, domain whitelist) |                           |
  |                              |                           |
  |                              |                           |
User ──── SSO login ────────────>|                           |
  |                              |-- auth code callback ───> |
  |                              |                           |-- validate token
  |                              |                           |-- check domain whitelist
  |                              |                           |-- create local user (JIT)
  |                              |                           |-- link external identity
  |<──── authenticated ──────────|                           |
  |                              |                           |
Admin                                                        |
  |-- view users list           ─────────────────────────────|
  |-- see JIT user (pending/approved)                        |
  |-- approve if auto-approve=false                          |
```

## Files to Create

| File | Purpose | LOC est |
|------|---------|---------|
| `e2e/jit-sso-flow.spec.ts` | Full JIT SSO test with mocked IdP | ~150 |
| `e2e/helpers/mock-idp-helper.ts` | Playwright route intercept to simulate external IdP | ~80 |
| `e2e/jit-sso-admin-config.spec.ts` | Admin CRUD for JIT provider configs | ~80 |

## Implementation Steps

### 1. Create `e2e/helpers/mock-idp-helper.ts` — Mock IdP via route intercept

```typescript
import { Page } from '@playwright/test';

interface MockIdpConfig {
  issuerUrl: string;
  email: string;
  name: string;
  sub: string;  // external user ID
}

/**
 * Intercept OIDC discovery + token endpoints to simulate external IdP.
 * Uses Playwright route() to mock HTTP responses without a real IdP server.
 */
export class MockIdpHelper {
  constructor(private page: Page, private config: MockIdpConfig) {}

  async setup() {
    // Intercept OIDC discovery
    await this.page.route(`${this.config.issuerUrl}/.well-known/openid-configuration`, (route) => {
      route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          issuer: this.config.issuerUrl,
          authorization_endpoint: `${this.config.issuerUrl}/authorize`,
          token_endpoint: `${this.config.issuerUrl}/token`,
          userinfo_endpoint: `${this.config.issuerUrl}/userinfo`,
          jwks_uri: `${this.config.issuerUrl}/.well-known/jwks`,
        }),
      });
    });

    // Intercept token exchange — return mock token
    await this.page.route(`${this.config.issuerUrl}/token`, (route) => {
      route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          access_token: 'mock-access-token',
          id_token: this.buildMockIdToken(),
          token_type: 'Bearer',
          expires_in: 3600,
        }),
      });
    });

    // Intercept userinfo
    await this.page.route(`${this.config.issuerUrl}/userinfo`, (route) => {
      route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          sub: this.config.sub,
          email: this.config.email,
          name: this.config.name,
        }),
      });
    });
  }

  private buildMockIdToken(): string {
    // Build unsigned JWT with claims (test only — not verified by backend in mock mode)
    const header = btoa(JSON.stringify({ alg: 'none', typ: 'JWT' }));
    const payload = btoa(JSON.stringify({
      sub: this.config.sub,
      email: this.config.email,
      name: this.config.name,
      iss: this.config.issuerUrl,
      aud: 'test-client',
      exp: Math.floor(Date.now() / 1000) + 3600,
    }));
    return `${header}.${payload}.`;
  }
}
```

### 2. Create `e2e/jit-sso-admin-config.spec.ts` — Admin provider config CRUD

```typescript
test.describe('JIT SSO Provider Config Management', () => {
  test.describe.configure({ mode: 'serial' });

  test('/admin/jit-provider-configs renders', async ({ authedPage }) => {
    await navigateAndWait(authedPage, `${WEB_URL}/admin/jit-provider-configs`);
    // Assert page renders
  });

  test('create JIT provider config', async ({ authedPage }) => {
    // Click add button
    // Fill modal: name, issuer URL, client ID, client secret, domain whitelist
    // Submit
    // Verify row appears in table
  });

  test('edit JIT provider config', async ({ authedPage }) => {
    // Click edit on created row
    // Modify domain whitelist
    // Save
    // Verify updated value
  });

  test('delete JIT provider config', async ({ authedPage }) => {
    // Click delete on created row
    // Confirm deletion
    // Verify row removed
  });
});
```

### 3. Create `e2e/jit-sso-flow.spec.ts` — Full JIT SSO E2E flow

```typescript
test.describe('JIT SSO Provisioning Flow', () => {
  test.describe.configure({ mode: 'serial' });

  const MOCK_ISSUER = 'https://mock-idp.test';
  const JIT_EMAIL = 'jit-user@allowed-domain.com';
  
  let providerConfigId: string;

  test('admin creates JIT provider config via API', async ({ apiToken, request }) => {
    // POST /api/v1/admin/jit-provider-configs
    // Configure: issuer=MOCK_ISSUER, domainWhitelist=['allowed-domain.com'], autoApprove=false
    // Store providerConfigId
  });

  test('SSO callback triggers JIT user creation', async ({ apiToken, request }) => {
    // Simulate what happens after SSO callback:
    // POST /api/v1/identity/jit-provision (or equivalent internal endpoint)
    // Body: { providerId, externalSub, email, name }
    // Assert: 200/201, user created in pending state
  });

  test('admin sees JIT user in pending state', async ({ authedPage }) => {
    await navigateAndWait(authedPage, `${WEB_URL}/admin/users`);
    // Search for JIT_EMAIL
    // Assert user appears with pending/unapproved status
  });

  test('admin approves JIT user', async ({ authedPage }) => {
    // Find JIT user row
    // Click approve button
    // Confirm
    // Assert status changes to active/approved
  });

  test('domain whitelist rejects non-allowed domain', async ({ apiToken, request }) => {
    // POST /api/v1/identity/jit-provision with email @blocked-domain.com
    // Assert: 403 or 422, user NOT created
  });

  test('cleanup: delete JIT user + provider config', async ({ apiToken, request }) => {
    // DELETE user, DELETE provider config
  });
});
```

### 4. Alternative: API-level JIT SSO test (if browser mock too complex)

If route interception proves unreliable for OIDC flow, fall back to pure API testing:

```typescript
test.describe('JIT SSO API Flow', () => {
  test('POST /admin/jit-provider-configs creates config', async ({ request, apiToken }) => {
    // CRUD via API
  });

  test('JIT provision endpoint creates user', async ({ request, apiToken }) => {
    // Direct API call simulating SSO callback result
  });

  test('domain whitelist enforcement', async ({ request, apiToken }) => {
    // API call with blocked domain
  });
});
```

## Test Matrix

| Test | Scenario | Expected Result |
|------|----------|----------------|
| admin-config/renders | Navigate to /admin/jit-provider-configs | Page renders, table visible |
| admin-config/create | Add new provider config | Row appears in table |
| admin-config/edit | Modify domain whitelist | Updated value saved |
| admin-config/delete | Remove provider config | Row removed |
| sso-flow/create-config | API: create provider config | 200, ID returned |
| sso-flow/jit-provision | Simulate SSO callback | User created, pending status |
| sso-flow/admin-sees-pending | Navigate to users, find JIT user | User visible, pending badge |
| sso-flow/admin-approves | Approve JIT user | Status changes to active |
| sso-flow/domain-reject | Provision with blocked domain | 403/422, no user created |
| sso-flow/cleanup | Delete user + config | Entities removed |

## Success Criteria
- [x] JIT provider config CRUD works in browser
- [x] JIT provisioning creates user (via API simulation or mocked SSO)
- [x] Pending approval flow: admin sees + approves JIT user
- [x] Domain whitelist rejects non-allowed domains
- [x] All test data cleaned up after run

## Risk Assessment
| Risk | Likelihood | Mitigation |
|------|-----------|------------|
| Mock IdP route intercept doesn't work for server-side OIDC | High | Fall back to API-level test (Step 4) |
| JIT provision endpoint may not exist yet | Medium | Check API first, skip with descriptive message |
| OIDC flow too complex to mock in Playwright | Medium | Test admin config CRUD + API provision separately |
| Token validation rejects unsigned JWT | High | Use API-level simulation instead of browser mock |

## Decision: Mock Strategy

**Recommended approach: API-level simulation** (Step 4) rather than browser-level mock.

Rationale:
1. OIDC token validation happens server-side — Playwright route intercept only mocks client-side HTTP
2. Server validates JWT signatures, not just claims
3. API-level test covers the same business logic (JIT provisioning, domain whitelist, approval flow)
4. Browser test for admin config CRUD separately verifies the UI

This gives full coverage without the fragility of mocking a real OIDC flow.
