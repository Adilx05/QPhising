#!/usr/bin/env node

const fs = require('fs');
const path = require('path');

function parseArgs(argv) {
  const args = argv.slice(2);
  const options = {
    swagger: null,
    ocelot: null
  };

  for (let index = 0; index < args.length; index += 1) {
    const arg = args[index];

    if ((arg === '--swagger' || arg === '-s') && args[index + 1]) {
      options.swagger = args[index + 1];
      index += 1;
      continue;
    }

    if ((arg === '--ocelot' || arg === '-o') && args[index + 1]) {
      options.ocelot = args[index + 1];
      index += 1;
      continue;
    }

    if (arg === '--help' || arg === '-h') {
      printUsage();
      process.exit(0);
    }

    console.error(`Error: unrecognized argument '${arg}'.`);
    printUsage();
    process.exit(1);
  }

  return options;
}

function printUsage() {
  console.log('Usage: node scripts/check-gateway-swagger-alignment.js [--swagger <path-or-url>] [--ocelot <path>]');
}

function loadJsonFromPath(filePath, description) {
  if (!fs.existsSync(filePath)) {
    console.error(`Error: ${description} file not found at '${filePath}'.`);
    process.exit(1);
  }

  const content = fs.readFileSync(filePath, 'utf8');
  if (!content || !content.trim()) {
    console.error(`Error: ${description} file '${filePath}' is empty.`);
    process.exit(1);
  }

  try {
    return JSON.parse(content);
  } catch {
    console.error(`Error: ${description} file '${filePath}' is not valid JSON.`);
    process.exit(1);
  }
}

function getTemplateMatcher(template) {
  if (template.endsWith('/{everything}')) {
    const prefix = template.slice(0, -('/{everything}'.length));
    return {
      kind: 'prefix',
      value: prefix
    };
  }

  return {
    kind: 'exact',
    value: template
  };
}

function matchesTemplate(pathValue, matcher) {
  if (matcher.kind === 'exact') {
    return pathValue === matcher.value;
  }

  return pathValue === matcher.value || pathValue.startsWith(`${matcher.value}/`);
}

function normalizeMethodSet(routeMethods) {
  if (!Array.isArray(routeMethods) || routeMethods.length === 0) {
    return null;
  }

  const methods = new Set();
  for (const method of routeMethods) {
    if (typeof method === 'string' && method.trim().length > 0) {
      methods.add(method.toLowerCase());
    }
  }

  return methods.size === 0 ? null : methods;
}

function getSwaggerOperations(swagger) {
  if (!swagger || typeof swagger !== 'object' || !swagger.paths || typeof swagger.paths !== 'object') {
    console.error("Error: Swagger document is missing 'paths'.");
    process.exit(1);
  }

  const operations = [];
  const supportedMethods = new Set(['get', 'post', 'put', 'patch', 'delete', 'head', 'options', 'trace']);

  for (const [pathValue, pathItem] of Object.entries(swagger.paths)) {
    if (!pathItem || typeof pathItem !== 'object') {
      continue;
    }

    for (const [method, operation] of Object.entries(pathItem)) {
      const normalizedMethod = method.toLowerCase();
      if (!supportedMethods.has(normalizedMethod) || !operation || typeof operation !== 'object') {
        continue;
      }

      operations.push({
        path: pathValue,
        method: normalizedMethod
      });
    }
  }

  return operations;
}

function main() {
  const repoRoot = path.resolve(__dirname, '..');
  const defaults = {
    swagger: path.join(repoRoot, 'frontend', 'openapi', 'proxy-validation.swagger.json'),
    ocelot: path.join(repoRoot, 'backend', 'Gateway', 'ocelot.json')
  };

  const args = parseArgs(process.argv);
  const swaggerPath = path.resolve(args.swagger || defaults.swagger);
  const ocelotPath = path.resolve(args.ocelot || defaults.ocelot);

  const swagger = loadJsonFromPath(swaggerPath, 'Swagger');
  const ocelotConfig = loadJsonFromPath(ocelotPath, 'Ocelot');

  if (!Array.isArray(ocelotConfig.Routes) || ocelotConfig.Routes.length === 0) {
    console.error("Error: Ocelot config is missing non-empty 'Routes'.");
    process.exit(1);
  }

  const swaggerOperations = getSwaggerOperations(swagger);
  const apiOperations = swaggerOperations.filter((operation) => operation.path.startsWith('/api/'));

  if (apiOperations.length === 0) {
    console.error('Error: Swagger document does not expose any /api operations to align with gateway routes.');
    process.exit(1);
  }

  const errors = [];
  const apiRoutes = ocelotConfig.Routes.filter((route) => {
    const upstream = typeof route.UpstreamPathTemplate === 'string' ? route.UpstreamPathTemplate : '';
    const downstream = typeof route.DownstreamPathTemplate === 'string' ? route.DownstreamPathTemplate : '';
    return upstream.startsWith('/api') || downstream.startsWith('/api');
  });

  if (apiRoutes.length === 0) {
    console.error('Error: Ocelot config does not define any /api routes to validate.');
    process.exit(1);
  }

  for (const route of apiRoutes) {
    const upstreamTemplate = route.UpstreamPathTemplate;
    const downstreamTemplate = route.DownstreamPathTemplate;

    if (typeof upstreamTemplate !== 'string' || typeof downstreamTemplate !== 'string') {
      errors.push('Gateway route is missing UpstreamPathTemplate or DownstreamPathTemplate.');
      continue;
    }

    const matcher = getTemplateMatcher(downstreamTemplate);
    const allowedMethods = normalizeMethodSet(route.UpstreamHttpMethod);

    const matchedOperations = apiOperations.filter((operation) => matchesTemplate(operation.path, matcher));
    if (matchedOperations.length === 0) {
      errors.push(`No Swagger /api operations match gateway route '${upstreamTemplate}' -> '${downstreamTemplate}'.`);
      continue;
    }

    if (allowedMethods === null) {
      continue;
    }

    const hasMethodMismatch = matchedOperations.some((operation) => !allowedMethods.has(operation.method));
    if (hasMethodMismatch) {
      const unsupportedOperations = matchedOperations
        .filter((operation) => !allowedMethods.has(operation.method))
        .map((operation) => `${operation.method.toUpperCase()} ${operation.path}`)
        .sort();

      errors.push(
        `Gateway route '${upstreamTemplate}' does not allow all matching Swagger methods. Unsupported: ${unsupportedOperations.join(', ')}`
      );
    }
  }

  if (errors.length > 0) {
    for (const error of errors) {
      console.error(`Error: ${error}`);
    }

    process.exit(1);
  }

  console.log(`Gateway-to-Swagger alignment check passed for ${apiRoutes.length} API route(s).`);
}

main();
