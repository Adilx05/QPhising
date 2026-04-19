#!/usr/bin/env node
'use strict';

const fs = require('node:fs');
const path = require('node:path');

const repoRoot = path.resolve(__dirname, '..');

function read(relativePath) {
  return fs.readFileSync(path.join(repoRoot, relativePath), 'utf8');
}

function assertIncludes(haystack, needle, message) {
  if (!haystack.includes(needle)) {
    throw new Error(message);
  }
}

function run() {
  const routes = read('frontend/src/app/app.routes.ts');
  const dashboardPage = read('frontend/src/app/features/dashboard/pages/dashboard-page.component.ts');
  const trackingPage = read('frontend/src/app/features/tracking/pages/tracking-dashboard-page.component.ts');

  assertIncludes(routes, "path: 'dashboard'", 'Missing dashboard route in app.routes.ts');
  assertIncludes(routes, "path: 'tracking'", 'Missing tracking route in app.routes.ts');
  assertIncludes(routes, 'setupCompletionCanActivateGuard', 'Setup guard is not enforced on protected routes.');

  assertIncludes(dashboardPage, 'Security Operations Dashboard', 'Dashboard smoke content not found.');

  assertIncludes(trackingPage, 'Tracking Pages Grid', 'Tracking grid section missing.');
  assertIncludes(trackingPage, 'Tracking Page Editor', 'Tracking editor section missing.');
  assertIncludes(trackingPage, 'Analytics Detail', 'Tracking analytics detail section missing.');
  assertIncludes(trackingPage, 'listTrackingPages', 'Tracking grid flow is not wired to generated proxy data-access.');
  assertIncludes(trackingPage, 'createTrackingPage', 'Tracking editor create flow is not wired.');
  assertIncludes(trackingPage, 'updateTrackingPage', 'Tracking editor update flow is not wired.');
  assertIncludes(trackingPage, 'getTrackingPageAnalytics', 'Tracking analytics flow is not wired.');

  console.log('Frontend UI smoke checks passed.');
}

try {
  run();
} catch (error) {
  console.error(`Frontend UI smoke checks failed: ${error.message}`);
  process.exit(1);
}
