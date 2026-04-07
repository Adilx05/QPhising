#!/usr/bin/env node
import { existsSync, mkdirSync, rmSync } from 'node:fs';
import { dirname, resolve } from 'node:path';
import { fileURLToPath } from 'node:url';
import { spawnSync } from 'node:child_process';

const scriptDir = dirname(fileURLToPath(import.meta.url));
const frontendDir = resolve(scriptDir, '..');

const openApiSpecUrl = process.env.OPENAPI_SPEC_URL ?? 'http://localhost:5000/openapi/v1.json';
const generatorImage = process.env.OPENAPI_GENERATOR_IMAGE ?? 'openapitools/openapi-generator-cli:v7.14.0';
const generatedDir = resolve(frontendDir, 'src/app/core/api/generated');

const run = (command, args, options = {}) => {
  const result = spawnSync(command, args, {
    cwd: frontendDir,
    stdio: 'inherit',
    shell: false,
    ...options
  });

  if (result.error) {
    throw result.error;
  }

  if (result.status !== 0) {
    process.exit(result.status ?? 1);
  }
};

const ensureDockerAvailable = () => {
  const result = spawnSync('docker', ['--version'], {
    cwd: frontendDir,
    stdio: 'ignore',
    shell: false
  });

  if (result.error || result.status !== 0) {
    console.error('ERROR: Docker is required to generate the OpenAPI client.');
    console.error('Install Docker Desktop/Engine and ensure the `docker` command is available in PATH.');
    process.exit(1);
  }
};

ensureDockerAvailable();

if (existsSync(generatedDir)) {
  rmSync(generatedDir, { recursive: true, force: true });
}
mkdirSync(generatedDir, { recursive: true });

const dockerArgs = [
  'run',
  '--rm',
  '--volume',
  `${frontendDir}:/local`
];

if (process.platform !== 'win32') {
  dockerArgs.push('--user', `${process.getuid()}:${process.getgid()}`);
}

dockerArgs.push(
  generatorImage,
  'generate',
  '--generator-name',
  'typescript-angular',
  '--input-spec',
  openApiSpecUrl,
  '--config',
  '/local/openapi/openapi-generator.config.json',
  '--output',
  '/local/src/app/core/api/generated'
);

run('docker', dockerArgs);
