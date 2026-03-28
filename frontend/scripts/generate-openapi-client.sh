#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
FRONTEND_DIR="$(cd "${SCRIPT_DIR}/.." && pwd)"

OPENAPI_SPEC_URL="${OPENAPI_SPEC_URL:-http://localhost:5000/openapi/v1.json}"
GENERATOR_IMAGE="${OPENAPI_GENERATOR_IMAGE:-openapitools/openapi-generator-cli:v7.14.0}"

cd "${FRONTEND_DIR}"

rm -rf src/app/core/api/generated
mkdir -p src/app/core/api/generated

docker run --rm \
  --user "$(id -u):$(id -g)" \
  --volume "${FRONTEND_DIR}:/local" \
  "${GENERATOR_IMAGE}" generate \
  --generator-name typescript-angular \
  --input-spec "${OPENAPI_SPEC_URL}" \
  --config "/local/openapi/openapi-generator.config.json" \
  --output "/local/src/app/core/api/generated"
