#!/usr/bin/env bash
set -euo pipefail

if [[ $# -lt 1 ]]; then
  echo "Usage: $0 <up|down>"
  exit 1
fi

DIRECTION="$1"
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
MIGRATIONS_DIR="$(cd "$SCRIPT_DIR/../Migrations" && pwd)"
ROLLBACK_DIR="$MIGRATIONS_DIR/rollback"

: "${DATABASE_URL:?DATABASE_URL environment variable must be set}"
PSQL_CMD=(psql "$DATABASE_URL" -v ON_ERROR_STOP=1)

"${PSQL_CMD[@]}" <<'SQL'
CREATE TABLE IF NOT EXISTS schema_migrations (
    migration_id varchar(64) PRIMARY KEY,
    applied_at_utc timestamptz NOT NULL DEFAULT (now() at time zone 'utc')
);
SQL

if [[ "$DIRECTION" == "up" ]]; then
  while IFS= read -r migration; do
    migration_file="$(basename "$migration")"
    migration_id="${migration_file%.sql}"

    already_applied="$("${PSQL_CMD[@]}" -tA -c "SELECT EXISTS(SELECT 1 FROM schema_migrations WHERE migration_id = '$migration_id');")"
    if [[ "$already_applied" == "t" ]]; then
      continue
    fi

    echo "Applying $migration_file"
    "${PSQL_CMD[@]}" -f "$migration"
    "${PSQL_CMD[@]}" -c "INSERT INTO schema_migrations (migration_id) VALUES ('$migration_id');"
  done < <(find "$MIGRATIONS_DIR" -maxdepth 1 -type f -name '*.sql' | sort)

  echo "Migration up completed."
  exit 0
fi

if [[ "$DIRECTION" == "down" ]]; then
  latest_migration_id="$("${PSQL_CMD[@]}" -tA -c "SELECT migration_id FROM schema_migrations ORDER BY migration_id DESC LIMIT 1;")"

  if [[ -z "$latest_migration_id" ]]; then
    echo "No applied migrations found in schema_migrations."
    exit 0
  fi

  rollback_file="$ROLLBACK_DIR/${latest_migration_id}.down.sql"
  if [[ ! -f "$rollback_file" ]]; then
    echo "Rollback script not found for migration '$latest_migration_id': $rollback_file"
    exit 1
  fi

  echo "Rolling back $latest_migration_id"
  "${PSQL_CMD[@]}" -f "$rollback_file"
  "${PSQL_CMD[@]}" -c "DELETE FROM schema_migrations WHERE migration_id = '$latest_migration_id';"

  echo "Migration down completed."
  exit 0
fi

echo "Unsupported direction: $DIRECTION"
exit 1
