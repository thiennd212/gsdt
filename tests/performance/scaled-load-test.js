// GSDT — Scaled load test (500 / 1000 / 2000 CCU tiers)
// Purpose: validate SLOs across different deployment sizing tiers
//
// Run by tier:
//   k6 run scaled-load-test.js --env BASE_URL=https://staging.internal --env API_KEY=<key> --env TIER=500
//   k6 run scaled-load-test.js --env BASE_URL=https://staging.internal --env API_KEY=<key> --env TIER=1000
//   k6 run scaled-load-test.js --env BASE_URL=https://staging.internal --env API_KEY=<key> --env TIER=2000

import http from 'k6/http';
import { check, group, sleep } from 'k6';
import { Counter, Rate } from 'k6/metrics';
import { getJsonApiKeyHeaders, getApiKeyHeaders } from './lib/auth.js';
import { randomRef, pageParams, parseBody } from './lib/helpers.js';

// ─── Tier Config ─────────────────────────────────────────────────────────────

const BASE_URL  = __ENV.BASE_URL  || 'https://staging.internal';
const TIER      = parseInt(__ENV.TIER || '500', 10);
const TENANT_ID = __ENV.TENANT_ID || '00000000-0000-0000-0000-000000000001';

// SLO thresholds tighten at lower tiers, relax slightly at higher tiers
// (higher CCU → more DB contention → allow slight p95 increase)
const TIERS = {
  500:  { vus: 500,  read: 200,  write: 400,  errorRate: 0.01 },
  1000: { vus: 1000, read: 300,  write: 600,  errorRate: 0.01 },
  2000: { vus: 2000, read: 500,  write: 1000, errorRate: 0.02 },
};

const cfg = TIERS[TIER] || TIERS[500];

export const options = {
  stages: [
    // Ramp up gradually — avoid thundering herd on startup
    { duration: '5m',  target: Math.round(cfg.vus * 0.25) },
    { duration: '5m',  target: Math.round(cfg.vus * 0.75) },
    { duration: '5m',  target: cfg.vus },
    // Sustain peak load
    { duration: '20m', target: cfg.vus },
    // Ramp down
    { duration: '5m',  target: 0 },
  ],
  thresholds: {
    [`http_req_duration{operation:read}`]:  [`p(95)<${cfg.read}`],
    [`http_req_duration{operation:write}`]: [`p(95)<${cfg.write}`],
    'http_req_failed': [`rate<${cfg.errorRate}`],
  },
};

// ─── Custom metrics ───────────────────────────────────────────────────────────

const createErrors = new Rate('create_errors');
const readErrors   = new Rate('read_errors');
const casesCreated = new Counter('cases_created_total');

// ─── Main scenario ────────────────────────────────────────────────────────────

export function setup() {
  return { tier: TIER, cfg };
}

export default function (data) {
  const jsonHeaders = getJsonApiKeyHeaders();
  const readHeaders = getApiKeyHeaders();
  let caseId = null;

  // ── CREATE ──────────────────────────────────────────────────────────────────
  group('Create case', () => {
    const res = http.post(`${BASE_URL}/api/v1/cases`,
      JSON.stringify({
        tenantId:    TENANT_ID,
        title:       `Scaled test case tier ${data.tier} ${randomRef('')}`,
        description: `Created by k6 scaled load test tier=${data.tier} at ${Date.now()}`,
        type:        2,   // CaseType.Request
        priority:    1,   // CasePriority.Medium
      }),
      { headers: jsonHeaders, tags: { operation: 'write' } }
    );

    const ok = check(res, {
      'POST /cases 200': (r) => r.status === 200,
    });

    createErrors.add(!ok);
    if (ok) {
      const body = parseBody(res);
      if (body?.data?.id) { caseId = body.data.id; casesCreated.add(1); }
    }
  });

  sleep(0.2);

  // ── LIST (paginated read) ────────────────────────────────────────────────────
  group('List cases', () => {
    const res = http.get(
      `${BASE_URL}/api/v1/cases?tenantId=${TENANT_ID}&${pageParams(1, 20)}`,
      { headers: readHeaders, tags: { operation: 'read' } }
    );

    const ok = check(res, {
      'GET /cases 200':      (r) => r.status === 200,
      'GET /cases has data': (r) => Array.isArray(parseBody(r)?.data),
    });

    readErrors.add(!ok);
  });

  sleep(0.2);

  // ── GET single ──────────────────────────────────────────────────────────────
  if (caseId) {
    group('Get case by id', () => {
      const res = http.get(
        `${BASE_URL}/api/v1/cases/${caseId}?tenantId=${TENANT_ID}`,
        { headers: readHeaders, tags: { operation: 'read' } }
      );
      readErrors.add(res.status !== 200);
    });

    sleep(0.1);
  }

  sleep(0.5);
}

// ─── Summary ─────────────────────────────────────────────────────────────────

export function handleSummary(data) {
  const p95Read  = data.metrics['http_req_duration{operation:read}']?.values?.['p(95)']  ?? 0;
  const p95Write = data.metrics['http_req_duration{operation:write}']?.values?.['p(95)'] ?? 0;
  const errRate  = data.metrics['http_req_failed']?.values?.rate ?? 0;
  const created  = data.metrics['cases_created_total']?.values?.count ?? 0;

  const summary = {
    tier:          TIER,
    sloRead_ms:    cfg.read,
    sloWrite_ms:   cfg.write,
    p95Read_ms:    Math.round(p95Read),
    p95Write_ms:   Math.round(p95Write),
    errorRate_pct: (errRate * 100).toFixed(2) + '%',
    casesCreated:  created,
    sloMet: p95Read <= cfg.read && p95Write <= cfg.write && errRate <= cfg.errorRate,
  };

  console.log(`\n=== SCALED LOAD TEST — TIER ${TIER} CCU ===`);
  console.log(`p95 read : ${summary.p95Read_ms}ms  (SLO: <${cfg.read}ms)  ${summary.p95Read_ms <= cfg.read ? '✓' : '✗ BREACHED'}`);
  console.log(`p95 write: ${summary.p95Write_ms}ms  (SLO: <${cfg.write}ms)  ${summary.p95Write_ms <= cfg.write ? '✓' : '✗ BREACHED'}`);
  console.log(`Error rate: ${summary.errorRate_pct}  (SLO: <${cfg.errorRate * 100}%)`);
  console.log(`SLOs met: ${summary.sloMet ? 'YES' : 'NO — scale up infrastructure'}`);

  return {
    'stdout': JSON.stringify(summary, null, 2),
    [`/tests/results/scaled-${TIER}-result.json`]: JSON.stringify(summary, null, 2),
  };
}
