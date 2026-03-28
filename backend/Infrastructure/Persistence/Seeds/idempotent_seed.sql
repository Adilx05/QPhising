BEGIN;

-- Prevent concurrent seed runs from racing in shared DB environments.
SELECT pg_advisory_xact_lock(hashtext('qphising-idempotent-seed-v1'));

-- Fail fast with a clear error when schema bootstrap has not been applied yet.
DO $$
DECLARE
    missing_table text;
BEGIN
    SELECT required.table_name
    INTO missing_table
    FROM (VALUES
        ('identity_roles'),
        ('identity_user_role_assumptions'),
        ('identity_user_role_assignments'),
        ('templates'),
        ('template_variables'),
        ('campaigns'),
        ('tracking_clicks'),
        ('queued_tasks'),
        ('task_execution_logs'),
        ('export_jobs')
    ) AS required(table_name)
    WHERE to_regclass(required.table_name) IS NULL
    LIMIT 1;

    IF missing_table IS NOT NULL THEN
        RAISE EXCEPTION 'Cannot run idempotent seed because table % is missing. Apply migrations first.', missing_table;
    END IF;
END;
$$;

WITH anchor AS (
    SELECT '2026-03-28T00:00:00Z'::timestamptz AS ts
)
INSERT INTO identity_roles (role_name, description, is_assignable, created_at_utc)
VALUES
    ('Admin', 'Full platform administration and policy management privileges.', true, (SELECT ts FROM anchor)),
    ('Operator', 'Operational campaign and template management privileges.', true, (SELECT ts FROM anchor)),
    ('Viewer', 'Read-only dashboard and reporting privileges.', true, (SELECT ts FROM anchor))
ON CONFLICT (role_name) DO UPDATE
SET
    description = EXCLUDED.description,
    is_assignable = EXCLUDED.is_assignable;

WITH anchor AS (
    SELECT '2026-03-28T00:00:00Z'::timestamptz AS ts
)
INSERT INTO identity_user_role_assumptions (
    external_user_id,
    username,
    email,
    display_name,
    is_enabled,
    identity_provider,
    created_at_utc)
VALUES
    ('seed-admin', 'admin.user', 'admin@qphising.local', 'QPhising Admin User', true, 'keycloak', (SELECT ts FROM anchor)),
    ('seed-operator', 'operator.user', 'operator@qphising.local', 'QPhising Operator User', true, 'keycloak', (SELECT ts FROM anchor)),
    ('seed-viewer', 'viewer.user', 'viewer@qphising.local', 'QPhising Viewer User', true, 'keycloak', (SELECT ts FROM anchor))
ON CONFLICT (external_user_id) DO UPDATE
SET
    username = EXCLUDED.username,
    email = EXCLUDED.email,
    display_name = EXCLUDED.display_name,
    is_enabled = EXCLUDED.is_enabled,
    identity_provider = EXCLUDED.identity_provider;

INSERT INTO identity_user_role_assignments (external_user_id, role_name, assigned_at_utc)
VALUES
    ('seed-admin', 'Admin', '2026-03-28T00:00:00Z'::timestamptz),
    ('seed-operator', 'Operator', '2026-03-28T00:00:00Z'::timestamptz),
    ('seed-viewer', 'Viewer', '2026-03-28T00:00:00Z'::timestamptz)
ON CONFLICT (external_user_id, role_name) DO NOTHING;

INSERT INTO templates (id, name, type, html_content, status, version)
VALUES
    ('a4510e3f-2366-4f07-b964-4f81d54d00a1', 'Credential Refresh Notice v2', 'Email', '<html><body><h1>Credential refresh required</h1><p>Hello {{first_name}}, your VPN profile must be refreshed before {{due_date}}.</p></body></html>', 'Published', 4),
    ('6ce27f6a-3f63-41fd-aa32-7b7f904f89ec', 'Secure Intranet Login Replica', 'LandingPage', '<html><body><h1>Corporate Portal</h1><form><input name="user"/><input name="password" type="password"/></form></body></html>', 'Published', 2),
    ('9e645801-c0d0-417f-a36d-f4c4249fbc80', 'Quarterly Security Brief Draft', 'Email', '<html><body><h1>Quarterly Security Brief</h1><p>Draft internal announcement content.</p></body></html>', 'Draft', 1),
    ('a7eafb7c-8a27-46f8-b3ec-2f5144cf18e9', 'Legacy Vendor Payroll Alert', 'Email', '<html><body><h1>Payroll verification required</h1><p>This template has been retired.</p></body></html>', 'Archived', 3)
