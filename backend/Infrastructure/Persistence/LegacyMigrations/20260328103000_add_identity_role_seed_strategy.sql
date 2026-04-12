CREATE TABLE IF NOT EXISTS identity_roles (
    role_name varchar(64) PRIMARY KEY,
    description varchar(256) NOT NULL,
    is_assignable boolean NOT NULL DEFAULT true,
    created_at_utc timestamptz NOT NULL
);

CREATE TABLE IF NOT EXISTS identity_user_role_assumptions (
    external_user_id varchar(128) PRIMARY KEY,
    username varchar(128) NOT NULL,
    email varchar(320) NOT NULL,
    display_name varchar(200) NOT NULL,
    is_enabled boolean NOT NULL DEFAULT true,
    identity_provider varchar(64) NOT NULL,
    created_at_utc timestamptz NOT NULL
);

CREATE TABLE IF NOT EXISTS identity_user_role_assignments (
    external_user_id varchar(128) NOT NULL,
    role_name varchar(64) NOT NULL,
    assigned_at_utc timestamptz NOT NULL,
    CONSTRAINT pk_identity_user_role_assignments PRIMARY KEY (external_user_id, role_name),
    CONSTRAINT fk_identity_user_role_assignments_user
        FOREIGN KEY (external_user_id)
        REFERENCES identity_user_role_assumptions(external_user_id)
        ON DELETE CASCADE,
    CONSTRAINT fk_identity_user_role_assignments_role
        FOREIGN KEY (role_name)
        REFERENCES identity_roles(role_name)
        ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS ix_identity_user_role_assumptions_username
    ON identity_user_role_assumptions (username);

CREATE INDEX IF NOT EXISTS ix_identity_user_role_assumptions_email
    ON identity_user_role_assumptions (email);

CREATE INDEX IF NOT EXISTS ix_identity_user_role_assignments_role_name
    ON identity_user_role_assignments (role_name);

INSERT INTO identity_roles (role_name, description, is_assignable, created_at_utc)
VALUES
    ('Admin', 'Full platform administration and policy management privileges.', true, NOW()),
    ('Operator', 'Operational campaign and template management privileges.', true, NOW()),
    ('Viewer', 'Read-only dashboard and reporting privileges.', true, NOW())
ON CONFLICT (role_name) DO UPDATE
SET
    description = EXCLUDED.description,
    is_assignable = EXCLUDED.is_assignable;

INSERT INTO identity_user_role_assumptions (
    external_user_id,
    username,
    email,
    display_name,
    is_enabled,
    identity_provider,
    created_at_utc)
VALUES
    ('seed-admin', 'admin.user', 'admin@qphising.local', 'QPhising Admin User', true, 'keycloak', NOW()),
    ('seed-operator', 'operator.user', 'operator@qphising.local', 'QPhising Operator User', true, 'keycloak', NOW()),
    ('seed-viewer', 'viewer.user', 'viewer@qphising.local', 'QPhising Viewer User', true, 'keycloak', NOW())
ON CONFLICT (external_user_id) DO UPDATE
SET
    username = EXCLUDED.username,
    email = EXCLUDED.email,
    display_name = EXCLUDED.display_name,
    is_enabled = EXCLUDED.is_enabled,
    identity_provider = EXCLUDED.identity_provider;

INSERT INTO identity_user_role_assignments (external_user_id, role_name, assigned_at_utc)
VALUES
    ('seed-admin', 'Admin', NOW()),
    ('seed-operator', 'Operator', NOW()),
    ('seed-viewer', 'Viewer', NOW())
ON CONFLICT (external_user_id, role_name) DO NOTHING;
