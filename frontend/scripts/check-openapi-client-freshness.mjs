#!/usr/bin/env node
import { dirname, resolve } from 'node:path';
import { fileURLToPath } from 'node:url';
import { spawnSync } from 'node:child_process';

const scriptDir = dirname(fileURLToPath(import.meta.url));
const frontendDir = resolve(scriptDir, '..');

const run = (command, args) => {
  const result = spawnSync(command, args, {
    cwd: frontendDir,
    stdio: 'inherit',
    shell: false
  });

  if (result.error) {
    throw result.error;
  }

  if (result.status !== 0) {
    process.exit(result.status ?? 1);
  }
};

run('node', ['./scripts/generate-openapi-client.mjs']);

const diffResult = spawnSync('git', ['diff', '--exit-code', '--', 'src/app/core/api/generated'], {
  cwd: frontendDir,
  stdio: 'inherit',
  shell: false
});

if (diffResult.status !== 0) {
  console.error('ERROR: Generated OpenAPI client artifacts are stale.');
  console.error("Run 'cd frontend && npm run generate:api-client', then commit updated files under src/app/core/api/generated/.");
  process.exit(diffResult.status ?? 1);
}

console.log('OpenAPI generated client artifacts are fresh.');
