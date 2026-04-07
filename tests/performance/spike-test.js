// GSDT — K6 spike test
// Purpose: validate circuit breaker and graceful degradation under sudden traffic spikes
// Run: k6 run spike-test.js --env BASE_URL=https://staging.internal --env API_KEY=<key>

import http from 'k6/http';
import { check, group, sleep } from 'k6';
import { Rate, Trend, Counter } from 'k6/metrics';
import { getApiKeyHeaders } from './lib/auth.js';

// ─── Config ──────────────────────────────────────────────────────────────────

const BASE_URL = __ENV.BASE_URL || 'https://staging.internal';

export const options = {
  stages: [
    // Baseline — confirm system is healthy before spike
    { duration: '1m',  target: 10   },
    // SPIKE: 0 → 1000 VUs in 30 seconds (circuit breaker trigger zone)
    { duration: '30s', target: 1000 },
    // Hold spike for 1 minute
    { duration: '1m',  target: 1000 },
    // Ramp down — validate recovery
    { duration: '30s', target: 10   },
    // Post-spike baseline — confirm system recovered
    { duration: '2m',  target: 10   },
  ],
  thresholds: {
    // During spike, some failures are expected; circuit breaker should open
    // Allow up to 30% error rate — the important metric is recovery
    'http_req_failed{phase:spike}':    ['rate<0.30'],
    // Post-spike: system must recover to normal error rate
    'http_req_failed{phase:recovery}': ['rate<0.01'],
    // Circuit-breaker responses (503) should appear during spike
    'circuit_breaker_activations': ['count>0'],
  },
};

// ─── Custom metrics ───────────────────────────────────────────────────────────

const circuitBreakerActivations = new Counter('circuit_breaker_activations');
const spikeErrors               = new Rate('spike_errors');
const recoveryErrors            = new Rate('recovery_errors');
const spikeDuration             = new Trend('spike_response_duration');
const recoveryDuration          = new Trend('recovery_response_duration');
// Recovery time: time from spike-end until p95 returns to baseline (<200ms staging / <500ms Docker)
// Tracked as a Trend of response durations during the recovery window only
const recoveryTimeTrend         = new Trend('recovery_time_to_baseline');

// Determine phase based on elapsed time (seconds)
// Baseline: 0–60s | Spike: 60–150s | Recovery: 150–300s
function getPhase(elapsed) {
  if (elapsed < 60)  return 'baseline';
  if (elapsed < 150) return 'spike';
  return 'recovery';
}

// ─── Setup ────────────────────────────────────────────────────────────────────

export function setup() {
  return { startTime: Date.now() };
}

// ─── Main scenario ────────────────────────────────────────────────────────────

export default function (data) {
  const elapsed = (Date.now() - data.startTime) / 1000;
  const phase   = getPhase(elapsed);
  const headers = getApiKeyHeaders();

  // ── Health probe (always run — monitors circuit breaker state) ──────────────
  group('Health probe', () => {
    const res = http.get(`${BASE_URL}/health`, {
      headers,
      tags: { operation: 'health', phase },
    });

    // 503 = circuit breaker open (expected during spike, not after)
    if (res.status === 503) {
      circuitBreakerActivations.add(1);
    }

    const ok = check(res, {
      'health 200 or 503 (cb open)': (r) => r.status === 200 || r.status === 503,
    });

    if (phase === 'spike')    spikeErrors.add(!ok);
    if (phase === 'recovery') recoveryErrors.add(!ok);

    if (phase === 'spike')    spikeDuration.add(res.timings.duration);
    if (phase === 'recovery') {
      recoveryDuration.add(res.timings.duration);
      // Track every response during recovery to measure how quickly p95 returns to baseline
      // Baseline target: <200ms staging, <500ms Docker — recorded for post-run analysis
      recoveryTimeTrend.add(res.timings.duration);
    }
  });

  sleep(0.05); // tight loop during spike to maximise pressure

  // ── Cases endpoint under spike pressure ──────────────────────────────────────
  group('Cases list under spike', () => {
    const res = http.get(`${BASE_URL}/api/v1/cases?page=1&pageSize=5`, {
      headers,
      tags: { operation: 'read', phase },
    });

    check(res, {
      'cases 200, 429 (rate-limited), or 503 (cb open)': (r) =>
        r.status === 200 || r.status === 429 || r.status === 503,
    });

    if (res.status === 503) circuitBreakerActivations.add(1);
  });

  sleep(0.05);
}

// ─── Summary ─────────────────────────────────────────────────────────────────

export function handleSummary(data) {
  const cbCount      = data.metrics['circuit_breaker_activations']?.values?.count ?? 0;
  const spikeErrRate = data.metrics['spike_errors']?.values?.rate ?? 0;
  const recovErrRate = data.metrics['recovery_errors']?.values?.rate ?? 0;

  // Recovery time analysis: p50/p95 of response durations measured after spike drops
  // Lower values = faster return to baseline; target p95 < 200ms (staging) / 500ms (Docker)
  const recovP50 = data.metrics['recovery_time_to_baseline']?.values?.['p(50)'] ?? 0;
  const recovP95 = data.metrics['recovery_time_to_baseline']?.values?.['p(95)'] ?? 0;
  const recovMax = data.metrics['recovery_time_to_baseline']?.values?.max ?? 0;

  const summary = {
    circuitBreakerActivations: cbCount,
    spikePhaseErrorRate:       (spikeErrRate * 100).toFixed(2) + '%',
    recoveryPhaseErrorRate:    (recovErrRate * 100).toFixed(2) + '%',
    circuitBreakerWorking:     cbCount > 0,
    systemRecovered:           recovErrRate < 0.01,
    // Recovery time to baseline (response duration trend during recovery window)
    recoveryTime: {
      p50Ms: Math.round(recovP50),
      p95Ms: Math.round(recovP95),
      maxMs: Math.round(recovMax),
      // System considered back at baseline when p95 < 200ms (staging target)
      atBaselineP95: recovP95 < 200,
    },
  };

  console.log('\n=== SPIKE TEST — CIRCUIT BREAKER ANALYSIS ===');
  console.log(`Circuit breaker activations : ${cbCount}`);
  console.log(`Spike phase error rate      : ${summary.spikePhaseErrorRate}`);
  console.log(`Recovery phase error rate   : ${summary.recoveryPhaseErrorRate}`);
  console.log(`Circuit breaker working     : ${summary.circuitBreakerWorking ? 'YES' : 'NOT DETECTED'}`);
  console.log(`System recovered            : ${summary.systemRecovered ? 'YES' : 'NO - INVESTIGATE'}`);
  console.log(`Recovery p50 (ms)           : ${summary.recoveryTime.p50Ms}`);
  console.log(`Recovery p95 (ms)           : ${summary.recoveryTime.p95Ms}`);
  console.log(`Recovery max (ms)           : ${summary.recoveryTime.maxMs}`);
  console.log(`At baseline (<200ms p95)    : ${summary.recoveryTime.atBaselineP95 ? 'YES' : 'NO — still elevated'}`);

  return {
    'stdout': JSON.stringify(summary, null, 2),
    'tests/performance/spike-result.json': JSON.stringify(summary, null, 2),
  };
}
