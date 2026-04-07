import { inject } from '@angular/core';
import { CanActivateFn, CanMatchFn, Route, Router, RouterStateSnapshot, UrlSegment, UrlTree } from '@angular/router';

import { APP_ROLES, AppRole } from '../state/app-state.store';
import { AuthService } from './auth.service';

function resolveRoleRequirements(route: Pick<Route, 'data'>): AppRole[] {
  const configuredRoles = route.data?.['roles'];
  return Array.isArray(configuredRoles) ? configuredRoles.filter((role): role is AppRole => APP_ROLES.includes(role as AppRole)) : [];
}

function resolveCurrentPathFromSegments(segments: UrlSegment[]): string {
  const path = segments.map((segment) => segment.path).filter((segment) => segment.length > 0).join('/');
  return path.length > 0 ? `/${path}` : '/';
}

function evaluateRouteAccess(route: Pick<Route, 'data'>, returnUrl: string): true | false | UrlTree {
  const authService = inject(AuthService);
  const router = inject(Router);

  if (!authService.isAuthenticated()) {
    authService.login(returnUrl);
    return false;
  }

  const requiredRoles = resolveRoleRequirements(route);
  if (requiredRoles.length === 0) {
    return true;
  }

  const userRoles = authService.getUserRoles();
  const hasRequiredRole = requiredRoles.some((requiredRole) => userRoles.includes(requiredRole));

  if (hasRequiredRole) {
    return true;
  }

  return router.createUrlTree(['/unauthorized'], {
    queryParams: { reason: 'role' }
  });
}

export const routeAccessGuard: CanActivateFn = (route, state: RouterStateSnapshot) =>
  evaluateRouteAccess(route, state.url);

export const routeMatchAccessGuard: CanMatchFn = (route: Route, segments: UrlSegment[]) =>
  evaluateRouteAccess(route, resolveCurrentPathFromSegments(segments));
