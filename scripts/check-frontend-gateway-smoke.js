#!/usr/bin/env node
'use strict';

const DEFAULT_BASE_URL = 'http://localhost:8080';
const DEFAULT_TIMEOUT_MS = 5000;

const endpoints = [
  {
    name: 'setup status',
    method: 'GET',
    path: '/api/setup/status'
  },
  {
    name: 'setup guard decision',
    method: 'GET',
    path: '/api/setup/guard-decision'
  },
  {
    name: 'runtime configuration status',
    method: 'GET',
    path: '/api/configuration'
  },
  {
    name: 'gateway route policies',
    method: 'GET',
    path: '/api/gateway/route-policies'
  }
];

function parseArgs(argv) {
  const options = {
    baseUrl: DEFAULT_BASE_URL,
    timeoutMs: DEFAULT_TIMEOUT_MS,
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

async function verifyEndpoint(baseUrl, timeoutMs, endpoint) {
  const controller = new AbortController();
  const timeout = setTimeout(() => controller.abort(), timeoutMs);

  try {
    const response = await fetch(`${baseUrl}${endpoint.path}`, {
      method: endpoint.method,
      headers: {
        Accept: 'application/json'
      },
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

async function main() {
  const options = parseArgs(process.argv.slice(2));

  if (options.help) {
    console.log('Usage: node scripts/check-frontend-gateway-smoke.js [--base-url <url>] [--timeout-ms <integer>]');
    return;
  }

  const baseUrl = normalizeBaseUrl(options.baseUrl);
  const failures = [];

  for (const endpoint of endpoints) {
    try {
      await verifyEndpoint(baseUrl, options.timeoutMs, endpoint);
      console.log(`✓ ${endpoint.name}: ${endpoint.method} ${endpoint.path}`);
    } catch (error) {
      failures.push(`- ${endpoint.name}: ${error.message}`);
    }
  }

  if (failures.length > 0) {
    throw new Error(
      `Frontend gateway smoke validation failed against ${baseUrl}.\n` +
      failures.join('\n') +
      '\nMake sure API and Gateway are running before smoke validation.'
    );
  }

  console.log(`Frontend gateway smoke validation passed for ${endpoints.length} endpoint(s) via ${baseUrl}.`);
}

main().catch(error => {
  console.error(`Error: ${error.message}`);
  process.exit(1);
});
