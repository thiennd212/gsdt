# Phase 6: Performance Baselines

## Overview
- **Priority:** P3 — observability layer, catches regressions
- **Effort:** 1h
- **Status:** Done
- **Dependencies:** Phase 1 (POM + Fixtures)
- **Blocks:** None

## Context
- No performance assertions in current test suite
- Playwright provides `page.evaluate()` access to Performance API and PerformanceObserver
- Core Web Vitals (LCP, FID/INP, CLS) measurable via browser APIs
- API response times measurable via Playwright request timing
- Baseline thresholds should be generous initially, tightened after data collection

## Key Insights
- Page load includes OIDC auth check + API data fetch + React render
- Dashboard is heaviest page (multiple API calls, charts)
- Admin tables do server-side pagination — initial load should be fast
- Ant Design bundle is large (~300KB gzipped) — affects FCP
- Dev server (Vite) is faster than production build for HMR but slower for initial load

## Files to Create

| File | Purpose | LOC est |
|------|---------|---------|
| `e2e/performance-baselines.spec.ts` | Page load + API + Core Web Vitals assertions | ~120 |
| `e2e/helpers/performance-helper.ts` | Helpers to measure timing metrics | ~60 |

## Threshold Strategy

Initial thresholds (generous, tighten after 2 weeks of data):

| Metric | Threshold | Rationale |
|--------|-----------|-----------|
| Page load (domcontentloaded) | <3000ms | Includes OIDC check + API |
| Page load (networkidle) | <8000ms | Includes lazy chunks + data |
| API response (list endpoints) | <500ms | Paginated, should be fast |
| API response (detail endpoints) | <300ms | Single record |
| LCP (Largest Contentful Paint) | <4000ms | Google "needs improvement" threshold |
| CLS (Cumulative Layout Shift) | <0.25 | Google "needs improvement" threshold |
| Bundle size (main chunk) | <500KB | Ant Design + React + TanStack |

## Implementation Steps

### 1. Create `e2e/helpers/performance-helper.ts`

```typescript
import { Page } from '@playwright/test';

export interface PageMetrics {
  domContentLoaded: number;  // ms from navigationStart
  loadComplete: number;      // ms from navigationStart
  lcp: number | null;        // Largest Contentful Paint
  cls: number | null;        // Cumulative Layout Shift
  resourceCount: number;     // number of resources loaded
  transferSize: number;      // total bytes transferred
}

/**
 * Measure page load performance metrics using Performance API.
 * Call AFTER page.goto() and page.waitForLoadState('networkidle').
 */
export async function measurePageMetrics(page: Page): Promise<PageMetrics> {
  return page.evaluate(() => {
    const nav = performance.getEntriesByType('navigation')[0] as PerformanceNavigationTiming;
    const resources = performance.getEntriesByType('resource') as PerformanceResourceTiming[];
    
    // LCP — last paint entry
    let lcp: number | null = null;
    const lcpEntries = performance.getEntriesByType('largest-contentful-paint');
    if (lcpEntries.length > 0) {
      lcp = lcpEntries[lcpEntries.length - 1].startTime;
    }
    
    // CLS — sum of layout shift values
    let cls: number | null = null;
    const layoutShifts = performance.getEntriesByType('layout-shift') as any[];
    if (layoutShifts.length > 0) {
      cls = layoutShifts
        .filter((e) => !e.hadRecentInput)
        .reduce((sum, e) => sum + e.value, 0);
    }
    
    const transferSize = resources.reduce((sum, r) => sum + (r.transferSize || 0), 0);
    
    return {
      domContentLoaded: nav.domContentLoadedEventEnd - nav.startTime,
      loadComplete: nav.loadEventEnd - nav.startTime,
      lcp,
      cls,
      resourceCount: resources.length,
      transferSize,
    };
  });
}

/**
 * Measure API response time for a single request.
 */
export async function measureApiResponseTime(
  request: any,
  url: string,
  headers: Record<string, string>,
): Promise<{ status: number; durationMs: number }> {
  const start = Date.now();
  const res = await request.get(url, { headers });
  const durationMs = Date.now() - start;
  return { status: res.status(), durationMs };
}
```

### 2. Create `e2e/performance-baselines.spec.ts`

