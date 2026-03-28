-- Baseline schema snapshot for greenfield database bootstrap.
-- Safe to run multiple times (idempotent) and suitable for container startup initialization.

CREATE TABLE IF NOT EXISTS campaigns (
    id uuid PRIMARY KEY,
    name varchar(200) NOT NULL,
    template_type varchar(50) NOT NULL,
    html_content text NOT NULL,
    start_date timestamptz NOT NULL,
    end_date timestamptz NOT NULL,
    status varchar(50) NOT NULL,
    CONSTRAINT ck_campaigns_date_window CHECK (start_date <= end_date)
);

CREATE INDEX IF NOT EXISTS ix_campaigns_status ON campaigns (status);
CREATE INDEX IF NOT EXISTS ix_campaigns_start_date ON campaigns (start_date);
CREATE INDEX IF NOT EXISTS ix_campaigns_end_date ON campaigns (end_date);
CREATE INDEX IF NOT EXISTS ix_campaigns_status_start_end ON campaigns (status, start_date, end_date);

CREATE TABLE IF NOT EXISTS templates (
    id uuid PRIMARY KEY,
    name character varying(200) NOT NULL,
    type character varying(50) NOT NULL,
    html_content text NOT NULL,
    status character varying(50) NOT NULL,
    version integer NOT NULL
);

CREATE TABLE IF NOT EXISTS template_variables (
    id uuid PRIMARY KEY,
    template_id uuid NOT NULL REFERENCES templates(id) ON DELETE CASCADE,
    name character varying(64) NOT NULL
);

CREATE INDEX IF NOT EXISTS ix_templates_name ON templates (name);
CREATE INDEX IF NOT EXISTS ix_templates_status ON templates (status);
CREATE INDEX IF NOT EXISTS ix_templates_type ON templates (type);
CREATE INDEX IF NOT EXISTS ix_templates_status_type ON templates (status, type);
CREATE UNIQUE INDEX IF NOT EXISTS ux_templates_published_name ON templates (name) WHERE status = 'Published';
CREATE UNIQUE INDEX IF NOT EXISTS ux_template_variables_template_name ON template_variables (template_id, name);

CREATE TABLE IF NOT EXISTS tracking_clicks
(
    id uuid PRIMARY KEY,
    campaign_id uuid NOT NULL,
    tracking_token varchar(512) NOT NULL,
    ip_address varchar(64) NOT NULL,
    user_agent varchar(1024) NOT NULL,
    fingerprint varchar(256) NULL,
    clicked_at_utc timestamptz NOT NULL
);

CREATE INDEX IF NOT EXISTS ix_tracking_clicks_campaign_id ON tracking_clicks (campaign_id);
CREATE INDEX IF NOT EXISTS ix_tracking_clicks_clicked_at_utc ON tracking_clicks (clicked_at_utc);
CREATE INDEX IF NOT EXISTS ix_tracking_clicks_campaign_clicked_at ON tracking_clicks (campaign_id, clicked_at_utc);

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
    next_attempt_at timestamptz NOT NULL,
    last_error varchar(2048) NULL,
    correlation_id varchar(128) NULL
);

CREATE INDEX IF NOT EXISTS ix_queued_tasks_status_created_at ON queued_tasks (status, created_at);
CREATE INDEX IF NOT EXISTS ix_queued_tasks_lease_expires_at ON queued_tasks (lease_expires_at);
CREATE INDEX IF NOT EXISTS ix_queued_tasks_status_next_attempt_at_created_at ON queued_tasks (status, next_attempt_at, created_at);

CREATE TABLE IF NOT EXISTS task_execution_logs (
    id uuid PRIMARY KEY,
    task_id uuid NOT NULL,
    event_type varchar(32) NOT NULL,
    task_status varchar(32) NOT NULL,
    attempt_number integer NOT NULL,
    occurred_at timestamptz NOT NULL,
    correlation_id varchar(128) NULL,
    details varchar(4000) NULL,
    execution_duration_ms bigint NULL,
    CONSTRAINT fk_task_execution_logs_queued_tasks_task_id
        FOREIGN KEY (task_id)
        REFERENCES queued_tasks(id)
        ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS ix_task_execution_logs_task_id_occurred_at ON task_execution_logs(task_id, occurred_at);
CREATE INDEX IF NOT EXISTS ix_task_execution_logs_correlation_id ON task_execution_logs(correlation_id);

CREATE TABLE IF NOT EXISTS export_jobs (
    id uuid PRIMARY KEY,
    owner_user_id varchar(200) NOT NULL,
    export_type varchar(64) NOT NULL,
    format varchar(32) NOT NULL,
    status varchar(32) NOT NULL,
    requested_at timestamptz NOT NULL,
    queued_at timestamptz NULL,
    processing_started_at timestamptz NULL,
    completed_at timestamptz NULL,
    failed_at timestamptz NULL,
    canceled_at timestamptz NULL,
    expires_at timestamptz NULL,
    file_name varchar(255) NULL,
    storage_path varchar(1024) NULL,
    content_type varchar(150) NULL,
    file_size_bytes bigint NULL,
    error_message varchar(2000) NULL,
    correlation_id varchar(100) NULL
);

CREATE INDEX IF NOT EXISTS ix_export_jobs_owner_user_id ON export_jobs (owner_user_id);
CREATE INDEX IF NOT EXISTS ix_export_jobs_status ON export_jobs (status);
CREATE INDEX IF NOT EXISTS ix_export_jobs_owner_status_requested_at ON export_jobs (owner_user_id, status, requested_at DESC);

DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1
        FROM pg_constraint
        WHERE conname = 'fk_tracking_clicks_campaigns_campaign_id')
    THEN
        ALTER TABLE tracking_clicks
            ADD CONSTRAINT fk_tracking_clicks_campaigns_campaign_id
            FOREIGN KEY (campaign_id)
            REFERENCES campaigns(id)
            ON DELETE CASCADE;
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1
        FROM pg_constraint
        WHERE conname = 'ck_export_jobs_file_size_non_negative')
    THEN
        ALTER TABLE export_jobs
            ADD CONSTRAINT ck_export_jobs_file_size_non_negative
            CHECK (file_size_bytes IS NULL OR file_size_bytes >= 0);
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1
        FROM pg_constraint
        WHERE conname = 'ck_task_execution_logs_duration_non_negative')
    THEN
        ALTER TABLE task_execution_logs
            ADD CONSTRAINT ck_task_execution_logs_duration_non_negative
            CHECK (execution_duration_ms IS NULL OR execution_duration_ms >= 0);
    END IF;
END $$;
