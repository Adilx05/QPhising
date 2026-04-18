#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

if ! command -v node >/dev/null 2>&1; then
  echo "Error: node is required to validate gateway/Swagger alignment." >&2
  echo "Install Node.js and rerun this script." >&2
  exit 1
fi

node "${SCRIPT_DIR}/check-gateway-swagger-alignment.js" "$@"
