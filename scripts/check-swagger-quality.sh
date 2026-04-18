#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "${SCRIPT_DIR}/.." && pwd)"
DEFAULT_SWAGGER_SOURCE="${REPO_ROOT}/frontend/openapi/proxy-validation.swagger.json"
SWAGGER_SOURCE="${1:-$DEFAULT_SWAGGER_SOURCE}"
SWAGGER_TEMP_FILE="$(mktemp)"

cleanup() {
  rm -f "${SWAGGER_TEMP_FILE}"
}
trap cleanup EXIT

if ! command -v node >/dev/null 2>&1; then
  echo "Error: node is required to validate Swagger quality gates." >&2
  echo "Install Node.js and rerun this script." >&2
  exit 1
fi

if [[ "${SWAGGER_SOURCE}" =~ ^https?:// ]]; then
  if ! command -v curl >/dev/null 2>&1; then
    echo "Error: curl is required to fetch Swagger from '${SWAGGER_SOURCE}'." >&2
    exit 1
  fi

  if ! curl --fail --silent --show-error --location "${SWAGGER_SOURCE}" --output "${SWAGGER_TEMP_FILE}"; then
    echo "Error: failed to fetch Swagger from '${SWAGGER_SOURCE}'." >&2
    exit 1
  fi
else
  if [[ "${SWAGGER_SOURCE}" =~ ^file:// ]]; then
    SWAGGER_SOURCE="${SWAGGER_SOURCE#file://}"
  fi

  if [ ! -f "${SWAGGER_SOURCE}" ]; then
    echo "Error: Swagger file not found at '${SWAGGER_SOURCE}'." >&2
    exit 1
  fi

  cp "${SWAGGER_SOURCE}" "${SWAGGER_TEMP_FILE}"
fi

if [ ! -s "${SWAGGER_TEMP_FILE}" ]; then
  echo "Error: Swagger document '${SWAGGER_SOURCE}' is empty." >&2
  exit 1
fi

echo "Running Swagger quality gate: ${SWAGGER_SOURCE}"

node - "${SWAGGER_TEMP_FILE}" <<'NODE'
const fs = require('fs');

const requiredPaths = [
  '/api/proxy-validation/assert-sync',
  '/api/configuration',
  '/api/setup/status',
  '/api/setup/guard-decision',
  '/api/setup/test-db',
  '/api/setup/test-redis',
  '/api/setup/test-keycloak',
  '/api/setup/save'
];
const persistenceContractChecks = [
  {
    method: 'post',
    path: '/api/setup/save',
    requestSchemaRef: '#/components/schemas/SaveSetupConfigurationRequest',
    responseSchemaRef: '#/components/schemas/SetupStatusResult'
  },
  {
    method: 'get',
    path: '/api/setup/status',
    responseSchemaRef: '#/components/schemas/SetupStatusResult'
  },
  {
    method: 'get',
    path: '/api/configuration',
    responseSchemaRef: '#/components/schemas/RuntimeConfigurationResult'
  },
  {
    method: 'post',
    path: '/api/configuration',
    requestSchemaRef: '#/components/schemas/SaveRuntimeConfigurationRequest',
    responseSchemaRef: '#/components/schemas/RuntimeConfigurationResult'
  },
  {
    method: 'patch',
    path: '/api/configuration',
    requestSchemaRef: '#/components/schemas/UpdateRuntimeConfigurationRequest',
    responseSchemaRef: '#/components/schemas/RuntimeConfigurationResult'
  }
];
const requiredSchemaProperties = [
  { schema: 'SaveSetupConfigurationRequest', properties: ['databaseConnectionString', 'redisConnectionString', 'keycloakAuthority', 'keycloakRealm', 'keycloakClientId', 'keycloakClientSecret'] },
  { schema: 'SetupStatusResult', properties: ['isDatabaseConfigured', 'isKeycloakConfigured', 'isRedisConfigured', 'readinessState'] },
  { schema: 'SaveRuntimeConfigurationRequest', properties: ['databaseConnectionString', 'redisConnectionString', 'keycloakAuthority', 'keycloakRealm', 'keycloakClientId', 'keycloakClientSecret'] },
  { schema: 'UpdateRuntimeConfigurationRequest', properties: ['databaseConnectionString', 'redisConnectionString', 'keycloakAuthority', 'keycloakRealm', 'keycloakClientId', 'keycloakClientSecret'] },
  { schema: 'RuntimeConfigurationResult', properties: ['isDatabaseConfigured', 'isRedisConfigured', 'isKeycloakConfigured', 'isReadyForProtectedRuntime', 'updatedAtUtc'] }
];
const httpMethods = new Set(['get', 'post', 'put', 'patch', 'delete', 'head', 'options', 'trace']);
const requiredProblemStatusCodes = ['400', '401', '403', '500'];
const problemDetailsSchemaRef = '#/components/schemas/ProblemDetails';

let swagger;

try {
  swagger = JSON.parse(fs.readFileSync(process.argv[2], 'utf8'));
} catch {
  console.error('Error: Swagger document is not valid JSON.');
  process.exit(1);
}

if (!swagger || typeof swagger !== 'object') {
  console.error('Error: Swagger document must be a JSON object.');
  process.exit(1);
}

if (!swagger.openapi && !swagger.swagger) {
  console.error('Error: Swagger document is missing OpenAPI/Swagger version metadata.');
  process.exit(1);
}

if (!swagger.paths || typeof swagger.paths !== 'object') {
  console.error("Error: Swagger document is missing 'paths'.");
  process.exit(1);
}

for (const requiredPath of requiredPaths) {
  if (!Object.prototype.hasOwnProperty.call(swagger.paths, requiredPath)) {
    console.error(`Error: required path "${requiredPath}" was not found in Swagger.`);
    process.exit(1);
  }
}

const hasProblemDetailsSchema =
  swagger.components &&
  swagger.components.schemas &&
  Object.prototype.hasOwnProperty.call(swagger.components.schemas, 'ProblemDetails');

if (!hasProblemDetailsSchema) {
  console.error('Error: Swagger document is missing components.schemas.ProblemDetails.');
  process.exit(1);
}

for (const [path, pathItem] of Object.entries(swagger.paths)) {
  if (!pathItem || typeof pathItem !== 'object') {
    continue;
  }

  for (const [method, operation] of Object.entries(pathItem)) {
    if (!httpMethods.has(method)) {
      continue;
    }

    const operationName = `${method.toUpperCase()} ${path}`;
    if (!operation || typeof operation !== 'object') {
      console.error(`Error: operation "${operationName}" has an invalid schema object.`);
      process.exit(1);
    }

    if (!operation.operationId || typeof operation.operationId !== 'string' || operation.operationId.trim().length === 0) {
      console.error(`Error: operation "${operationName}" is missing a non-empty operationId.`);
      process.exit(1);
    }

    if (!operation.responses || typeof operation.responses !== 'object') {
      console.error(`Error: operation "${operationName}" is missing responses.`);
      process.exit(1);
    }

    for (const statusCode of requiredProblemStatusCodes) {
      const response = operation.responses[statusCode];
      if (!response || typeof response !== 'object') {
        console.error(`Error: operation "${operationName}" is missing standardized ${statusCode} response.`);
        process.exit(1);
      }

      const content = response.content && (response.content['application/problem+json'] || response.content['application/json']);
      const schemaRef = content && content.schema && content.schema.$ref;
      if (schemaRef !== problemDetailsSchemaRef) {
        console.error(`Error: operation "${operationName}" ${statusCode} response must reference ${problemDetailsSchemaRef}.`);
        process.exit(1);
      }
    }
  }
}

const schemas = swagger.components && swagger.components.schemas;
if (!schemas || typeof schemas !== 'object') {
  console.error('Error: Swagger document is missing components.schemas.');
  process.exit(1);
}

for (const contractCheck of persistenceContractChecks) {
  const operation = swagger.paths?.[contractCheck.path]?.[contractCheck.method];
  const operationName = `${contractCheck.method.toUpperCase()} ${contractCheck.path}`;
  if (!operation) {
    console.error(`Error: persistence contract operation "${operationName}" was not found.`);
    process.exit(1);
  }

  if (contractCheck.requestSchemaRef) {
    const requestSchemaRef = operation.requestBody?.content?.['application/json']?.schema?.$ref;
    if (requestSchemaRef !== contractCheck.requestSchemaRef) {
      console.error(`Error: operation "${operationName}" request schema must reference ${contractCheck.requestSchemaRef}.`);
      process.exit(1);
    }
  }

  if (contractCheck.responseSchemaRef) {
    const responseSchemaRef = operation.responses?.['200']?.content?.['application/json']?.schema?.$ref;
    if (responseSchemaRef !== contractCheck.responseSchemaRef) {
      console.error(`Error: operation "${operationName}" 200 response schema must reference ${contractCheck.responseSchemaRef}.`);
      process.exit(1);
    }
  }
}

for (const schemaCheck of requiredSchemaProperties) {
  const schema = schemas[schemaCheck.schema];
  if (!schema || typeof schema !== 'object') {
    console.error(`Error: required schema "${schemaCheck.schema}" was not found.`);
    process.exit(1);
  }

  const properties = schema.properties;
  if (!properties || typeof properties !== 'object') {
    console.error(`Error: schema "${schemaCheck.schema}" is missing properties.`);
    process.exit(1);
  }

  for (const propertyName of schemaCheck.properties) {
    if (!Object.prototype.hasOwnProperty.call(properties, propertyName)) {
      console.error(`Error: schema "${schemaCheck.schema}" is missing "${propertyName}" property.`);
      process.exit(1);
    }
  }
}

console.log('Swagger quality gate passed.');
NODE
