// GSDT — K6 search load test
// Purpose: TC-PERF-SEARCH-001 — full-text search SLO (Vietnamese terms)
// Run: k6 run search-load-test.js --env BASE_URL=https://staging.internal --env API_KEY=<key>

import http from 'k6/http';
import { check, group, sleep } from 'k6';
import { Rate, Trend } from 'k6/metrics';
import { getJsonApiKeyHeaders } from './lib/auth.js';
import { pageParams, parseBody } from './lib/helpers.js';

// ─── Config ──────────────────────────────────────────────────────────────────

const BASE_URL = __ENV.BASE_URL || 'https://staging.internal';
const IS_DOCKER = (__ENV.ENVIRONMENT || '').toLowerCase() === 'docker';

// TC-PERF-SEARCH-001: 30 VUs — full-text search p95<500ms
export const options = {
  stages: [
    { duration: '1m', target: 30 },  // ramp up
    { duration: '2m', target: 30 },  // hold
    { duration: '1m', target: 0  },  // ramp down
  ],
  thresholds: {
    'http_req_duration{operation:search}': [IS_DOCKER ? 'p(95)<1000' : 'p(95)<500'],
    'http_req_failed': ['rate<0.01'],
  },
};

// ─── Search term pool — realistic Vietnamese domain terms ─────────────────────

const SEARCH_TERMS = [
  'khiếu nại',   // complaint
  'hồ sơ',       // dossier/file
  'báo cáo',     // report
  'quyết định',  // decision
];

// ─── Custom metrics ───────────────────────────────────────────────────────────

const searchErrors   = new Rate('search_errors');
const searchDuration = new Trend('search_duration');
const emptyResults   = new Rate('search_empty_results');

// ─── Main scenario ────────────────────────────────────────────────────────────

export default function () {
  const headers = getJsonApiKeyHeaders();

  // Round-robin through search terms so each VU uses a different term
  const term = SEARCH_TERMS[(__VU - 1) % SEARCH_TERMS.length];
  const encodedTerm = encodeURIComponent(term);

  // TC-PERF-SEARCH-001: Full-text search
  group('Full-text search', () => {
    const res = http.get(
      `${BASE_URL}/api/v1/cases?search=${encodedTerm}&${pageParams(1, 20)}`,
      { headers, tags: { operation: 'search' } },
    );

    const ok = check(res, {
      'GET /api/v1/cases?search= status 200': (r) => r.status === 200,
      'GET /api/v1/cases?search= has data items': (r) => {
        const body = parseBody(r);
        return Array.isArray(body?.data?.items);
      },
      'GET /api/v1/cases?search= response under threshold': (r) =>
        r.timings.duration < (IS_DOCKER ? 1000 : 500),
    });

    searchErrors.add(!ok);
    searchDuration.add(res.timings.duration);

    // Track how often search returns no results (useful for index health)
    if (ok) {
      const body = parseBody(res);
      emptyResults.add((body?.data?.items?.length ?? 0) === 0 ? 1 : 0);
    }
  });

  sleep(0.5);
}

// ─── Summary ─────────────────────────────────────────────────────────────────

export function handleSummary(data) {
  const p95        = data.metrics['search_duration']?.values?.['p(95)'] ?? 0;
  const emptyRate  = data.metrics['search_empty_results']?.values?.rate ?? 0;

  console.log('\n=== SEARCH LOAD TEST SUMMARY ===');
  console.log(`Search p95 (ms)     : ${Math.round(p95)}`);
  console.log(`Empty result rate   : ${(emptyRate * 100).toFixed(1)}%`);
  console.log(`Terms tested        : ${SEARCH_TERMS.join(', ')}`);

  return { 'stdout': JSON.stringify(data.metrics, null, 2) };
}
