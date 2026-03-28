# QPhising Operational Runbook

## 1) Scope, ownership, and severity model

This runbook defines production operations for the QPhising stack (`frontend`, `gateway`, `api`, `worker`, `postgres`, `redis`, `keycloak`) deployed via `docker compose`.

### On-call ownership

- **Primary on-call:** Platform/DevOps engineer
- **Secondary on-call:** Backend engineer
- **Escalation:** Security lead for authentication, abuse, or data exposure incidents

### Incident severities

- **SEV-1:** Full outage, data loss risk, or active security breach
- **SEV-2:** Partial outage or degraded core path (campaign/tracking/export)
- **SEV-3:** Non-critical degradation, retryable failures, or isolated user impact

## 2) Backup procedures

## 2.1 PostgreSQL logical backup

Frequency: **daily full backup** + **before every schema migration**.
Retention: **30 days** in off-host/object storage.

### Backup command

```bash
timestamp="$(date -u +%Y%m%dT%H%M%SZ)"
mkdir -p backups/postgres

docker compose exec -T postgres \
  pg_dump -U "${POSTGRES_USER:-qphising}" -d "${POSTGRES_DB:-qphising}" -F c \
  > "backups/postgres/qphising_${timestamp}.dump"
```

### Integrity check command

```bash
pg_restore -l "backups/postgres/qphising_${timestamp}.dump" >/dev/null
```

Success criteria:
- Backup file exists and is non-empty.
- `pg_restore -l` exits with code `0`.

## 2.2 Redis durability snapshot

Redis is used for cache/rate-limit/deduplication and should not be system-of-record. Snapshotting still helps post-incident analysis.

Frequency: **daily snapshot**.
Retention: **7 days**.

### Snapshot command

```bash
docker compose exec -T redis redis-cli BGSAVE
docker compose exec -T redis redis-cli LASTSAVE
```

Success criteria:
- `BGSAVE` accepted.
- `LASTSAVE` timestamp advances compared to previous run.

## 2.3 Export artifact backup

If export files are persisted on host volumes, back up storage root configured by `ExportStorage:BasePath`.

```bash
# Example when BasePath is mounted under ./data/exports
rsync -a --delete ./data/exports/ "./backups/exports/$(date -u +%Y%m%dT%H%M%SZ)/"
```

Success criteria:
- Backup directory created with expected file count.
- Random sample file checksum matches source.

## 3) Restore procedures

## 3.1 PostgreSQL restore (disaster recovery)

1. Stop write traffic (gateway/api/worker) to avoid new writes during restore.
2. Restore from a verified dump.
3. Validate health and core read/write paths.

### Restore commands

```bash
docker compose stop gateway api worker

cat backups/postgres/<backup-file>.dump | docker compose exec -T postgres \
  pg_restore -U "${POSTGRES_USER:-qphising}" -d "${POSTGRES_DB:-qphising}" \
  --clean --if-exists --no-owner --no-privileges

docker compose start api worker gateway
```

### Post-restore validation

```bash
docker compose ps
curl -fsS http://localhost:5000/health/ready
curl -fsS http://localhost:8080/api-health/ready
```

Success criteria:
- Services return to `running`/`healthy`.
- API and gateway readiness probes return HTTP `200`.

## 3.2 Redis restore

Redis data is non-authoritative; standard recovery is cache warm-up by application traffic.

For incident forensics requiring snapshot restore:
1. Stop dependent services (`gateway`, `api`, `worker`).
2. Restore `dump.rdb` into Redis data volume.
3. Start Redis and then dependent services.

Validate by checking:

```bash
docker compose exec -T redis redis-cli PING
```

Expected output: `PONG`.

## 4) Incident response workflow

## 4.1 Detection and triage

Detection sources:
- CI/CD failures for security/quality gates.
- Health endpoint failures (`/health/ready`, `/api-health/ready`).
- Error-rate or latency anomalies in logs.
- Security alerts (auth failures, throttling spikes, suspicious tracking activity).

Immediate triage checklist:
1. Record incident start time (UTC), reporter, impacted scope.
2. Assign severity (SEV-1/2/3).
3. Open incident channel and assign commander.
4. Freeze risky deploys until containment is complete.

## 4.2 Containment and mitigation playbook

- **Auth outage (Keycloak/JWT):**
  - Verify `keycloak` health and authority config.
  - Temporarily disable rollout and recycle `gateway`/`api` after keycloak recovery.
- **Rate-limit abuse / attack traffic:**
  - Validate gateway logs include correlation IDs and throttle metadata.
  - Tighten rate-limit policy and block abusive client IDs/IPs.
- **Database degradation:**
  - Shift to read-only operations if required.
  - Execute fail-safe restart order: `postgres` -> `api` -> `worker` -> `gateway`.

## 4.3 Recovery and closure

1. Confirm user-impact path recovery (campaign, tracking, exports).
2. Verify health/readiness endpoints.
3. Close incident only after 30-minute stability window.
4. Publish post-incident report within 24 hours:
   - timeline,
   - root cause,
   - corrective actions,
   - prevention actions with owners/due dates.

## 5) Logging and observability guidance

## 5.1 Required telemetry signals

- **Health:**
  - API: `/health`, `/health/live`, `/health/ready`
  - Gateway: `/api-health/live`, `/api-health/ready`
- **Correlation:** enforce `X-Correlation-ID` propagation for request tracing.
- **Structured access logs (gateway):** route, status, latency, principal, throttle decision.
- **Application logs:** validation failures, auth failures, domain transition errors.

## 5.2 Operational commands

### Stack and health status

```bash
docker compose ps
curl -fsS http://localhost:5000/health/live
curl -fsS http://localhost:5000/health/ready
curl -fsS http://localhost:8080/api-health/live
curl -fsS http://localhost:8080/api-health/ready
```

### Focused log tails

```bash
docker compose logs -f gateway api worker
```

### Correlation search pattern

Use a known correlation ID from response header and filter logs:

```bash
docker compose logs gateway api worker | rg "<correlation-id>"
```

## 5.3 Alerting recommendations

Configure alerts for:
- readiness probe failures > 3 minutes,
- sustained 5xx rate > 2% for 5 minutes,
- p95 latency above service SLO,
- repeated auth/authorization failures above baseline,
- gateway throttling spikes indicating abuse or policy misconfiguration.

## 6) Change management and evidence

For every operational change or incident action:
- capture commands executed,
- capture exact timestamps in UTC,
- capture impacted services,
- link PR/commit and follow-up tasks in `TASKS.md`.

This runbook must be reviewed quarterly or after every SEV-1 incident.
