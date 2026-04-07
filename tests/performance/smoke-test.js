// GSDT — K6 smoke test
// Purpose: validate baseline health after every deployment (fast, low VUs)
// Run: k6 run smoke-test.js --env BASE_URL=https://staging.internal

import http from 'k6/http';
import { check, sleep } from 'k6';
import { Rate } from 'k6/metrics';
import { parseBody } from './lib/helpers.js';

// ─── Config ──────────────────────────────────────────────────────────────────

const BASE_URL = __ENV.BASE_URL || 'https://staging.internal';
const IS_DOCKER = (__ENV.ENVIRONMENT || '').toLowerCase() === 'docker';

export const options = {
  vus: 5,
  duration: '1m',
  // Treat 401 (unauthenticated) as expected — not a failure
  // Only 5xx and unexpected 4xx (not 401) count as failures
  setupTimeout: '30s',
  thresholds: {
    // Less than 1% of requests may fail (401 excluded via setResponseCallback)
    http_req_failed: ['rate<0.01'],
    // Environment-aware p95 threshold:
    // Docker/local: <500ms (Class A SLA — container overhead)
    // Staging/prod: <200ms (target SLO)
    http_req_duration: [IS_DOCKER ? 'p(95)<500' : 'p(95)<200'],
    // Tagged thresholds — verify each endpoint class independently
    'http_req_duration{endpoint:health}':     [IS_DOCKER ? 'p(95)<200' : 'p(95)<100'],
    'http_req_duration{endpoint:cases-list}': [IS_DOCKER ? 'p(95)<500' : 'p(95)<200'],
  },
};

// ─── Custom metrics ───────────────────────────────────────────────────────────

const healthCheckErrors = new Rate('health_check_errors');
const casesEndpointErrors = new Rate('cases_endpoint_errors');

// ─── Test scenarios ───────────────────────────────────────────────────────────

export default function () {
  // Mark 401 as expected (unauthenticated smoke test) — not counted in http_req_failed
  http.setResponseCallback(http.expectedStatuses(200, 201, 204, 401));

  // --- Health endpoint (ready probe) ---
  const healthRes = http.get(`${BASE_URL}/health/ready`, {
    tags: { endpoint: 'health' },
  });

  const healthOk = check(healthRes, {
    'GET /health/ready status 200': (r) => r.status === 200,
    'GET /health/ready response time < 100ms': (r) => r.timings.duration < 100,
    'GET /health/ready body contains "Healthy"': (r) => r.body && r.body.includes('Healthy'),
  });

  healthCheckErrors.add(!healthOk);

  sleep(0.5);

  // --- Cases list endpoint ---
  const casesRes = http.get(`${BASE_URL}/api/v1/cases`, {
    headers: { Accept: 'application/json' },
    tags: { endpoint: 'cases-list' },
  });

  const casesOk = check(casesRes, {
    'GET /api/v1/cases status 200 or 401': (r) => r.status === 200 || r.status === 401,
    'GET /api/v1/cases response time < 200ms': (r) => r.timings.duration < 200,
  });

  casesEndpointErrors.add(!casesOk);

  sleep(0.5);
}

// ─── Setup / teardown ─────────────────────────────────────────────────────────

export function handleSummary(data) {
  return {
    'stdout': JSON.stringify(data, null, 2),
  };
}
