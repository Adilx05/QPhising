# SQL Migrations

This folder stores ordered SQL migrations for PostgreSQL.

## Structure

- `00000000000000_baseline_full_schema.sql`: baseline snapshot for greenfield bootstrap.
- `YYYYMMDDHHMMSS_<name>.sql`: incremental forward migrations.
- `20260328103000_add_identity_role_seed_strategy.sql`: identity role assumptions and deterministic RBAC seed metadata.
- `20260328113000_seed_representative_business_analytics_data.sql`: representative campaigns/templates/tasks/tracking/export bootstrap dataset for analytics and dashboard validation.
- `rollback/YYYYMMDDHHMMSS_<name>.down.sql`: optional rollback scripts for downgrade validation.

## Apply / Rollback

Use the migration runner script:

```bash
DATABASE_URL="postgresql://postgres:postgres@localhost:5432/qphising" \
  ./backend/Infrastructure/Persistence/Scripts/migrate.sh up
```

```bash
DATABASE_URL="postgresql://postgres:postgres@localhost:5432/qphising" \
  ./backend/Infrastructure/Persistence/Scripts/migrate.sh down
```

The script persists migration state in `schema_migrations` and executes migrations in lexical order.