```typescript
import { test, expect } from '@playwright/test';
import { getAccessToken, authHeaders, API_BASE } from './helpers/auth-helper';
import { measurePageMetrics, measureApiResponseTime } from './helpers/performance-helper';

const WEB_URL = process.env.WEB_URL ?? 'http://localhost:3000';

test.describe('Performance Baselines', () => {
  test.setTimeout(60_000);

  // ── Page Load Performance ──────────────────────────────────────────

  test.describe('Page Load Times', () => {
    test.describe.configure({ mode: 'serial' });

    let authedPage: any;

    test.beforeAll(async ({ browser }) => {
      test.setTimeout(120_000);
      const ctx = await browser.newContext();
      authedPage = await ctx.newPage();
      // Login via OIDC
      await authedPage.goto(WEB_URL);
      await authedPage.waitForLoadState('domcontentloaded');
      // ... login flow (use LoginPage POM)
    });

    test.afterAll(async () => {
      await authedPage?.context().close();
    });

    const PAGES_TO_MEASURE = [
      { name: 'Dashboard', path: '/' },
      { name: 'Cases', path: '/cases' },
      { name: 'Admin Users', path: '/admin/users' },
      { name: 'Forms', path: '/forms' },
      { name: 'Audit Logs', path: '/audit/logs' },
      { name: 'Admin System Params', path: '/admin/system-params' },
    ];

    for (const { name, path } of PAGES_TO_MEASURE) {
      test(`${name} loads within threshold`, async () => {
        await authedPage.goto(`${WEB_URL}${path}`);
        await authedPage.waitForLoadState('networkidle');

        const metrics = await measurePageMetrics(authedPage);

        console.log(`[PERF] ${name}:`, {
          domContentLoaded: `${metrics.domContentLoaded.toFixed(0)}ms`,
          loadComplete: `${metrics.loadComplete.toFixed(0)}ms`,
          lcp: metrics.lcp ? `${metrics.lcp.toFixed(0)}ms` : 'N/A',
          cls: metrics.cls?.toFixed(3) ?? 'N/A',
          resources: metrics.resourceCount,
          transferred: `${(metrics.transferSize / 1024).toFixed(0)}KB`,
        });

        // Assertions — generous thresholds
        expect(metrics.domContentLoaded).toBeLessThan(3000);
        expect(metrics.loadComplete).toBeLessThan(8000);

        if (metrics.lcp !== null) {
          expect(metrics.lcp).toBeLessThan(4000);
        }
        if (metrics.cls !== null) {
          expect(metrics.cls).toBeLessThan(0.25);
        }
      });
    }
  });

  // ── API Response Time ──────────────────────────────────────────────

  test.describe('API Response Times', () => {
    let token: string;

    test.beforeAll(async ({ request }) => {
      token = await getAccessToken(request);
    });

    const LIST_ENDPOINTS = [
      '/cases?page=1&pageSize=10',
      '/forms/templates?page=1&pageSize=10',
      '/notifications?page=1&pageSize=10',
      '/admin/users?page=1&pageSize=10',
      '/admin/system-params',
      '/audit/logs?page=1&pageSize=10',
    ];

    for (const ep of LIST_ENDPOINTS) {
      test(`API ${ep.split('?')[0]} responds within 500ms`, async ({ request }) => {
        const { status, durationMs } = await measureApiResponseTime(
          request,
          `${API_BASE}/api/v1${ep}`,
          authHeaders(token),
        );

        console.log(`[PERF] API ${ep.split('?')[0]}: ${durationMs}ms (${status})`);
        expect(status).toBeLessThan(500); // not a server error
        expect(durationMs).toBeLessThan(500);
      });
    }

    const DETAIL_ENDPOINTS = [
      '/notifications/unread-count',
      '/admin/data-scopes/types',
      '/announcements/active',
    ];

    for (const ep of DETAIL_ENDPOINTS) {
      test(`API ${ep} responds within 300ms`, async ({ request }) => {
        const { status, durationMs } = await measureApiResponseTime(
          request,
          `${API_BASE}/api/v1${ep}`,
          authHeaders(token),
        );

        console.log(`[PERF] API ${ep}: ${durationMs}ms (${status})`);
        expect(status).toBeLessThan(500);
        expect(durationMs).toBeLessThan(300);
      });
    }
  });

  // ── Auth Token Performance ─────────────────────────────────────────

  test.describe('Auth Performance', () => {
    test('ROPC token acquisition under 1s', async ({ request }) => {
      const start = Date.now();
      const token = await getAccessToken(request);
      const elapsed = Date.now() - start;

      console.log(`[PERF] Token acquisition: ${elapsed}ms`);
      expect(token).toBeTruthy();
      expect(elapsed).toBeLessThan(1000);
    });
  });
});
```

### 3. Update `playwright.config.ts` — add perf project

```typescript
{
  name: 'e2e-perf',
  testMatch: /performance-baselines/,
  use: { ...devices['Desktop Chrome'] },
},
```

## Test Matrix

| Test | Scenario | Threshold |
|------|----------|-----------|
| pageload/dashboard | Navigate to / after auth | DOMContentLoaded <3s |
| pageload/cases | Navigate to /cases | DOMContentLoaded <3s |
| pageload/admin-users | Navigate to /admin/users | DOMContentLoaded <3s |
| pageload/forms | Navigate to /forms | DOMContentLoaded <3s |
| pageload/audit-logs | Navigate to /audit/logs | DOMContentLoaded <3s |
| pageload/system-params | Navigate to /admin/system-params | DOMContentLoaded <3s |
| cwv/lcp | All measured pages | LCP <4s |
| cwv/cls | All measured pages | CLS <0.25 |
| api/cases | GET /cases (paginated) | <500ms |
| api/forms | GET /forms/templates | <500ms |
| api/notifications | GET /notifications | <500ms |
| api/users | GET /admin/users | <500ms |
| api/system-params | GET /admin/system-params | <500ms |
| api/audit-logs | GET /audit/logs | <500ms |
| api/unread-count | GET /notifications/unread-count | <300ms |
| api/data-scopes | GET /admin/data-scopes/types | <300ms |
| api/announcements | GET /announcements/active | <300ms |
| auth/token | ROPC token acquisition | <1s |

## Success Criteria
- [x] All 6 page load tests pass with DOMContentLoaded <3s
- [x] All 9 API response time tests pass
- [x] LCP <4s and CLS <0.25 where measurable
- [x] Token acquisition <1s
- [x] Performance metrics logged to console for baseline collection
- [x] No test flakes (perf tests are inherently variable — thresholds are generous)

## Risk Assessment
| Risk | Likelihood | Mitigation |
|------|-----------|------------|
| Perf tests flaky due to machine load | Medium | 2x generous thresholds, skip in CI if unstable |
| LCP/CLS APIs not available in headless Chrome | Low | Null-check, skip assertion if unavailable |
| Cold start (first run after deploy) is slower | Medium | Run page load twice, measure second |
| Dev server perf differs from production | Medium | Document as dev-only baselines, separate prod suite later |

## Future Improvements (Not in scope)
- Lighthouse CI integration for comprehensive Core Web Vitals
- Performance trend tracking (save metrics to file, compare across runs)
- Bundle size regression checks (compare main.js size against budget)
- Memory leak detection (long-running page, monitor heap growth)
