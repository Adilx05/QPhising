import { inject } from '@angular/core';
import { CanActivateFn, CanMatchFn, Route, Router, UrlSegment, UrlTree } from '@angular/router';

import { AppRole, AppStateStore } from '../state/app-state.store';

function resolveRoleRequirements(route: Pick<Route, 'data'>): AppRole[] {
  const configuredRoles = route.data?.['roles'];

  if (!Array.isArray(configuredRoles)) {
    return [];
  }

  return configuredRoles.filter((role): role is AppRole => role === 'Admin' || role === 'Operator' || role === 'Viewer');
}

function evaluateRouteAccess(route: Pick<Route, 'data'>): true | UrlTree {
  const appStateStore = inject(AppStateStore);
  const router = inject(Router);

  if (!appStateStore.isAuthenticated()) {
    return router.createUrlTree(['/unauthorized'], {
      queryParams: { reason: 'auth' }
    });
  }

  const requiredRoles = resolveRoleRequirements(route);

  if (requiredRoles.length === 0 || appStateStore.canAccessAnyRole(requiredRoles)) {
    return true;
  }

  return router.createUrlTree(['/unauthorized'], {
    queryParams: { reason: 'role' }
  });
}

export const routeAccessGuard: CanActivateFn = (route) => evaluateRouteAccess(route);

export const routeMatchAccessGuard: CanMatchFn = (route: Route, _segments: UrlSegment[]) => evaluateRouteAccess(route);
