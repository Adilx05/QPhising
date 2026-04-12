CREATE TABLE IF NOT EXISTS system_settings
(
    key VARCHAR(128) PRIMARY KEY,
    value VARCHAR(2048) NOT NULL,
    updated_at_utc TIMESTAMPTZ NOT NULL
);

INSERT INTO system_settings (key, value, updated_at_utc)
VALUES ('setup.is_completed', 'false', NOW())
ON CONFLICT (key) DO NOTHING;
