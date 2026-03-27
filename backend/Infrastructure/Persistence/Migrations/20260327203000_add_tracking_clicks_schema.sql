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

CREATE INDEX IF NOT EXISTS ix_tracking_clicks_campaign_id
    ON tracking_clicks (campaign_id);

CREATE INDEX IF NOT EXISTS ix_tracking_clicks_clicked_at_utc
    ON tracking_clicks (clicked_at_utc);

CREATE INDEX IF NOT EXISTS ix_tracking_clicks_campaign_clicked_at
    ON tracking_clicks (campaign_id, clicked_at_utc);
