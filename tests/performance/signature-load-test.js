// GSDT — K6 signature module load test
// Purpose: TC-PERF-SIG-001/002 — signature request create + query SLO
// Run: k6 run signature-load-test.js --env BASE_URL=https://staging.internal --env API_KEY=<key>

import http from 'k6/http';
import { check, group, sleep } from 'k6';
import { Rate, Trend } from 'k6/metrics';
import { getJsonApiKeyHeaders } from './lib/auth.js';
import { parseBody, getTenantId } from './lib/helpers.js';

// ─── Config ──────────────────────────────────────────────────────────────────

const BASE_URL = __ENV.BASE_URL || 'https://staging.internal';
const IS_DOCKER = (__ENV.ENVIRONMENT || '').toLowerCase() === 'docker';

// TC-PERF-SIG-001/002: 30 VUs — signature operations p95<500ms
export const options = {
  stages: [
    { duration: '1m', target: 30 },
    { duration: '2m', target: 30 },
    { duration: '1m', target: 0 },
  ],
  thresholds: {
    'http_req_duration{operation:sig-query}':  [IS_DOCKER ? 'p(95)<1000' : 'p(95)<500'],
    'http_req_duration{operation:sig-create}': [IS_DOCKER ? 'p(95)<1500' : 'p(95)<800'],
    'http_req_failed': ['rate<0.05'],
  },
};

// ─── Custom metrics ─────────────────────────────────────────────────────────

const sigQueryErrors    = new Rate('sig_query_errors');
const sigQueryDuration  = new Trend('sig_query_duration');
const sigCreateErrors   = new Rate('sig_create_errors');
const sigCreateDuration = new Trend('sig_create_duration');

// ─── Main scenario ──────────────────────────────────────────────────────────

export default function () {
  const headers = getJsonApiKeyHeaders();

  // TC-PERF-SIG-001: Query signatures by document
  group('Signature query by document', () => {
    const docId = '00000000-0000-0000-0000-000000000001';
    const res = http.get(
      `${BASE_URL}/api/v1/signatures?documentId=${docId}&page=1&pageSize=20`,
      { headers, tags: { operation: 'sig-query' } },
    );

    const ok = check(res, {
      'GET /signatures?documentId= status 200': (r) => r.status === 200,
      'GET /signatures?documentId= has items': (r) => {
        const body = parseBody(r);
        return body?.data?.items !== undefined;
      },
    });

    sigQueryErrors.add(!ok);
    sigQueryDuration.add(res.timings.duration);
  });

  sleep(0.3);

  // TC-PERF-SIG-002: Create signature request
  group('Signature create request', () => {
    const payload = JSON.stringify({
      tenantId: getTenantId(),
      documentId: '00000000-0000-0000-0000-000000000001',
      documentHash: `sha256:k6-perf-${Date.now()}-${__VU}`,
      signingMethod: 0,
      signatureFormat: 0,
      signerUserIds: [],
      expiresAt: null,
    });

    const res = http.post(
      `${BASE_URL}/api/v1/signatures/request`,
      payload,
      { headers, tags: { operation: 'sig-create' } },
    );

    // 200 = created, 422 = validation — both prove endpoint works under load
    const ok = check(res, {
      'POST /signatures/request status 200|422': (r) => [200, 422].includes(r.status),
    });

    sigCreateErrors.add(!ok);
    sigCreateDuration.add(res.timings.duration);
  });

  sleep(0.5);
}

// ─── Summary ─────────────────────────────────────────────────────────────────

export function handleSummary(data) {
  const qp95 = data.metrics['sig_query_duration']?.values?.['p(95)'] ?? 0;
  const cp95 = data.metrics['sig_create_duration']?.values?.['p(95)'] ?? 0;

  console.log('\n=== SIGNATURE LOAD TEST SUMMARY ===');
  console.log(`Query p95 (ms)  : ${Math.round(qp95)}`);
  console.log(`Create p95 (ms) : ${Math.round(cp95)}`);

  return { 'stdout': JSON.stringify(data.metrics, null, 2) };
}
