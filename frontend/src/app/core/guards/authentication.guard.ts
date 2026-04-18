import { inject } from '@angular/core';
import { Router, type ActivatedRouteSnapshot, type CanActivateFn, type CanMatchFn, type Route, type RouterStateSnapshot, type UrlSegment, type UrlTree } from '@angular/router';
import { getAuthSession, hasRequiredRole, type IdentityRole } from '../auth/auth-session';

const resolveRequiredRole = (source: ActivatedRouteSnapshot | Route): IdentityRole | null => {
  const role = source.data?.['requiredRole'];

  if (role === 'Admin' || role === 'Operator' || role === 'Viewer') {
    return role;
  }

  return null;
};

const resolveUnauthorizedRedirect = (router: Router, targetUrl: string): UrlTree =>
  router.createUrlTree(['/setup'], {
    queryParams: {
      returnUrl: targetUrl,
      reason: 'unauthorized'
    }
  });

const resolveAuthentication = (router: Router, targetUrl: string): boolean | UrlTree => {
  const session = getAuthSession();

  return session.isAuthenticated ? true : resolveUnauthorizedRedirect(router, targetUrl);
};

const resolveAuthorization = (router: Router, requiredRole: IdentityRole | null, targetUrl: string): boolean | UrlTree => {
  if (requiredRole === null) {
    return true;
  }

  return hasRequiredRole(requiredRole) ? true : resolveUnauthorizedRedirect(router, targetUrl);
};

const authorizeNavigation = (
  router: Router,
  targetUrl: string,
  requiredRole: IdentityRole | null
): boolean | UrlTree => {
  const authenticationResult = resolveAuthentication(router, targetUrl);

  if (authenticationResult !== true) {
    return authenticationResult;
  }

  return resolveAuthorization(router, requiredRole, targetUrl);
};

export const authenticationCanActivateGuard: CanActivateFn = (
  route: ActivatedRouteSnapshot,
  state: RouterStateSnapshot
) => {
  const router = inject(Router);

  return authorizeNavigation(router, state.url, resolveRequiredRole(route));
};

export const authenticationCanMatchGuard: CanMatchFn = (
  route: Route,
  segments: UrlSegment[]
) => {
  const router = inject(Router);
  const url = `/${segments.map((segment) => segment.path).join('/')}`;

  return authorizeNavigation(router, url, resolveRequiredRole(route));
};
