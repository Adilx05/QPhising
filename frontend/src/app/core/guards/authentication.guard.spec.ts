import { TestBed, fakeAsync, tick } from '@angular/core/testing';
import { Router, type ActivatedRouteSnapshot, type Route, type RouterStateSnapshot, type UrlSegment, type UrlTree } from '@angular/router';
import { AuthSessionService, type AuthSession } from '../auth/auth-session';
import { OidcAuthService } from '../auth/oidc-auth.service';
import { authenticationCanActivateGuard, authenticationCanMatchGuard } from './authentication.guard';

function createMockRouter(): jasmine.SpyObj<Router> {
  const mock = jasmine.createSpyObj<Router>('Router', ['createUrlTree', 'navigate'], { url: '/test' });
  mock.createUrlTree.and.returnValue({} as UrlTree);
  return mock;
}

function createMockOidcAuthService(): jasmine.SpyObj<OidcAuthService> {
  const mock = jasmine.createSpyObj<OidcAuthService>('OidcAuthService', [
    'login',
    'getSession',
    'getTokenClaims',
    'refreshSession',
    'hasValidRefreshToken'
  ]);
  mock.login.and.resolveTo();
  return mock;
}

function createMockAuthSessionService(): jasmine.SpyObj<AuthSessionService> {
  const mock = jasmine.createSpyObj<AuthSessionService>('AuthSessionService', [
    'getAccessToken',
    'getAuthSession',
    'hasRequiredRole',
    'getUserProfile'
  ]);
  mock.getAuthSession.and.returnValue({
    accessToken: null,
    isAuthenticated: false,
    roles: new Set()
  } as AuthSession);
  mock.hasRequiredRole.and.returnValue(false);
  return mock;
}

describe('authenticationCanActivateGuard', () => {
  let mockRouter: jasmine.SpyObj<Router>;
  let mockOidcAuthService: jasmine.SpyObj<OidcAuthService>;
  let mockAuthSessionService: jasmine.SpyObj<AuthSessionService>;

  function createRouteSnapshot(data: Record<string, unknown> = {}): ActivatedRouteSnapshot {
    return { data } as unknown as ActivatedRouteSnapshot;
  }

  function createRouterStateSnapshot(url: string = '/test'): RouterStateSnapshot {
    return { url } as unknown as RouterStateSnapshot;
  }

  function setupAuthenticatedSession(roles: string[] = ['Viewer']): void {
    mockAuthSessionService.getAuthSession.and.returnValue({
      accessToken: 'test-token',
      isAuthenticated: true,
      roles: new Set(roles as any)
    } as AuthSession);
  }

  beforeEach(() => {
    mockRouter = createMockRouter();
    mockOidcAuthService = createMockOidcAuthService();
    mockAuthSessionService = createMockAuthSessionService();

    TestBed.configureTestingModule({
      providers: [
        { provide: Router, useValue: mockRouter },
        { provide: OidcAuthService, useValue: mockOidcAuthService },
        { provide: AuthSessionService, useValue: mockAuthSessionService }
      ]
    });
  });

  it('should return true when authenticated', () => {
    setupAuthenticatedSession();

    const route = createRouteSnapshot();
    const state = createRouterStateSnapshot();

    const result = TestBed.runInInjectionContext(() =>
      authenticationCanActivateGuard(route, state)
    );

    expect(result).toBeTrue();
  });

  it('should trigger login and return false when not authenticated', () => {
    const route = createRouteSnapshot();
    const state = createRouterStateSnapshot('/protected');

    const result = TestBed.runInInjectionContext(() =>
      authenticationCanActivateGuard(route, state)
    );

    expect(result).toBeFalse();
    expect(mockOidcAuthService.login).toHaveBeenCalledWith('/protected');
  });

  it('should navigate to unauthorized when login fails', fakeAsync(() => {
    mockOidcAuthService.login.and.rejectWith(new Error('login failed'));

    const route = createRouteSnapshot();
    const state = createRouterStateSnapshot('/protected');

    TestBed.runInInjectionContext(() =>
      authenticationCanActivateGuard(route, state)
    );

    tick();

    expect(mockRouter.navigate).toHaveBeenCalledWith(['/auth/unauthorized'], {
      queryParams: {
        returnUrl: '/protected',
        reason: 'login-failed'
      }
    });
  }));

  it('should check role requirement from route data when authenticated', () => {
    setupAuthenticatedSession(['Viewer']);
    mockAuthSessionService.hasRequiredRole.withArgs('Admin').and.returnValue(false);

    const route = createRouteSnapshot({ requiredRole: 'Admin' });
    const state = createRouterStateSnapshot('/admin');

    const result = TestBed.runInInjectionContext(() =>
      authenticationCanActivateGuard(route, state)
    );

    expect(result).not.toBeTrue();
    expect(mockRouter.createUrlTree).toHaveBeenCalledWith(['/auth/unauthorized'], {
      queryParams: {
        returnUrl: '/admin',
        reason: 'insufficient-role'
      }
    });
  });

  it('should pass authentication for routes with sufficient role', () => {
    setupAuthenticatedSession(['Admin']);
    mockAuthSessionService.hasRequiredRole.withArgs('Admin').and.returnValue(true);

    const route = createRouteSnapshot({ requiredRole: 'Admin' });
    const state = createRouterStateSnapshot('/admin');

    const result = TestBed.runInInjectionContext(() =>
      authenticationCanActivateGuard(route, state)
    );

    expect(result).toBeTrue();
  });

  it('should ignore unknown role values in route data', () => {
    setupAuthenticatedSession();

    const route = createRouteSnapshot({ requiredRole: 'UnknownRole' });
    const state = createRouterStateSnapshot('/test');

    const result = TestBed.runInInjectionContext(() =>
      authenticationCanActivateGuard(route, state)
    );

    expect(result).toBeTrue();
  });

  it('should handle missing role in route data as no restriction', () => {
    setupAuthenticatedSession();

    const route = createRouteSnapshot({});
    const state = createRouterStateSnapshot('/test');

    const result = TestBed.runInInjectionContext(() =>
      authenticationCanActivateGuard(route, state)
    );

    expect(result).toBeTrue();
  });
});

