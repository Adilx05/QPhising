#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "${SCRIPT_DIR}/.." && pwd)"
DEFAULT_SWAGGER_URL="http://localhost:5000/swagger/v1/swagger.json"
SWAGGER_URL="${1:-$DEFAULT_SWAGGER_URL}"
OUTPUT_DIR="${REPO_ROOT}/frontend/src/app/shared/proxy"

if ! command -v npx >/dev/null 2>&1; then
  echo "Error: npx is required to generate TypeScript clients." >&2
  echo "Install Node.js (which includes npm/npx) and rerun this script." >&2
  exit 1
fi

if ! command -v node >/dev/null 2>&1; then
  echo "Error: node is required to validate Swagger content before generation." >&2
  echo "Install Node.js and rerun this script." >&2
  exit 1
fi

validate_swagger_document() {
  local swagger_url="$1"

  if ! command -v curl >/dev/null 2>&1; then
    echo "Error: curl is required to fetch Swagger JSON from '${swagger_url}'." >&2
    exit 1
  fi

  local swagger_json
  if ! swagger_json="$(curl --fail --silent --show-error --location "${swagger_url}")"; then
    echo "Error: failed to fetch Swagger document from '${swagger_url}'." >&2
    echo "Ensure the API is running and Swagger is accessible before generating proxies." >&2
    exit 1
  fi

  if [ -z "${swagger_json}" ]; then
    echo "Error: Swagger document at '${swagger_url}' is empty." >&2
    exit 1
  fi

  if ! node -e '
const requiredPath = "/api/proxy-validation/assert-sync";
let swagger;

try {
  swagger = JSON.parse(process.argv[1]);
} catch {
  console.error("Error: Swagger response is not valid JSON.");
  process.exit(1);
}

if (!swagger || typeof swagger !== "object") {
  console.error("Error: Swagger document must be a JSON object.");
  process.exit(1);
}

if (!swagger.openapi && !swagger.swagger) {
  console.error("Error: Swagger document is missing OpenAPI/Swagger version metadata.");
  process.exit(1);
}

if (!swagger.paths || typeof swagger.paths !== "object") {
  console.error("Error: Swagger document is missing 'paths'.");
  process.exit(1);
}

if (!Object.prototype.hasOwnProperty.call(swagger.paths, requiredPath)) {
  console.error(`Error: required path '${requiredPath}' was not found in Swagger. Run backend contract updates first.`);
  process.exit(1);
}
' "${swagger_json}"; then
    exit 1
  fi
}

echo "Validating Swagger preconditions from: ${SWAGGER_URL}"
validate_swagger_document "${SWAGGER_URL}"

echo "Generating TypeScript proxy from: ${SWAGGER_URL}"
echo "Output directory: ${OUTPUT_DIR}"

rm -rf "${OUTPUT_DIR}"
mkdir -p "${OUTPUT_DIR}"

npx --yes openapi-typescript-codegen \
  --input "${SWAGGER_URL}" \
  --output "${OUTPUT_DIR}" \
  --client axios \
  --useOptions \
  --exportCore true \
  --exportServices true \
  --exportModels true \
  --indent 2

if [ ! -f "${OUTPUT_DIR}/index.ts" ]; then
  echo "Error: proxy generation did not produce ${OUTPUT_DIR}/index.ts" >&2
  exit 1
fi

echo "Proxy generation completed successfully."
