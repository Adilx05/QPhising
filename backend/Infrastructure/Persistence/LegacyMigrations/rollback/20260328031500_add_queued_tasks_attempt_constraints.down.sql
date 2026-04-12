-- Rollback for queued task retry constraints.

ALTER TABLE queued_tasks
    DROP CONSTRAINT IF EXISTS ck_queued_tasks_attempt_count_not_greater_than_max;

ALTER TABLE queued_tasks
    DROP CONSTRAINT IF EXISTS ck_queued_tasks_max_attempts_positive;

ALTER TABLE queued_tasks
    DROP CONSTRAINT IF EXISTS ck_queued_tasks_attempt_count_non_negative;
