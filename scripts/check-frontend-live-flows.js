#!/usr/bin/env node
'use strict';

const DEFAULT_BASE_URL = 'http://localhost:8080';
const DEFAULT_TIMEOUT_MS = 10000;

function parseArgs(argv) {
  const options = {
    baseUrl: DEFAULT_BASE_URL,
    timeoutMs: DEFAULT_TIMEOUT_MS,
    token: null,
    help: false
  };

  for (let index = 0; index < argv.length; index += 1) {
    const arg = argv[index];

    if (arg === '--base-url') {
      const value = argv[index + 1];
      if (!value) {
        throw new Error('Missing value for --base-url.');
      }

      options.baseUrl = value;
      index += 1;
      continue;
    }

    if (arg === '--timeout-ms') {
      const value = argv[index + 1];
      if (!value) {
        throw new Error('Missing value for --timeout-ms.');
      }

      const parsedTimeout = Number.parseInt(value, 10);
      if (!Number.isFinite(parsedTimeout) || parsedTimeout <= 0) {
        throw new Error(`Invalid --timeout-ms value '${value}'. Use a positive integer.`);
      }

      options.timeoutMs = parsedTimeout;
      index += 1;
      continue;
    }

    if (arg === '--token') {
      const value = argv[index + 1];
      if (!value) {
        throw new Error('Missing value for --token.');
      }

      options.token = value;
      index += 1;
      continue;
    }

    if (arg === '--help' || arg === '-h') {
      options.help = true;
      continue;
    }

    throw new Error(`Unknown argument: ${arg}`);
  }

  return options;
}

function normalizeBaseUrl(value) {
  return value.endsWith('/') ? value.slice(0, -1) : value;
}

function ensureObject(value, message) {
  if (!value || typeof value !== 'object') {
    throw new Error(message);
  }
}

function createRequestHeaders(token) {
  const headers = {
    Accept: 'application/json',
    'Content-Type': 'application/json'
  };

  if (token) {
    headers.Authorization = `Bearer ${token}`;
  }

  return headers;
}

async function requestJson({ baseUrl, timeoutMs, token, method, path, body }) {
  const controller = new AbortController();
  const timeout = setTimeout(() => controller.abort(), timeoutMs);

  try {
    const response = await fetch(`${baseUrl}${path}`, {
      method,
      headers: createRequestHeaders(token),
      body: body ? JSON.stringify(body) : undefined,
      signal: controller.signal
    });

    const contentType = response.headers.get('content-type') ?? '';
    const isJson = contentType.toLowerCase().includes('application/json');
    const parsedBody = isJson ? await response.json() : null;

    if (!response.ok) {
      const details = parsedBody && typeof parsedBody === 'object' ? JSON.stringify(parsedBody) : 'No JSON body';
      throw new Error(`${method} ${path} returned HTTP ${response.status}. Details: ${details}`);
    }

    if (!isJson) {
      throw new Error(`${method} ${path} returned unsupported content type '${contentType || 'unknown'}'.`);
    }

    return parsedBody;
  } catch (error) {
    if (error.name === 'AbortError') {
      throw new Error(`${method} ${path} timed out after ${timeoutMs}ms.`);
    }

    throw error;
  } finally {
    clearTimeout(timeout);
  }
}

async function runLiveFlowChecks(options) {
  console.log('1) Verifying gateway liveness via /health/live ...');
  const liveHealth = await requestJson({
    ...options,
    method: 'GET',
    path: '/health/live'
  });

  ensureObject(liveHealth, 'Expected /health/live to return a JSON object payload.');

  console.log('2) Verifying gateway readiness via /health/ready ...');
  const readyHealth = await requestJson({
    ...options,
    method: 'GET',
    path: '/health/ready'
  });

  ensureObject(readyHealth, 'Expected /health/ready to return a JSON object payload.');

  if (!options.token) {
    console.log('3) Skipping authenticated runtime checks because no JWT token was provided.');
    console.log('   Pass --token <jwt> to validate active campaign/tracking/gateway policy contracts.');
    return;
  }

  console.log('3) Reading gateway route policies via /api/gateway/route-policies ...');
  const routePolicies = await requestJson({
    ...options,
    method: 'GET',
    path: '/api/gateway/route-policies'
  });

  ensureObject(routePolicies, 'Expected gateway route policies response to be a JSON object.');

  console.log('4) Reading campaign listing via /api/campaigns ...');
  const campaigns = await requestJson({
    ...options,
    method: 'GET',
    path: '/api/campaigns'
  });

  if (!Array.isArray(campaigns)) {
    throw new Error('Expected /api/campaigns to return a JSON array.');
  }

  console.log('5) Reading tracking page listing via /api/tracking/pages ...');
  const trackingPages = await requestJson({
    ...options,
    method: 'GET',
    path: '/api/tracking/pages'
  });

  if (!Array.isArray(trackingPages)) {
    throw new Error('Expected /api/tracking/pages to return a JSON array.');
  }

  console.log('6) Reading analytics overview via /api/tracking/analytics/overview ...');
  const analyticsOverview = await requestJson({
    ...options,
    method: 'GET',
    path: '/api/tracking/analytics/overview'
  });

  ensureObject(analyticsOverview, 'Expected /api/tracking/analytics/overview to return a JSON object.');
}

async function main() {
  const parsed = parseArgs(process.argv.slice(2));

  if (parsed.help) {
    console.log('Usage: node scripts/check-frontend-live-flows.js [--base-url <url>] [--token <jwt>] [--timeout-ms <integer>]');
    console.log('Checks active health/live-readiness endpoints and, optionally, authenticated gateway/campaign/tracking baseline flows.');
    return;
  }

  const options = {
    ...parsed,
    baseUrl: normalizeBaseUrl(parsed.baseUrl)
  };

  await runLiveFlowChecks(options);

  console.log(`Frontend live-flow verification passed against ${options.baseUrl}.`);
}

main().catch(error => {
  console.error(`Error: ${error.message}`);
  process.exit(1);
});
