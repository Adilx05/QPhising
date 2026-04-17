#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "${SCRIPT_DIR}/.." && pwd)"
DEFAULT_SWAGGER_URL="http://localhost:5000/swagger/v1/swagger.json"
SWAGGER_URL="${1:-$DEFAULT_SWAGGER_URL}"
OUTPUT_DIR="${REPO_ROOT}/frontend/src/app/shared/proxy"
GENERATOR_VERSION="${GENERATOR_VERSION:-0.29.0}"
GENERATOR_PACKAGE="openapi-typescript-codegen@${GENERATOR_VERSION}"
SWAGGER_TEMP_FILE="$(mktemp)"

cleanup() {
  rm -f "${SWAGGER_TEMP_FILE}"
}
trap cleanup EXIT

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

download_and_validate_swagger_document() {
  local swagger_url="$1"
  local output_file="$2"

  if ! command -v curl >/dev/null 2>&1; then
    echo "Error: curl is required to fetch Swagger JSON from '${swagger_url}'." >&2
    exit 1
  fi

  if ! curl --fail --silent --show-error --location "${swagger_url}" --output "${output_file}"; then
    echo "Error: failed to fetch Swagger document from '${swagger_url}'." >&2
    echo "Ensure the API is running and Swagger is accessible before generating proxies." >&2
    exit 1
  fi

  if [ ! -s "${output_file}" ]; then
    echo "Error: Swagger document at '${swagger_url}' is empty." >&2
    exit 1
  fi

  if ! node -e '
const fs = require("fs");
const requiredPath = "/api/proxy-validation/assert-sync";
let swagger;

try {
  swagger = JSON.parse(fs.readFileSync(process.argv[1], "utf8"));
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
  console.error(`Error: required path "${requiredPath}" was not found in Swagger. Run backend contract updates first.`);
  process.exit(1);
}
' "${output_file}"; then
    exit 1
  fi
}

normalize_generated_line_endings() {
  local output_dir="$1"

  node -e '
const fs = require("fs");
const path = require("path");

const root = process.argv[1];
const exts = new Set([".ts", ".js", ".json", ".md"]);

function walk(dir) {
  for (const entry of fs.readdirSync(dir, { withFileTypes: true })) {
    const fullPath = path.join(dir, entry.name);
    if (entry.isDirectory()) {
      walk(fullPath);
      continue;
    }

    if (!exts.has(path.extname(entry.name))) {
      continue;
    }

    const original = fs.readFileSync(fullPath, "utf8");
    const normalized = original.replace(/\r\n/g, "\n");
    if (normalized !== original) {
      fs.writeFileSync(fullPath, normalized, "utf8");
    }
  }
}

if (!fs.existsSync(root)) {
  process.exit(0);
}

walk(root);
' "${output_dir}"
}

echo "Validating Swagger preconditions from: ${SWAGGER_URL}"
download_and_validate_swagger_document "${SWAGGER_URL}" "${SWAGGER_TEMP_FILE}"

echo "Generating TypeScript proxy from: ${SWAGGER_URL}"
echo "Output directory: ${OUTPUT_DIR}"
echo "Generator package: ${GENERATOR_PACKAGE}"

rm -rf "${OUTPUT_DIR}"
mkdir -p "${OUTPUT_DIR}"

npx --yes "${GENERATOR_PACKAGE}" \
  --input "${SWAGGER_TEMP_FILE}" \
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

normalize_generated_line_endings "${OUTPUT_DIR}"

echo "Proxy generation completed successfully."
