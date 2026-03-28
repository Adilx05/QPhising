#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
cd "$ROOT_DIR"

COMPOSE_FILE="${COMPOSE_FILE:-docker-compose.yml}"
STARTUP_TIMEOUT_SECONDS="${STARTUP_TIMEOUT_SECONDS:-600}"
POLL_INTERVAL_SECONDS="${POLL_INTERVAL_SECONDS:-5}"

API_URL="${API_HEALTH_URL:-http://localhost:${API_PORT:-5000}/health}"
GATEWAY_URL="${GATEWAY_HEALTH_URL:-http://localhost:${GATEWAY_PORT:-5001}/health}"
FRONTEND_URL="${FRONTEND_HEALTH_URL:-http://localhost:${FRONTEND_PORT:-4200}/}"
KEYCLOAK_URL="${KEYCLOAK_HEALTH_URL:-http://localhost:${KEYCLOAK_PORT:-8080}/health/ready}"

if ! command -v docker >/dev/null 2>&1; then
  echo "[ERROR] docker CLI is required but was not found in PATH." >&2
  exit 1
fi

cleanup() {
  docker compose -f "$COMPOSE_FILE" down -v --remove-orphans >/dev/null 2>&1 || true
}
trap cleanup EXIT

wait_for_healthy_stack() {
  local deadline=$((SECONDS + STARTUP_TIMEOUT_SECONDS))

  while (( SECONDS < deadline )); do
    if docker compose -f "$COMPOSE_FILE" ps --format json | python3 - <<'PY'
import json
import sys

services = json.load(sys.stdin)
if not services:
    sys.exit(1)

allowed_states = {"running", "exited"}
for service in services:
    state = (service.get("State") or "").lower()
    health = (service.get("Health") or "").lower()
    if state not in allowed_states:
        sys.exit(1)
    if health and health != "healthy":
        sys.exit(1)

sys.exit(0)
PY
    then
      return 0
    fi

    sleep "$POLL_INTERVAL_SECONDS"
  done

  return 1
}

assert_reachable() {
  local url="$1"
  local name="$2"

  if curl -fsS "$url" >/dev/null; then
    echo "[OK] $name reachable: $url"
  else
    echo "[ERROR] $name not reachable: $url" >&2
    return 1
  fi
}

echo "[INFO] Bringing up full stack from $COMPOSE_FILE"
docker compose -f "$COMPOSE_FILE" up --build -d --remove-orphans

echo "[INFO] Waiting for service health convergence"
if ! wait_for_healthy_stack; then
  echo "[ERROR] Stack did not become healthy within ${STARTUP_TIMEOUT_SECONDS}s" >&2
  docker compose -f "$COMPOSE_FILE" ps
  exit 1
fi

echo "[INFO] Running reachability checks"
assert_reachable "$KEYCLOAK_URL" "keycloak"
assert_reachable "$API_URL" "api"
assert_reachable "$GATEWAY_URL" "gateway"
assert_reachable "$FRONTEND_URL" "frontend"

echo "[INFO] Full-stack lifecycle verification passed"
docker compose -f "$COMPOSE_FILE" ps
