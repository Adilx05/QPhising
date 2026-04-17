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
