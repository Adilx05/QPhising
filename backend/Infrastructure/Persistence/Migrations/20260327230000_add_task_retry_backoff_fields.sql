ALTER TABLE queued_tasks
    ADD COLUMN IF NOT EXISTS next_attempt_at timestamptz;

UPDATE queued_tasks
SET next_attempt_at = created_at
WHERE next_attempt_at IS NULL;

ALTER TABLE queued_tasks
    ALTER COLUMN next_attempt_at SET NOT NULL;

CREATE INDEX IF NOT EXISTS ix_queued_tasks_status_next_attempt_at_created_at
    ON queued_tasks (status, next_attempt_at, created_at);
