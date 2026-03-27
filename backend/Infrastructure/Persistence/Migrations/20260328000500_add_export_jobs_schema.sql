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
CREATE INDEX IF NOT EXISTS ix_export_jobs_owner_status_requested_at
    ON export_jobs (owner_user_id, status, requested_at DESC);
