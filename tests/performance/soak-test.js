// GSDT — K6 soak test
// Purpose: detect memory leaks and resource exhaustion under sustained load (60 min)
// Run: k6 run soak-test.js --env BASE_URL=https://staging.internal --env API_KEY=<key>

import http from 'k6/http';
import { check, group, sleep } from 'k6';
import { Trend, Rate, Counter } from 'k6/metrics';
import { getApiKeyHeaders } from './lib/auth.js';

// ─── Config ──────────────────────────────────────────────────────────────────

const BASE_URL = __ENV.BASE_URL || 'https://staging.internal';

export const options = {
  stages: [
    // Gentle ramp-up to avoid false memory spike on startup
    { duration: '5m',  target: 300 },
    // Sustain 300 VUs for 60 minutes — long enough to expose memory leaks
    { duration: '60m', target: 300 },
    // Ramp down
    { duration: '5m',  target: 0   },
  ],
  thresholds: {
    // Error rate must stay below 1% throughout entire soak
    'http_req_failed': ['rate<0.01'],
    // p95 + p99 on single key (duplicate key silently drops one — combine both rules)
    'http_req_duration': ['p(95)<500', 'p(99)<1000'],
  },
};

// ─── Custom metrics ───────────────────────────────────────────────────────────

// Track response times per 10-minute window to detect gradual degradation
const earlyPhaseDuration  = new Trend('duration_early_phase');   // first 20 min
const middlePhaseDuration = new Trend('duration_middle_phase');  // 20–40 min
const latePhaseDuration   = new Trend('duration_late_phase');    // 40–60 min

// Memory leak proxy: rolling response time trend across the full soak duration.
// A steady upward drift in this metric indicates resource exhaustion or leak.
const responseTimeTrend = new Trend('soak_response_time_trend');

const requestErrors = new Rate('soak_request_errors');
const totalRequests = new Counter('soak_total_requests');

// Bucket elapsed seconds into phase label for memory-leak detection
function phaseMetric(elapsed) {
  if (elapsed < 1200) return earlyPhaseDuration;   // 0–20 min
  if (elapsed < 2400) return middlePhaseDuration;  // 20–40 min
  return latePhaseDuration;                         // 40–60 min
}

// ─── Main scenario ────────────────────────────────────────────────────────────

export function setup() {
  return { startTime: Date.now() };
}

export default function (data) {
  const elapsed = (Date.now() - data.startTime) / 1000;
  const headers = getApiKeyHeaders();

  // Health check — fastest probe to detect cascading failures early
  group('Health probe', () => {
    const res = http.get(`${BASE_URL}/health`, {
      headers,
      tags: { operation: 'health' },
    });

    const ok = check(res, {
      'health 200': (r) => r.status === 200,
    });

    requestErrors.add(!ok);
    phaseMetric(elapsed).add(res.timings.duration);
    responseTimeTrend.add(res.timings.duration);
    totalRequests.add(1);
  });

  sleep(0.3);

  // Cases list — representative read workload
  group('Cases list', () => {
    const res = http.get(`${BASE_URL}/api/v1/cases?page=1&pageSize=10`, {
      headers,
      tags: { operation: 'read' },
    });

    const ok = check(res, {
      'cases list 200': (r) => r.status === 200,  // 401 would indicate auth failure
    });

    requestErrors.add(!ok);
    phaseMetric(elapsed).add(res.timings.duration);
    responseTimeTrend.add(res.timings.duration);
    totalRequests.add(1);
  });

  sleep(0.7);
}

// ─── Memory-leak detection summary ───────────────────────────────────────────

export function handleSummary(data) {
  const early  = data.metrics['duration_early_phase']?.values?.['p(95)']  ?? 0;
  const middle = data.metrics['duration_middle_phase']?.values?.['p(95)'] ?? 0;
  const late   = data.metrics['duration_late_phase']?.values?.['p(95)']   ?? 0;

  // Flag if late-phase p95 is more than 50% higher than early-phase (leak indicator)
  const degradationRatio = early > 0 ? (late / early) : 1;
  const leakSuspected    = degradationRatio > 1.5;

  // TC: p95 must not drift >20% from early phase to late phase
  const driftPct        = early > 0 ? ((late - early) / early) * 100 : 0;
  const driftExceeded   = driftPct > 20;

  // Rolling trend p95 — overall view across full soak duration
  const trendP95 = data.metrics['soak_response_time_trend']?.values?.['p(95)'] ?? 0;

  const summary = {
    memoryLeakAnalysis: {
      earlyPhaseP95Ms:  Math.round(early),
      middlePhaseP95Ms: Math.round(middle),
      latePhaseP95Ms:   Math.round(late),
      degradationRatio: degradationRatio.toFixed(2),
      leakSuspected,
      // Response time drift check (>20% increase early→late = SLO violation risk)
      driftPct:         driftPct.toFixed(1),
      driftExceeded,
      rollingTrendP95Ms: Math.round(trendP95),
    },
    rawMetrics: data.metrics,
  };

  console.log('\n=== SOAK TEST MEMORY LEAK ANALYSIS ===');
  console.log(`Early  phase p95: ${summary.memoryLeakAnalysis.earlyPhaseP95Ms} ms`);
  console.log(`Middle phase p95: ${summary.memoryLeakAnalysis.middlePhaseP95Ms} ms`);
  console.log(`Late   phase p95: ${summary.memoryLeakAnalysis.latePhaseP95Ms} ms`);
  console.log(`Degradation ratio: ${summary.memoryLeakAnalysis.degradationRatio}x`);
  console.log(`Response time drift: ${summary.memoryLeakAnalysis.driftPct}% ${driftExceeded ? '— EXCEEDS 20% THRESHOLD' : '(within limit)'}`);
  console.log(`Rolling trend p95: ${summary.memoryLeakAnalysis.rollingTrendP95Ms} ms`);
  console.log(`Memory leak suspected: ${leakSuspected ? 'YES - INVESTIGATE' : 'No'}`);

  return {
    'stdout': JSON.stringify(summary, null, 2),
    'tests/performance/soak-result.json': JSON.stringify(summary, null, 2),
  };
}