ON CONFLICT (id) DO UPDATE
SET
    name = EXCLUDED.name,
    type = EXCLUDED.type,
    html_content = EXCLUDED.html_content,
    status = EXCLUDED.status,
    version = EXCLUDED.version;

INSERT INTO template_variables (id, template_id, name)
VALUES
    ('8bd715fa-53cb-4bdd-8f0f-2a5f151ce35f', 'a4510e3f-2366-4f07-b964-4f81d54d00a1', 'first_name'),
    ('5002d924-abfd-4f36-95f4-cfbcf957ec65', 'a4510e3f-2366-4f07-b964-4f81d54d00a1', 'due_date'),
    ('58b6f89d-6f40-4f8b-b867-5d7ff7fb6177', '6ce27f6a-3f63-41fd-aa32-7b7f904f89ec', 'employee_email'),
    ('c44390e5-9406-4b22-90cf-7eaf69f18a5a', '6ce27f6a-3f63-41fd-aa32-7b7f904f89ec', 'support_ticket')
ON CONFLICT (id) DO UPDATE
SET
    template_id = EXCLUDED.template_id,
    name = EXCLUDED.name;

INSERT INTO campaigns (id, name, template_type, html_content, start_date, end_date, status)
VALUES
    ('51f4f640-773f-4b6f-b95d-aaf7ded23ad0', 'Q1 Credential Harvest Simulation', 'Email', '<html><body><h2>Urgent: MFA reset</h2><p>Click to reset corporate MFA enrollment.</p></body></html>', '2026-03-08T00:00:00Z'::timestamptz, '2026-03-23T00:00:00Z'::timestamptz, 'Ended'),
    ('d1db00c9-5f94-4345-9fb3-d63da2054430', 'Executive Mailbox Delegation Test', 'Email', '<html><body><h2>Mailbox delegation approval required</h2><p>Review pending request immediately.</p></body></html>', '2026-03-26T00:00:00Z'::timestamptz, '2026-04-09T00:00:00Z'::timestamptz, 'Active'),
    ('b8f1f300-8612-4f62-ab31-9f74b6494800', 'Finance Portal Credential Check', 'LandingPage', '<html><body><h2>Finance Portal Session Timeout</h2><p>Re-authenticate to unlock invoices.</p></body></html>', '2026-04-04T00:00:00Z'::timestamptz, '2026-04-11T00:00:00Z'::timestamptz, 'Scheduled'),
    ('f11cde04-c4c6-4408-978e-abf47d74b1f7', 'Annual Security Awareness Draft', 'Email', '<html><body><h2>Security awareness annual review</h2><p>Draft campaign pending legal review.</p></body></html>', '2026-04-18T00:00:00Z'::timestamptz, '2026-05-02T00:00:00Z'::timestamptz, 'Draft'),
    ('6ef16ff1-11fc-4282-a579-0f91f99d4614', 'Retired Vendor Invoice Scenario', 'Email', '<html><body><h2>Vendor invoice remediation</h2><p>Scenario retained for historical reporting only.</p></body></html>', '2025-11-28T00:00:00Z'::timestamptz, '2026-01-27T00:00:00Z'::timestamptz, 'Archived')
ON CONFLICT (id) DO UPDATE
SET
    name = EXCLUDED.name,
    template_type = EXCLUDED.template_type,
    html_content = EXCLUDED.html_content,
    start_date = EXCLUDED.start_date,
    end_date = EXCLUDED.end_date,
    status = EXCLUDED.status;

INSERT INTO tracking_clicks (id, campaign_id, tracking_token, ip_address, user_agent, fingerprint, clicked_at_utc)
VALUES
    ('f4cc24c7-a252-45f1-8227-c4e2bc6c1117', '51f4f640-773f-4b6f-b95d-aaf7ded23ad0', 'trk-q1-0001', '10.12.4.18', 'Mozilla/5.0 (Windows NT 10.0; Win64; x64)', 'fp-win-001', '2026-03-10T00:00:00Z'::timestamptz),
    ('714ad9c2-fbbf-4b39-bca9-dbdf3f54fac4', '51f4f640-773f-4b6f-b95d-aaf7ded23ad0', 'trk-q1-0002', '10.12.8.41', 'Mozilla/5.0 (Macintosh; Intel Mac OS X 14_4)', 'fp-mac-042', '2026-03-11T00:00:00Z'::timestamptz),
    ('8e4a1a36-6a56-442f-867b-5dbc878d3989', 'd1db00c9-5f94-4345-9fb3-d63da2054430', 'trk-exec-0101', '172.20.3.56', 'Mozilla/5.0 (Windows NT 10.0; Win64; x64)', 'fp-exec-009', '2026-03-26T12:00:00Z'::timestamptz),
    ('c0d29f56-f579-4126-b527-7474c93884e4', 'd1db00c9-5f94-4345-9fb3-d63da2054430', 'trk-exec-0102', '172.20.3.57', 'Mozilla/5.0 (X11; Linux x86_64)', 'fp-lnx-331', '2026-03-26T20:00:00Z'::timestamptz),
    ('f4ba79a5-bf7a-49a3-aa9f-47fc0d7e7fb8', 'd1db00c9-5f94-4345-9fb3-d63da2054430', 'trk-exec-0103', '172.20.3.61', 'Mozilla/5.0 (Windows NT 10.0; Win64; x64)', 'fp-win-113', '2026-03-27T12:00:00Z'::timestamptz)
