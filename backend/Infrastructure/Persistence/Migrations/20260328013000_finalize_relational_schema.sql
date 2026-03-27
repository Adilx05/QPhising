-- Finalize relational schema across active modules.
-- Covers campaigns baseline table and cross-module integrity constraints/indexes.

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
