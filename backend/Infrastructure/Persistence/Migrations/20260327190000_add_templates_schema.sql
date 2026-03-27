-- Template persistence schema
CREATE TABLE IF NOT EXISTS templates (
    id uuid PRIMARY KEY,
    name character varying(200) NOT NULL,
    type character varying(50) NOT NULL,
    html_content text NOT NULL,
    status character varying(50) NOT NULL,
    version integer NOT NULL
);

CREATE TABLE IF NOT EXISTS template_variables (
    id uuid PRIMARY KEY,
    template_id uuid NOT NULL REFERENCES templates(id) ON DELETE CASCADE,
    name character varying(64) NOT NULL
);

CREATE INDEX IF NOT EXISTS ix_templates_name ON templates (name);
CREATE INDEX IF NOT EXISTS ix_templates_status ON templates (status);
CREATE INDEX IF NOT EXISTS ix_templates_type ON templates (type);
CREATE INDEX IF NOT EXISTS ix_templates_status_type ON templates (status, type);
CREATE UNIQUE INDEX IF NOT EXISTS ux_templates_published_name ON templates (name) WHERE status = 'Published';

CREATE UNIQUE INDEX IF NOT EXISTS ux_template_variables_template_name ON template_variables (template_id, name);
