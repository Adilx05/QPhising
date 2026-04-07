#!/usr/bin/env node
import { existsSync, mkdirSync, rmSync, writeFileSync } from 'node:fs';
import { dirname, resolve } from 'node:path';
import { fileURLToPath } from 'node:url';
import { generate } from 'openapi-typescript-codegen';

const scriptDir = dirname(fileURLToPath(import.meta.url));
const frontendDir = resolve(scriptDir, '..');

const openApiSpecUrl =
  process.env.OPENAPI_SPEC_URL ??
  'http://localhost:5000/openapi/v1.json';

const generatedDir = resolve(frontendDir, 'src/app/core/api/generated');
const tempSpecPath = resolve(frontendDir, 'openapi-temp.json');

console.log('🚀 OpenAPI client generation started...');
console.log('📡 Fetching spec from:', openApiSpecUrl);

const res = await fetch(openApiSpecUrl);
if (!res.ok) {
  console.error('❌ Failed to fetch OpenAPI spec:', res.status);
  process.exit(1);
}

const json = await res.text();
writeFileSync(tempSpecPath, json);

console.log('✅ Spec downloaded');

if (existsSync(generatedDir)) {
  console.log('🧹 Cleaning old generated files...');
  rmSync(generatedDir, { recursive: true, force: true });
}

mkdirSync(generatedDir, { recursive: true });

await generate({
  input: tempSpecPath,
  output: generatedDir,
  httpClient: 'fetch',
  useOptions: true,
  useUnionTypes: true
});

console.log('✅ API client generated successfully!');