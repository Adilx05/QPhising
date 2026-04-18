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

function buildPayloadSuffix() {
  const timestamp = new Date().toISOString().replace(/[^0-9]/g, '').slice(0, 14);
  return `live-${timestamp}`;
}

function ensureTruthy(value, message) {
  if (!value) {
    throw new Error(message);
  }
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
  const payloadSuffix = buildPayloadSuffix();

  const setupPayload = {
    databaseConnectionString: `Server=localhost;Database=qphising_setup_${payloadSuffix};User Id=sa;Password=P@ssw0rd!;TrustServerCertificate=true`,
    redisConnectionString: `localhost:6379,password=redis-${payloadSuffix}`,
    keycloakAuthority: `https://keycloak.${payloadSuffix}.local`,
    keycloakRealm: `qphising-${payloadSuffix}`,
    keycloakClientId: `setup-client-${payloadSuffix}`,
    keycloakClientSecret: `setup-secret-${payloadSuffix}`
  };

  const runtimePayload = {
    databaseConnectionString: `Server=localhost;Database=qphising_runtime_${payloadSuffix};User Id=sa;Password=P@ssw0rd!;TrustServerCertificate=true`,
    redisConnectionString: `localhost:6379,password=runtime-${payloadSuffix}`,
    keycloakAuthority: `https://runtime.${payloadSuffix}.local`,
    keycloakRealm: `qphising-runtime-${payloadSuffix}`,
    keycloakClientId: `runtime-client-${payloadSuffix}`,
    keycloakClientSecret: `runtime-secret-${payloadSuffix}`
  };

  console.log('1) Persisting setup configuration via /api/setup/save ...');
  await requestJson({
    ...options,
    method: 'POST',
    path: '/api/setup/save',
    body: setupPayload
  });

  console.log('2) Verifying setup status reflects persisted state ...');
  const setupStatus = await requestJson({
    ...options,
    method: 'GET',
    path: '/api/setup/status'
  });

  ensureTruthy(setupStatus.isDatabaseConfigured, 'Expected setup status to report database as configured.');
  ensureTruthy(setupStatus.isRedisConfigured, 'Expected setup status to report redis as configured.');
  ensureTruthy(setupStatus.isKeycloakConfigured, 'Expected setup status to report keycloak as configured.');

  console.log('3) Persisting runtime configuration via /api/configuration ...');
  await requestJson({
    ...options,
    method: 'POST',
    path: '/api/configuration',
    body: runtimePayload
  });

  console.log('4) Verifying runtime configuration readiness via /api/configuration ...');
  const runtimeStatus = await requestJson({
    ...options,
    method: 'GET',
    path: '/api/configuration'
  });

  ensureTruthy(runtimeStatus.isDatabaseConfigured, 'Expected runtime status to report database as configured.');
  ensureTruthy(runtimeStatus.isRedisConfigured, 'Expected runtime status to report redis as configured.');
  ensureTruthy(runtimeStatus.isKeycloakConfigured, 'Expected runtime status to report keycloak as configured.');
  ensureTruthy(runtimeStatus.updatedAtUtc, 'Expected runtime status to contain updatedAtUtc timestamp.');

  console.log('5) Patching runtime configuration via /api/configuration ...');
  await requestJson({
    ...options,
    method: 'PATCH',
    path: '/api/configuration',
    body: {
      keycloakClientSecret: `${runtimePayload.keycloakClientSecret}-patched`
    }
  });

  console.log('6) Re-checking runtime status after patch ...');
  const runtimeStatusAfterPatch = await requestJson({
    ...options,
    method: 'GET',
    path: '/api/configuration'
  });

  ensureTruthy(runtimeStatusAfterPatch.isReadyForProtectedRuntime, 'Expected runtime status to remain ready after patch update.');
}

async function main() {
  const parsed = parseArgs(process.argv.slice(2));

  if (parsed.help) {
    console.log('Usage: node scripts/check-frontend-live-flows.js [--base-url <url>] [--token <jwt>] [--timeout-ms <integer>]');
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
