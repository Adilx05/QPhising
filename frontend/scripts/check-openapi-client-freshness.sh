#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
FRONTEND_DIR="$(cd "${SCRIPT_DIR}/.." && pwd)"

cd "${FRONTEND_DIR}"

./scripts/generate-openapi-client.sh

if ! git diff --exit-code -- src/app/core/api/generated; then
  echo "ERROR: Generated OpenAPI client artifacts are stale."
  echo "Run 'cd frontend && npm run generate:api-client', then commit updated files under src/app/core/api/generated/."
  exit 1
fi

echo "OpenAPI generated client artifacts are fresh."
