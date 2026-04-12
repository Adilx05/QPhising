CREATE TABLE IF NOT EXISTS queued_tasks
(
    id uuid PRIMARY KEY,
    type varchar(64) NOT NULL,
    status varchar(64) NOT NULL,
    payload_json jsonb NOT NULL,
    attempt_count integer NOT NULL DEFAULT 0,
    max_attempts integer NOT NULL,
    created_at timestamptz NOT NULL,
    claimed_at timestamptz NULL,
    started_at timestamptz NULL,
    completed_at timestamptz NULL,
    last_failed_at timestamptz NULL,
    lease_expires_at timestamptz NULL,
    last_error varchar(2048) NULL,
    correlation_id varchar(128) NULL
);

CREATE INDEX IF NOT EXISTS ix_queued_tasks_status_created_at
    ON queued_tasks (status, created_at);

CREATE INDEX IF NOT EXISTS ix_queued_tasks_lease_expires_at
    ON queued_tasks (lease_expires_at);
