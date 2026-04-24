#!/usr/bin/env node
'use strict';

const DEFAULT_BASE_URL = 'http://localhost:8080';
const DEFAULT_TIMEOUT_MS = 5000;

const publicEndpoints = [
  {
    name: 'gateway liveness',
    method: 'GET',
    path: '/health/live'
  },
  {
    name: 'gateway readiness',
    method: 'GET',
    path: '/health/ready'
  }
];

const protectedEndpoints = [
  {
    name: 'gateway route policies',
    method: 'GET',
    path: '/api/gateway/route-policies'
  },
  {
    name: 'campaign list',
    method: 'GET',
    path: '/api/campaigns'
  },
  {
    name: 'tracking page list',
    method: 'GET',
    path: '/api/tracking/pages'
  },
  {
    name: 'tracking analytics overview',
    method: 'GET',
    path: '/api/tracking/analytics/overview'
  }
];

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

async function verifyEndpoint(baseUrl, timeoutMs, endpoint, token) {
  const controller = new AbortController();
  const timeout = setTimeout(() => controller.abort(), timeoutMs);

  try {
    const headers = {
      Accept: 'application/json'
    };

    if (token) {
      headers.Authorization = `Bearer ${token}`;
    }

    const response = await fetch(`${baseUrl}${endpoint.path}`, {
      method: endpoint.method,
      headers,
      signal: controller.signal
    });

    if (!response.ok) {
      throw new Error(`${endpoint.method} ${endpoint.path} returned HTTP ${response.status}.`);
    }

    const contentType = response.headers.get('content-type') ?? '';
    if (!contentType.toLowerCase().includes('application/json')) {
      throw new Error(
        `${endpoint.method} ${endpoint.path} returned unsupported content type '${contentType || 'unknown'}'.`
      );
    }

    await response.json();
  } catch (error) {
    if (error.name === 'AbortError') {
      throw new Error(`${endpoint.method} ${endpoint.path} timed out after ${timeoutMs}ms.`);
    }

    throw error;
  } finally {
    clearTimeout(timeout);
  }
}

async function verifyEndpoints(baseUrl, timeoutMs, endpoints, token) {
  const failures = [];

  for (const endpoint of endpoints) {
    try {
      await verifyEndpoint(baseUrl, timeoutMs, endpoint, token);
      console.log(`✓ ${endpoint.name}: ${endpoint.method} ${endpoint.path}`);
    } catch (error) {
      failures.push(`- ${endpoint.name}: ${error.message}`);
    }
  }

  return failures;
}

async function main() {
  const options = parseArgs(process.argv.slice(2));

  if (options.help) {
    console.log('Usage: node scripts/check-frontend-gateway-smoke.js [--base-url <url>] [--token <jwt>] [--timeout-ms <integer>]');
    console.log('Verifies gateway live/ready endpoints and, when token is provided, authenticated campaign/tracking baseline routes.');
    return;
  }

  const baseUrl = normalizeBaseUrl(options.baseUrl);
  const publicFailures = await verifyEndpoints(baseUrl, options.timeoutMs, publicEndpoints, null);
  const protectedFailures = options.token
    ? await verifyEndpoints(baseUrl, options.timeoutMs, protectedEndpoints, options.token)
    : [];

  if (!options.token) {
    console.log('ℹ Protected endpoint checks were skipped. Pass --token <jwt> to validate campaign/tracking and route-policy contracts.');
  }

  const failures = [...publicFailures, ...protectedFailures];
  if (failures.length > 0) {
    throw new Error(
      `Frontend gateway smoke validation failed against ${baseUrl}.\n` +
      failures.join('\n') +
      '\nEnsure API and Gateway are running, and provide a valid JWT for protected checks.'
    );
  }

  const endpointCount = publicEndpoints.length + (options.token ? protectedEndpoints.length : 0);
  console.log(`Frontend gateway smoke validation passed for ${endpointCount} endpoint(s) via ${baseUrl}.`);
}

main().catch(error => {
  console.error(`Error: ${error.message}`);
  process.exit(1);
});
