import { inject } from '@angular/core';
import { Router, type ActivatedRouteSnapshot, type CanActivateFn, type CanMatchFn, type Route, type RouterStateSnapshot, type UrlSegment, type UrlTree } from '@angular/router';
import { AuthSessionService, type IdentityRole } from '../auth/auth-session';
import { OidcAuthService } from '../auth/oidc-auth.service';

const resolveRequiredRole = (source: ActivatedRouteSnapshot | Route): IdentityRole | null => {
  const role = source.data?.['requiredRole'];

  if (role === 'Admin' || role === 'Operator' || role === 'Viewer') {
    return role;
  }

  return null;
};

const resolveUnauthorizedRedirect = (router: Router, targetUrl: string): UrlTree =>
  router.createUrlTree(['/auth/unauthorized'], {
    queryParams: {
      returnUrl: targetUrl,
      reason: 'insufficient-role'
    }
  });

const resolveAuthentication = (
  router: Router,
  oidcAuthService: OidcAuthService,
  authSessionService: AuthSessionService,
  targetUrl: string
): boolean => {
  const session = authSessionService.getAuthSession();

  if (session.isAuthenticated) {
    return true;
  }

  void oidcAuthService.login(targetUrl).catch(() => {
    void router.navigate(['/auth/unauthorized'], {
      queryParams: {
        returnUrl: targetUrl,
        reason: 'login-failed'
      }
    });
  });

  return false;
};

const resolveAuthorization = (
  router: Router,
  authSessionService: AuthSessionService,
  requiredRole: IdentityRole | null,
  targetUrl: string
): boolean | UrlTree => {
  if (requiredRole === null) {
    return true;
  }

  return authSessionService.hasRequiredRole(requiredRole) ? true : resolveUnauthorizedRedirect(router, targetUrl);
};

const authorizeNavigation = (
  router: Router,
  oidcAuthService: OidcAuthService,
  authSessionService: AuthSessionService,
  targetUrl: string,
  requiredRole: IdentityRole | null
): boolean | UrlTree => {
  const authenticationResult = resolveAuthentication(router, oidcAuthService, authSessionService, targetUrl);

  if (authenticationResult !== true) {
    return authenticationResult;
  }

  return resolveAuthorization(router, authSessionService, requiredRole, targetUrl);
};

export const authenticationCanActivateGuard: CanActivateFn = (
  route: ActivatedRouteSnapshot,
  state: RouterStateSnapshot
) => {
  const router = inject(Router);
  const oidcAuthService = inject(OidcAuthService);
  const authSessionService = inject(AuthSessionService);

  return authorizeNavigation(router, oidcAuthService, authSessionService, state.url, resolveRequiredRole(route));
};

export const authenticationCanMatchGuard: CanMatchFn = (
  route: Route,
  segments: UrlSegment[]
) => {
  const router = inject(Router);
  const oidcAuthService = inject(OidcAuthService);
  const authSessionService = inject(AuthSessionService);
  const url = `/${segments.map((segment) => segment.path).join('/')}`;

  return authorizeNavigation(router, oidcAuthService, authSessionService, url, resolveRequiredRole(route));
};