ON CONFLICT (id) DO UPDATE
SET
    campaign_id = EXCLUDED.campaign_id,
    tracking_token = EXCLUDED.tracking_token,
    ip_address = EXCLUDED.ip_address,
    user_agent = EXCLUDED.user_agent,
    fingerprint = EXCLUDED.fingerprint,
    clicked_at_utc = EXCLUDED.clicked_at_utc;

INSERT INTO queued_tasks (
    id,
    type,
    status,
    payload_json,
    attempt_count,
    max_attempts,
    created_at,
    claimed_at,
    started_at,
    completed_at,
    last_failed_at,
    lease_expires_at,
    next_attempt_at,
    last_error,
    correlation_id)
VALUES
    ('f3cb55e8-20bf-4e72-97af-f4e3c53f9105', 'TrackingClickProcessing', 'Succeeded', '{"campaignId":"d1db00c9-5f94-4345-9fb3-d63da2054430","trackingToken":"trk-exec-0101"}'::jsonb, 1, 5, '2026-03-26T08:00:00Z'::timestamptz, '2026-03-26T08:02:00Z'::timestamptz, '2026-03-26T08:03:00Z'::timestamptz, '2026-03-26T08:04:00Z'::timestamptz, NULL, NULL, '2026-03-26T08:10:00Z'::timestamptz, NULL, 'corr-seed-task-001'),
    ('4effb350-c57b-4ca0-8bf7-64daa2f36010', 'ExportGeneration', 'Failed', '{"exportJobId":"f9b17089-b865-4a88-b02f-28a90f0d8ec6","format":"Pdf"}'::jsonb, 3, 5, '2026-03-27T09:00:00Z'::timestamptz, '2026-03-27T09:05:00Z'::timestamptz, '2026-03-27T09:06:00Z'::timestamptz, NULL, '2026-03-27T09:08:00Z'::timestamptz, '2026-03-27T09:15:00Z'::timestamptz, '2026-03-28T00:15:00Z'::timestamptz, 'wkhtmltopdf worker timeout after 120000 ms', 'corr-seed-task-002'),
    ('3856a0d0-d1bd-45dd-8ca0-373f6da21342', 'CampaignActivation', 'Queued', '{"campaignId":"b8f1f300-8612-4f62-ab31-9f74b6494800"}'::jsonb, 0, 3, '2026-03-27T23:35:00Z'::timestamptz, NULL, NULL, NULL, NULL, NULL, '2026-03-28T00:05:00Z'::timestamptz, NULL, 'corr-seed-task-003')
ON CONFLICT (id) DO UPDATE
SET
    type = EXCLUDED.type,
    status = EXCLUDED.status,
    payload_json = EXCLUDED.payload_json,
    attempt_count = EXCLUDED.attempt_count,
    max_attempts = EXCLUDED.max_attempts,
    created_at = EXCLUDED.created_at,
    claimed_at = EXCLUDED.claimed_at,
    started_at = EXCLUDED.started_at,
    completed_at = EXCLUDED.completed_at,
    last_failed_at = EXCLUDED.last_failed_at,
    lease_expires_at = EXCLUDED.lease_expires_at,
    next_attempt_at = EXCLUDED.next_attempt_at,
    last_error = EXCLUDED.last_error,
    correlation_id = EXCLUDED.correlation_id;

INSERT INTO task_execution_logs (
    id,
    task_id,
    event_type,
    task_status,
    attempt_number,
    occurred_at,
    correlation_id,
    details,
    execution_duration_ms)
