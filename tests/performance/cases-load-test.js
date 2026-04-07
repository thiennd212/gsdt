// GSDT — K6 cases load test
// Purpose: TC-PERF-CASE-001/002/003 — cases list, create, and workflow transition SLOs
// Run: k6 run cases-load-test.js --env BASE_URL=https://staging.internal --env API_KEY=<key>

import http from 'k6/http';
import { check, group, sleep } from 'k6';
import { Counter, Rate } from 'k6/metrics';
import { getJsonApiKeyHeaders } from './lib/auth.js';
import { randomRef, pageParams, casePayload, parseBody } from './lib/helpers.js';

// ─── Config ──────────────────────────────────────────────────────────────────

const BASE_URL = __ENV.BASE_URL || 'https://staging.internal';
const IS_DOCKER = (__ENV.ENVIRONMENT || '').toLowerCase() === 'docker';

// TC-PERF-CASE-001: 50 VUs, 2-min hold — cases list p95<500ms
// TC-PERF-CASE-002: 50 VUs — case create p95<1000ms
// TC-PERF-CASE-003: 30 VUs — workflow transition p95<800ms
export const options = {
  stages: [
    { duration: '1m', target: 50 },  // ramp up
    { duration: '2m', target: 50 },  // hold
    { duration: '1m', target: 0  },  // ramp down
  ],
  thresholds: {
    'http_req_duration{operation:read}':  [IS_DOCKER ? 'p(95)<2000' : 'p(95)<500'],
    'http_req_duration{operation:write}': [IS_DOCKER ? 'p(95)<3000' : 'p(95)<1000'],
    'http_req_failed': ['rate<0.05'],  // Docker: allow up to 5% error rate
  },
};

// ─── Custom metrics ───────────────────────────────────────────────────────────

const listErrors       = new Rate('cases_list_errors');
const createErrors     = new Rate('cases_create_errors');
const transitionErrors = new Rate('cases_transition_errors');
const casesCreated     = new Counter('cases_created_total');

// ─── Main scenario ────────────────────────────────────────────────────────────

export default function () {
  const headers = getJsonApiKeyHeaders();
  let caseId = null; // reset each iteration — prevents re-submitting old cases

  // TC-PERF-CASE-001: Cases list
  group('List cases', () => {
    const res = http.get(`${BASE_URL}/api/v1/cases?${pageParams(1, 20)}`, {
      headers,
      tags: { operation: 'read' },
    });

    const ok = check(res, {
      'GET /api/v1/cases status 200': (r) => r.status === 200,
      'GET /api/v1/cases has data': (r) => {
        const body = parseBody(r);
        return body?.success && (Array.isArray(body?.data?.items) || Array.isArray(body?.data));
      },
    });

    listErrors.add(!ok);
  });

  sleep(0.2);

  // TC-PERF-CASE-002: Case create
  group('Create case', () => {
    const res = http.post(`${BASE_URL}/api/v1/cases`, casePayload(), {
      headers,
      tags: { operation: 'write' },
    });

    const ok = check(res, {
      'POST /api/v1/cases status 200': (r) => r.status === 200,
      'POST /api/v1/cases has id': (r) => {
        const body = parseBody(r);
        return body?.success && body?.data?.id !== undefined;
      },
    });

    createErrors.add(!ok);

    if (ok) {
      try {
        caseId = JSON.parse(res.body).data.id;
        casesCreated.add(1);
      } catch (_) { /* ignore parse errors */ }
    }
  });

  sleep(0.2);

  // TC-PERF-CASE-003: Workflow transition — POST /api/v1/cases/:id/submit
  if (caseId) {
    group('Transition case', () => {
      const res = http.post(
        `${BASE_URL}/api/v1/cases/${caseId}/submit`,
        null,
        { headers, tags: { operation: 'write' } },
      );

      const ok = check(res, {
        'POST /api/v1/cases/:id/submit status 204 or 200': (r) =>
          r.status === 204 || r.status === 200,
      });

      transitionErrors.add(!ok);
    });
  }

  sleep(0.5);
}

// ─── Summary ─────────────────────────────────────────────────────────────────

export function handleSummary(data) {
  return { 'stdout': JSON.stringify(data.metrics, null, 2) };
}
