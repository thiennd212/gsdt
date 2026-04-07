// GSDT — K6 connection pool test
// Purpose: TC-PERF-DB-002/003 — connection pool exhaustion and concurrent write SLOs
// Run: k6 run connection-pool-test.js --env BASE_URL=https://staging.internal --env API_KEY=<key>

import http from 'k6/http';
import { check, group, sleep } from 'k6';
import { Counter, Rate, Trend } from 'k6/metrics';
import { getJsonApiKeyHeaders } from './lib/auth.js';
import { randomRef, pageParams } from './lib/helpers.js';

// ─── Config ──────────────────────────────────────────────────────────────────

const BASE_URL = __ENV.BASE_URL || 'https://staging.internal';
const IS_DOCKER = (__ENV.ENVIRONMENT || '').toLowerCase() === 'docker';

// TC-PERF-DB-002: 200 VUs burst — connection pool must not exhaust (no 503/pool errors)
// TC-PERF-DB-003: 50 VUs concurrent writes — cases created simultaneously
export const options = {
  stages: [
    { duration: '30s', target: 200 },  // ramp up fast to trigger pool pressure
    { duration: '1m',  target: 200 },  // hold burst
    { duration: '30s', target: 0   },  // ramp down
  ],
  thresholds: {
    'http_req_duration{operation:read}':  [IS_DOCKER ? 'p(95)<3000' : 'p(95)<1000'],
    'http_req_duration{operation:write}': [IS_DOCKER ? 'p(95)<4000' : 'p(95)<2000'],
    'http_req_failed': ['rate<0.01'],
    // TC-PERF-DB-002: pool exhaustion manifests as 503 — must stay at zero
    'pool_exhaustion_errors': ['count<1'],
  },
};

// ─── Custom metrics ───────────────────────────────────────────────────────────

const poolExhaustionErrors  = new Counter('pool_exhaustion_errors');
const concurrentWriteErrors = new Rate('concurrent_write_errors');
const casesCreated          = new Counter('cases_created_total');
const writeDuration         = new Trend('concurrent_write_duration');

// ─── Main scenario ────────────────────────────────────────────────────────────

export default function () {
  const headers = getJsonApiKeyHeaders();

  // TC-PERF-DB-002: Read probe under 200 VU burst — detects pool exhaustion via 503
  group('Pool pressure read', () => {
    const res = http.get(`${BASE_URL}/api/v1/cases?${pageParams(1, 10)}`, {
      headers,
      tags: { operation: 'read' },
    });

    // 503 = server/pool overloaded; 429 = rate-limited (acceptable)
    if (res.status === 503) {
      poolExhaustionErrors.add(1);
    }

    check(res, {
      'GET cases not 503 (pool not exhausted)': (r) => r.status !== 503,
      'GET cases status 200 or 429':            (r) => r.status === 200 || r.status === 429,
    });
  });

  sleep(0.1);

  // TC-PERF-DB-003: Concurrent writes — all 200 VUs attempt case creation simultaneously
  // First 50 VUs perform writes; remaining hold read load (realistic mixed workload)
  if (__VU <= 50) {
    group('Concurrent case write', () => {
      const payload = JSON.stringify({
        reference:   randomRef('POOL'),
        title:       'Connection pool stress test case',
        description: 'Created by k6 connection-pool-test — concurrent write pressure',
        status:      'Draft',
      });

      const res = http.post(`${BASE_URL}/api/v1/cases`, payload, {
        headers,
        tags: { operation: 'write' },
      });

      if (res.status === 503) {
        poolExhaustionErrors.add(1);
      }

      const ok = check(res, {
        'POST /api/v1/cases status 201': (r) => r.status === 201,
        'POST /api/v1/cases has id': (r) => {
          try { return JSON.parse(r.body).data?.id !== undefined; } catch { return false; }
        },
      });

      concurrentWriteErrors.add(!ok);
      writeDuration.add(res.timings.duration);

      if (ok) casesCreated.add(1);
    });
  }

  sleep(0.2);
}

// ─── Summary ─────────────────────────────────────────────────────────────────

export function handleSummary(data) {
  const poolErrors  = data.metrics['pool_exhaustion_errors']?.values?.count ?? 0;
  const writeErrRate = data.metrics['concurrent_write_errors']?.values?.rate ?? 0;
  const totalCases  = data.metrics['cases_created_total']?.values?.count ?? 0;
  const writeP95    = data.metrics['concurrent_write_duration']?.values?.['p(95)'] ?? 0;

  console.log('\n=== CONNECTION POOL TEST SUMMARY ===');
  console.log(`Pool exhaustion (503) count : ${poolErrors}`);
  console.log(`Concurrent write error rate : ${(writeErrRate * 100).toFixed(2)}%`);
  console.log(`Cases created successfully  : ${totalCases}`);
  console.log(`Write p95 (ms)              : ${Math.round(writeP95)}`);
  console.log(`Pool health                 : ${poolErrors === 0 ? 'OK' : 'EXHAUSTED — INVESTIGATE'}`);

  return { 'stdout': JSON.stringify(data.metrics, null, 2) };
}
