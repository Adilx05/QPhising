-- Harden queue data integrity for retry semantics.

DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1
        FROM pg_constraint
        WHERE conname = 'ck_queued_tasks_attempt_count_non_negative')
    THEN
        ALTER TABLE queued_tasks
            ADD CONSTRAINT ck_queued_tasks_attempt_count_non_negative
            CHECK (attempt_count >= 0);
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1
        FROM pg_constraint
        WHERE conname = 'ck_queued_tasks_max_attempts_positive')
    THEN
        ALTER TABLE queued_tasks
            ADD CONSTRAINT ck_queued_tasks_max_attempts_positive
            CHECK (max_attempts > 0);
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1
        FROM pg_constraint
        WHERE conname = 'ck_queued_tasks_attempt_count_not_greater_than_max')
    THEN
        ALTER TABLE queued_tasks
            ADD CONSTRAINT ck_queued_tasks_attempt_count_not_greater_than_max
            CHECK (attempt_count <= max_attempts);
    END IF;
END $$;
