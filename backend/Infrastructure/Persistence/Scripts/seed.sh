#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
SEED_FILE="$(cd "$SCRIPT_DIR/../Seeds" && pwd)/idempotent_seed.sql"

: "${DATABASE_URL:?DATABASE_URL environment variable must be set}"

psql "$DATABASE_URL" -v ON_ERROR_STOP=1 -f "$SEED_FILE"

echo "Idempotent seed completed successfully."
