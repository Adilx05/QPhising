DELETE FROM task_execution_logs
WHERE id IN (
    '72525ca1-88c2-49a1-a887-a96d71de58ca',
    'cfde7309-c291-440f-ac35-fad536cd6f5d',
    '73f712ca-8e85-4480-ab22-e8cb9c521538',
    'e4971fca-d2ee-493c-a392-32e5e6e84e4f',
    '974a7a69-0a60-4905-9af9-bd9f519703bd',
    'f6dca2cb-21b5-4a39-8fa0-b6d67c6f63af'
);

DELETE FROM queued_tasks
WHERE id IN (
    'f3cb55e8-20bf-4e72-97af-f4e3c53f9105',
    '4effb350-c57b-4ca0-8bf7-64daa2f36010',
    '3856a0d0-d1bd-45dd-8ca0-373f6da21342'
);

DELETE FROM tracking_clicks
WHERE id IN (
    'f4cc24c7-a252-45f1-8227-c4e2bc6c1117',
    '714ad9c2-fbbf-4b39-bca9-dbdf3f54fac4',
    '8e4a1a36-6a56-442f-867b-5dbc878d3989',
    'c0d29f56-f579-4126-b527-7474c93884e4',
    'f4ba79a5-bf7a-49a3-aa9f-47fc0d7e7fb8'
);

DELETE FROM export_jobs
WHERE id IN (
    'f9b17089-b865-4a88-b02f-28a90f0d8ec6',
    '4ac85bd2-4e6b-4b0d-9e3c-987290b25a95'
);

DELETE FROM campaigns
WHERE id IN (
    '51f4f640-773f-4b6f-b95d-aaf7ded23ad0',
    'd1db00c9-5f94-4345-9fb3-d63da2054430',
    'b8f1f300-8612-4f62-ab31-9f74b6494800',
    'f11cde04-c4c6-4408-978e-abf47d74b1f7',
    '6ef16ff1-11fc-4282-a579-0f91f99d4614'
);

DELETE FROM template_variables
WHERE id IN (
    '8bd715fa-53cb-4bdd-8f0f-2a5f151ce35f',
    '5002d924-abfd-4f36-95f4-cfbcf957ec65',
    '58b6f89d-6f40-4f8b-b867-5d7ff7fb6177',
    'c44390e5-9406-4b22-90cf-7eaf69f18a5a'
);

DELETE FROM templates
WHERE id IN (
    'a4510e3f-2366-4f07-b964-4f81d54d00a1',
    '6ce27f6a-3f63-41fd-aa32-7b7f904f89ec',
    '9e645801-c0d0-417f-a36d-f4c4249fbc80',
    'a7eafb7c-8a27-46f8-b3ec-2f5144cf18e9'
);