VALUES
    ('72525ca1-88c2-49a1-a887-a96d71de58ca', 'f3cb55e8-20bf-4e72-97af-f4e3c53f9105', 'Claimed', 'Claimed', 1, '2026-03-26T08:02:00Z'::timestamptz, 'corr-seed-task-001', 'Task claimed by worker node worker-a.', NULL),
    ('cfde7309-c291-440f-ac35-fad536cd6f5d', 'f3cb55e8-20bf-4e72-97af-f4e3c53f9105', 'Started', 'Running', 1, '2026-03-26T08:03:00Z'::timestamptz, 'corr-seed-task-001', 'Click processing pipeline started.', NULL),
    ('73f712ca-8e85-4480-ab22-e8cb9c521538', 'f3cb55e8-20bf-4e72-97af-f4e3c53f9105', 'Succeeded', 'Succeeded', 1, '2026-03-26T08:04:00Z'::timestamptz, 'corr-seed-task-001', 'Tracking click persisted and analytics cache invalidated.', 942),
    ('e4971fca-d2ee-493c-a392-32e5e6e84e4f', '4effb350-c57b-4ca0-8bf7-64daa2f36010', 'Claimed', 'Claimed', 3, '2026-03-27T09:05:00Z'::timestamptz, 'corr-seed-task-002', 'Export generation task claimed for retry attempt 3.', NULL),
    ('974a7a69-0a60-4905-9af9-bd9f519703bd', '4effb350-c57b-4ca0-8bf7-64daa2f36010', 'Started', 'Running', 3, '2026-03-27T09:06:00Z'::timestamptz, 'corr-seed-task-002', 'PDF report rendering started.', NULL),
    ('f6dca2cb-21b5-4a39-8fa0-b6d67c6f63af', '4effb350-c57b-4ca0-8bf7-64daa2f36010', 'Failed', 'Failed', 3, '2026-03-27T09:08:00Z'::timestamptz, 'corr-seed-task-002', 'PDF renderer timeout exceeded worker SLA.', 120000)
ON CONFLICT (id) DO UPDATE
SET
    task_id = EXCLUDED.task_id,
    event_type = EXCLUDED.event_type,
    task_status = EXCLUDED.task_status,
    attempt_number = EXCLUDED.attempt_number,
    occurred_at = EXCLUDED.occurred_at,
    correlation_id = EXCLUDED.correlation_id,
    details = EXCLUDED.details,
    execution_duration_ms = EXCLUDED.execution_duration_ms;

INSERT INTO export_jobs (
    id,
    owner_user_id,
    export_type,
    format,
    status,
    requested_at,
    queued_at,
    processing_started_at,
    completed_at,
    failed_at,
    canceled_at,
    expires_at,
    file_name,
    storage_path,
    content_type,
    file_size_bytes,
    error_message,
    correlation_id)
VALUES
    ('f9b17089-b865-4a88-b02f-28a90f0d8ec6', 'seed-operator', 'AnalyticsReport', 'Pdf', 'Failed', '2026-03-27T09:00:00Z'::timestamptz, '2026-03-27T09:01:00Z'::timestamptz, '2026-03-27T09:06:00Z'::timestamptz, NULL, '2026-03-27T09:08:00Z'::timestamptz, NULL, '2026-04-27T00:00:00Z'::timestamptz, NULL, NULL, NULL, NULL, 'PDF renderer timeout exceeded worker SLA.', 'corr-seed-task-002'),
    ('4ac85bd2-4e6b-4b0d-9e3c-987290b25a95', 'seed-viewer', 'CampaignReport', 'Excel', 'Completed', '2026-03-27T20:00:00Z'::timestamptz, '2026-03-27T20:02:00Z'::timestamptz, '2026-03-27T20:05:00Z'::timestamptz, '2026-03-27T20:10:00Z'::timestamptz, NULL, NULL, '2026-04-27T00:00:00Z'::timestamptz, 'campaign-report-2026-03-28.xlsx', '/exports/2026/03/campaign-report-2026-03-28.xlsx', 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet', 126812, NULL, 'corr-seed-export-001')
ON CONFLICT (id) DO UPDATE
SET
    owner_user_id = EXCLUDED.owner_user_id,
    export_type = EXCLUDED.export_type,
    format = EXCLUDED.format,
    status = EXCLUDED.status,
    requested_at = EXCLUDED.requested_at,
    queued_at = EXCLUDED.queued_at,
    processing_started_at = EXCLUDED.processing_started_at,
    completed_at = EXCLUDED.completed_at,
    failed_at = EXCLUDED.failed_at,
    canceled_at = EXCLUDED.canceled_at,
    expires_at = EXCLUDED.expires_at,
    file_name = EXCLUDED.file_name,
    storage_path = EXCLUDED.storage_path,
    content_type = EXCLUDED.content_type,
    file_size_bytes = EXCLUDED.file_size_bytes,
    error_message = EXCLUDED.error_message,
    correlation_id = EXCLUDED.correlation_id;

COMMIT;
