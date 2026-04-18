#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "${SCRIPT_DIR}/.." && pwd)"
SWAGGER_FIXTURE="${REPO_ROOT}/frontend/openapi/proxy-validation.swagger.json"
PROXY_DIR="${REPO_ROOT}/frontend/src/app/shared/proxy"
PROXY_BACKUP_DIR=""

if ! command -v git >/dev/null 2>&1; then
  echo "Error: git is required to validate generated proxy drift." >&2
  exit 1
fi

restore_proxy_dir_on_failure() {
  local exit_code=$?

  if [ "${exit_code}" -ne 0 ] && [ -n "${PROXY_BACKUP_DIR}" ] && [ -d "${PROXY_BACKUP_DIR}" ]; then
    rm -rf "${PROXY_DIR}"
    mkdir -p "${PROXY_DIR}"
    cp -a "${PROXY_BACKUP_DIR}/." "${PROXY_DIR}/"
  fi

  if [ -n "${PROXY_BACKUP_DIR}" ]; then
    rm -rf "${PROXY_BACKUP_DIR}"
  fi

  exit "${exit_code}"
}

trap restore_proxy_dir_on_failure EXIT

PROXY_BACKUP_DIR="$(mktemp -d)"
if [ -d "${PROXY_DIR}" ]; then
  cp -a "${PROXY_DIR}/." "${PROXY_BACKUP_DIR}/"
fi

"${SCRIPT_DIR}/check-swagger-quality.sh" "${SWAGGER_FIXTURE}"
"${SCRIPT_DIR}/generate-proxy.sh" "file://${SWAGGER_FIXTURE}"
"${SCRIPT_DIR}/check-proxy-gateway-consistency.sh" --ocelot "${REPO_ROOT}/backend/Gateway/ocelot.json" --proxy-dir "${REPO_ROOT}/frontend/src/app/shared/proxy/services"

if ! git -C "${REPO_ROOT}" diff --quiet -- frontend/src/app/shared/proxy; then
  echo "Error: generated proxies are not in sync with ${SWAGGER_FIXTURE}." >&2
  echo "Review and commit updated files under frontend/src/app/shared/proxy." >&2
  exit 1
fi

echo "Proxy generation validation passed (no drift detected)."
