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

CREATE INDEX IF NOT EXISTS ix_task_execution_logs_task_id_occurred_at
    ON task_execution_logs(task_id, occurred_at);

CREATE INDEX IF NOT EXISTS ix_task_execution_logs_correlation_id
    ON task_execution_logs(correlation_id);
