CREATE TABLE IF NOT EXISTS setup_state
(
    id INTEGER PRIMARY KEY,
    is_completed BOOLEAN NOT NULL,
    completed_at_utc TIMESTAMPTZ NULL
);

INSERT INTO setup_state (id, is_completed, completed_at_utc)
SELECT 1, FALSE, NULL
WHERE NOT EXISTS (SELECT 1 FROM setup_state WHERE id = 1);