describe('authenticationCanMatchGuard', () => {
  let mockRouter: jasmine.SpyObj<Router>;
  let mockOidcAuthService: jasmine.SpyObj<OidcAuthService>;
  let mockAuthSessionService: jasmine.SpyObj<AuthSessionService>;

  function createRoute(data: Record<string, unknown> = {}): Route {
    return { data } as Route;
  }

  function createUrlSegments(path: string): UrlSegment[] {
    return path.split('/').filter(Boolean).map((p) => ({ path: p, parameterMap: new Map() } as unknown as UrlSegment));
  }

  function setupAuthenticatedSession(roles: string[] = ['Viewer']): void {
    mockAuthSessionService.getAuthSession.and.returnValue({
      accessToken: 'test-token',
      isAuthenticated: true,
      roles: new Set(roles as any)
    } as AuthSession);
  }

  beforeEach(() => {
    mockRouter = createMockRouter();
    mockOidcAuthService = createMockOidcAuthService();
    mockAuthSessionService = createMockAuthSessionService();

    TestBed.configureTestingModule({
      providers: [
        { provide: Router, useValue: mockRouter },
        { provide: OidcAuthService, useValue: mockOidcAuthService },
        { provide: AuthSessionService, useValue: mockAuthSessionService }
      ]
    });
  });

  it('should return true when authenticated with correct role', () => {
    setupAuthenticatedSession(['Admin']);
    mockAuthSessionService.hasRequiredRole.withArgs('Admin').and.returnValue(true);

    const route = createRoute({ requiredRole: 'Admin' });
    const segments = createUrlSegments('/admin/dashboard');

    const result = TestBed.runInInjectionContext(() =>
      authenticationCanMatchGuard(route, segments)
    );

    expect(result).toBeTrue();
  });

  it('should return UrlTree redirect when role insufficient', () => {
    setupAuthenticatedSession(['Viewer']);
    mockAuthSessionService.hasRequiredRole.withArgs('Admin').and.returnValue(false);

    const route = createRoute({ requiredRole: 'Admin' });
    const segments = createUrlSegments('/admin/dashboard');

    const result = TestBed.runInInjectionContext(() =>
      authenticationCanMatchGuard(route, segments)
    );

    expect(result).not.toBeTrue();
    expect(mockRouter.createUrlTree).toHaveBeenCalledWith(['/auth/unauthorized'], {
      queryParams: {
        returnUrl: '/admin/dashboard',
        reason: 'insufficient-role'
      }
    });
  });

  it('should trigger login when not authenticated', () => {
    const route = createRoute({ requiredRole: 'Viewer' });
    const segments = createUrlSegments('/viewer/content');

    const result = TestBed.runInInjectionContext(() =>
      authenticationCanMatchGuard(route, segments)
    );

    expect(result).toBeFalse();
    expect(mockOidcAuthService.login).toHaveBeenCalledWith('/viewer/content');
  });

  it('should return true when authenticated and no role required', () => {
    setupAuthenticatedSession(['Viewer']);

    const route = createRoute({});
    const segments = createUrlSegments('/public');

    const result = TestBed.runInInjectionContext(() =>
      authenticationCanMatchGuard(route, segments)
    );

    expect(result).toBeTrue();
  });

  it('should construct url from segments correctly', () => {
    const route = createRoute({});
    const segments = createUrlSegments('/admin/settings/profile');

    TestBed.runInInjectionContext(() =>
      authenticationCanMatchGuard(route, segments)
    );

    expect(mockOidcAuthService.login).toHaveBeenCalledWith('/admin/settings/profile');
  });
});
