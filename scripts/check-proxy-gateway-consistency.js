#!/usr/bin/env node
'use strict';

const fs = require('fs');
const path = require('path');

const repoRoot = path.resolve(__dirname, '..');
const defaultOcelotPath = path.join(repoRoot, 'backend', 'Gateway', 'ocelot.json');
const defaultProxyDir = path.join(repoRoot, 'frontend', 'src', 'app', 'shared', 'proxy', 'services');

function parseArgs(argv) {
  const options = {
    ocelot: defaultOcelotPath,
    proxyDir: defaultProxyDir
  };

  for (let index = 0; index < argv.length; index += 1) {
    const arg = argv[index];
    if (arg === '--ocelot') {
      const value = argv[index + 1];
      if (!value) {
        throw new Error('Missing value for --ocelot.');
      }

      options.ocelot = path.resolve(process.cwd(), value);
      index += 1;
      continue;
    }

    if (arg === '--proxy-dir') {
      const value = argv[index + 1];
      if (!value) {
        throw new Error('Missing value for --proxy-dir.');
      }

      options.proxyDir = path.resolve(process.cwd(), value);
      index += 1;
      continue;
    }

    if (arg === '--help' || arg === '-h') {
      options.help = true;
      continue;
    }

    throw new Error(`Unknown argument: ${arg}`);
  }

  return options;
}

function escapeRegExp(value) {
  return value.replace(/[.*+?^${}()|[\]\\]/g, '\\$&');
}

function toRouteRegex(upstreamPathTemplate) {
  const escapedTemplate = escapeRegExp(upstreamPathTemplate);

  const pattern = escapedTemplate
    .replace(/\\\{everything\\\}/gi, '.*')
    .replace(/\\\{[^{}]+\\\}/g, '[^/]+');

  return new RegExp(`^${pattern}$`, 'i');
}

function normalizeMethods(upstreamHttpMethod) {
  if (!Array.isArray(upstreamHttpMethod) || upstreamHttpMethod.length === 0) {
    return null;
  }

  return new Set(upstreamHttpMethod.map(method => String(method).toUpperCase()));
}

function readJson(filePath, description) {
  let rawContent;
  try {
    rawContent = fs.readFileSync(filePath, 'utf8');
  } catch (error) {
    throw new Error(`Unable to read ${description} at '${filePath}': ${error.message}`);
  }

  try {
    return JSON.parse(rawContent);
  } catch (error) {
    throw new Error(`Invalid JSON in ${description} at '${filePath}': ${error.message}`);
  }
}

function loadOcelotRoutes(ocelotPath) {
  const document = readJson(ocelotPath, 'Ocelot route configuration');
  if (!Array.isArray(document.Routes)) {
    throw new Error(`Ocelot route configuration '${ocelotPath}' does not contain a Routes array.`);
  }

  return document.Routes
    .filter(route => typeof route.UpstreamPathTemplate === 'string' && route.UpstreamPathTemplate.length > 0)
    .map(route => ({
      template: route.UpstreamPathTemplate,
      regex: toRouteRegex(route.UpstreamPathTemplate),
      methods: normalizeMethods(route.UpstreamHttpMethod)
    }));
}

function extractProxyOperationsFromFile(filePath) {
  const content = fs.readFileSync(filePath, 'utf8');
  const operationRegex = /method:\s*'([A-Z]+)'[\s\S]*?url:\s*'([^']+)'/g;

  const operations = [];
  let match;
  while ((match = operationRegex.exec(content)) !== null) {
    operations.push({
      method: match[1].toUpperCase(),
      url: match[2],
      filePath
    });
  }

  return operations;
}

function loadProxyOperations(proxyDir) {
  if (!fs.existsSync(proxyDir)) {
    throw new Error(`Proxy services directory does not exist: '${proxyDir}'.`);
  }

  const files = fs.readdirSync(proxyDir)
    .filter(entry => entry.endsWith('.ts'))
    .map(entry => path.join(proxyDir, entry));

  if (files.length === 0) {
    throw new Error(`Proxy services directory '${proxyDir}' does not contain TypeScript service files.`);
  }

  return files.flatMap(extractProxyOperationsFromFile);
}

function findMatchingRoute(operation, routes) {
  return routes.find(route => {
    if (!route.regex.test(operation.url)) {
      return false;
    }

    if (!route.methods) {
      return true;
    }

    return route.methods.has(operation.method);
  });
}

function main() {
  const options = parseArgs(process.argv.slice(2));

  if (options.help) {
    console.log('Usage: node scripts/check-proxy-gateway-consistency.js [--ocelot <path>] [--proxy-dir <path>]');
    return;
  }

  const routes = loadOcelotRoutes(options.ocelot);
  if (routes.length === 0) {
    throw new Error(`No gateway routes with UpstreamPathTemplate were found in '${options.ocelot}'.`);
  }

  const operations = loadProxyOperations(options.proxyDir);
  if (operations.length === 0) {
    throw new Error(`No proxy operations were found in '${options.proxyDir}'.`);
  }

  const unmatchedOperations = operations.filter(operation => !findMatchingRoute(operation, routes));

  if (unmatchedOperations.length > 0) {
    const details = unmatchedOperations
      .map(operation => `- ${operation.method} ${operation.url} (${path.relative(repoRoot, operation.filePath)})`)
      .join('\n');

    throw new Error(
      `Generated proxy operations are not fully routable through gateway strategy defined in '${path.relative(repoRoot, options.ocelot)}'.\n` +
      'Add/adjust gateway routes or regenerate proxies from the matching backend contract.\n' +
      details
    );
  }

  console.log(
    `Proxy/Gateway consistency check passed: ${operations.length} generated operation(s) map to gateway upstream routes in ${path.relative(repoRoot, options.ocelot)}.`
  );
}

try {
  main();
} catch (error) {
  console.error(`Error: ${error.message}`);
  process.exit(1);
}
