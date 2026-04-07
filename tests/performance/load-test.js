// GSDT — K6 load test
// Purpose: validate SLOs under sustained load (Phase 10: p95 read<200ms, write<400ms)
// Run: k6 run load-test.js --env BASE_URL=https://staging.internal --env API_KEY=<key>

import http from 'k6/http';
import { check, group, sleep } from 'k6';
import { Counter, Rate, Trend } from 'k6/metrics';
import { getJsonApiKeyHeaders } from './lib/auth.js';
import { randomRef, casePayload, parseBody } from './lib/helpers.js';

// ─── Config ──────────────────────────────────────────────────────────────────

const BASE_URL = __ENV.BASE_URL || 'https://staging.internal';
const IS_DOCKER = (__ENV.ENVIRONMENT || '').toLowerCase() === 'docker';

export const options = {
  stages: [
    // Ramp up to 500 VUs over 5 minutes
    { duration: '5m',  target: 500 },
    // Sustain 500 VUs for 20 minutes
    { duration: '20m', target: 500 },
    // Ramp down over 5 minutes
    { duration: '5m',  target: 0   },
  ],
  thresholds: {
    // Environment-aware SLO thresholds:
    // Docker/local: relaxed (container overhead + shared CPU)
    // Staging/prod: strict SLO targets
    'http_req_duration{operation:read}':  [IS_DOCKER ? 'p(95)<500' : 'p(95)<200'],
    'http_req_duration{operation:write}': [IS_DOCKER ? 'p(95)<800' : 'p(95)<400'],
    // Global error rate < 1%
    'http_req_failed': ['rate<0.01'],
  },
};

// ─── Custom metrics ───────────────────────────────────────────────────────────

const createErrors  = new Rate('create_errors');
const readErrors    = new Rate('read_errors');
const updateErrors  = new Rate('update_errors');
const deleteErrors  = new Rate('delete_errors');
const casesCreated  = new Counter('cases_created_total');

// ─── Main scenario ────────────────────────────────────────────────────────────

export default function () {
  const headers = getJsonApiKeyHeaders();

  // ── CREATE (write) ──────────────────────────────────────────────────────────
  let caseId = null;

  group('Create case', () => {
    const res = http.post(`${BASE_URL}/api/v1/cases`, casePayload(), {
      headers,
      tags: { operation: 'write' },
    });

    const ok = check(res, {
      'POST /api/v1/cases status 200': (r) => r.status === 200,
      'POST /api/v1/cases has id':     (r) => {
        const body = parseBody(r);
        return body?.success && body?.data?.id !== undefined;
      },
    });

    createErrors.add(!ok);

    if (ok) {
      const body = parseBody(res);
      if (body?.data?.id) {
        caseId = body.data.id;
        casesCreated.add(1);
      }
    }
  });

  sleep(0.2);

  // ── READ (read) ─────────────────────────────────────────────────────────────
  group('List cases', () => {
    const res = http.get(`${BASE_URL}/api/v1/cases?page=1&pageSize=20`, {
      headers,
      tags: { operation: 'read' },
    });

    const ok = check(res, {
      'GET /api/v1/cases status 200': (r) => r.status === 200,
      'GET /api/v1/cases has data':   (r) => {
        const body = parseBody(r);
        return body?.success && (Array.isArray(body?.data?.items) || Array.isArray(body?.data));
      },
    });

    readErrors.add(!ok);
  });

  sleep(0.2);

  // ── READ single (read) ──────────────────────────────────────────────────────
  if (caseId) {
    group('Get case by id', () => {
      const res = http.get(`${BASE_URL}/api/v1/cases/${caseId}`, {
        headers,
        tags: { operation: 'read' },
      });

      const ok = check(res, {
        'GET /api/v1/cases/:id status 200': (r) => r.status === 200,
      });

      readErrors.add(!ok);
    });

    sleep(0.1);

    // ── UPDATE (write) ────────────────────────────────────────────────────────
    group('Update case', () => {
      const payload = JSON.stringify({ title: 'Updated by load test', status: 'InProgress' });

      const res = http.put(`${BASE_URL}/api/v1/cases/${caseId}`, payload, {
        headers,
        tags: { operation: 'write' },
      });

      const ok = check(res, {
        'PUT /api/v1/cases/:id status 200': (r) => r.status === 200,
      });

      updateErrors.add(!ok);
    });

    sleep(0.1);

    // ── DELETE (write) ────────────────────────────────────────────────────────
    group('Delete case', () => {
      const res = http.del(`${BASE_URL}/api/v1/cases/${caseId}`, null, {
        headers,
        tags: { operation: 'write' },
      });

      const ok = check(res, {
        'DELETE /api/v1/cases/:id status 204': (r) => r.status === 204,
      });

      deleteErrors.add(!ok);
    });
  }

  sleep(0.5);
}
