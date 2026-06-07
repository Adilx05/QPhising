import { TestBed } from '@angular/core/testing';
import { AuthSessionService, type IdentityRole } from './auth-session';
import { OidcAuthService } from './oidc-auth.service';

function createTestJwt(payload: Record<string, unknown>): string {
  const header = btoa(JSON.stringify({ alg: 'HS256', typ: 'JWT' }));
  const encodedPayload = btoa(JSON.stringify(payload));
  return `${header}.${encodedPayload}.test-signature`;
}

describe('AuthSessionService', () => {
  let service: AuthSessionService;
  let mockOidcAuthService: jasmine.SpyObj<OidcAuthService>;

  beforeEach(() => {
    mockOidcAuthService = jasmine.createSpyObj<OidcAuthService>('OidcAuthService', [
      'getSession',
      'getTokenClaims',
      'refreshSession'
    ]);

    TestBed.configureTestingModule({
      providers: [
        AuthSessionService,
        { provide: OidcAuthService, useValue: mockOidcAuthService }
      ]
    });

    service = TestBed.inject(AuthSessionService);
  });

  describe('getAuthSession', () => {
    it('should return authenticated session when OIDC session exists', () => {
      mockOidcAuthService.getSession.and.returnValue({
        accessToken: createTestJwt({ sub: 'user' }),
        expiresAtEpochSeconds: Math.floor(Date.now() / 1000) + 3600,
        refreshToken: null,
        refreshExpiresAtEpochSeconds: 0,
        tokenType: 'Bearer',
        scope: 'openid'
      });
      mockOidcAuthService.getTokenClaims.and.returnValue({
        sub: 'user',
        roles: ['Admin']
      });

      const session = service.getAuthSession();

      expect(session.isAuthenticated).toBeTrue();
      expect(session.accessToken).toBeTruthy();
    });

    it('should return unauthenticated session when no OIDC session exists', () => {
      mockOidcAuthService.getSession.and.returnValue(null);

      const session = service.getAuthSession();

      expect(session.isAuthenticated).toBeFalse();
      expect(session.accessToken).toBeNull();
      expect(session.roles.size).toBe(0);
    });

    it('should extract roles from roles claim', () => {
      mockOidcAuthService.getSession.and.returnValue({
        accessToken: createTestJwt({ sub: 'user' }),
        expiresAtEpochSeconds: Math.floor(Date.now() / 1000) + 3600,
        refreshToken: null,
        refreshExpiresAtEpochSeconds: 0,
        tokenType: 'Bearer',
        scope: 'openid'
      });
      mockOidcAuthService.getTokenClaims.and.returnValue({
        roles: ['Admin', 'Operator']
      });

      const session = service.getAuthSession();

      expect(session.roles.has('Admin')).toBeTrue();
      expect(session.roles.has('Operator')).toBeTrue();
    });

    it('should extract roles from realm_access.roles', () => {
      mockOidcAuthService.getSession.and.returnValue({
        accessToken: createTestJwt({ sub: 'user' }),
        expiresAtEpochSeconds: Math.floor(Date.now() / 1000) + 3600,
        refreshToken: null,
        refreshExpiresAtEpochSeconds: 0,
        tokenType: 'Bearer',
        scope: 'openid'
      });
      mockOidcAuthService.getTokenClaims.and.returnValue({
        realm_access: { roles: ['Admin'] }
      });

      const session = service.getAuthSession();

      expect(session.roles.has('Admin')).toBeTrue();
    });

    it('should extract roles from resource_access', () => {
      mockOidcAuthService.getSession.and.returnValue({
        accessToken: createTestJwt({ sub: 'user' }),
        expiresAtEpochSeconds: Math.floor(Date.now() / 1000) + 3600,
        refreshToken: null,
        refreshExpiresAtEpochSeconds: 0,
        tokenType: 'Bearer',
        scope: 'openid'
      });
      mockOidcAuthService.getTokenClaims.and.returnValue({
        resource_access: {
          qphising: { roles: ['Viewer'] }
        }
      });

      const session = service.getAuthSession();

      expect(session.roles.has('Viewer')).toBeTrue();
    });

    it('should merge roles from multiple sources', () => {
      mockOidcAuthService.getSession.and.returnValue({
        accessToken: createTestJwt({ sub: 'user' }),
        expiresAtEpochSeconds: Math.floor(Date.now() / 1000) + 3600,
        refreshToken: null,
        refreshExpiresAtEpochSeconds: 0,
        tokenType: 'Bearer',
        scope: 'openid'
      });
      mockOidcAuthService.getTokenClaims.and.returnValue({
        role: 'Admin',
        roles: ['Admin', 'Operator'],
        realm_access: { roles: ['Viewer'] }
      });

      const session = service.getAuthSession();

      expect(session.roles.has('Admin')).toBeTrue();
      expect(session.roles.has('Operator')).toBeTrue();
      expect(session.roles.has('Viewer')).toBeTrue();
    });
  });

  describe('hasRequiredRole', () => {
    function setupRoles(roles: IdentityRole[]): void {
      mockOidcAuthService.getSession.and.returnValue({
        accessToken: createTestJwt({ sub: 'user' }),
        expiresAtEpochSeconds: Math.floor(Date.now() / 1000) + 3600,
        refreshToken: null,
        refreshExpiresAtEpochSeconds: 0,
        tokenType: 'Bearer',
        scope: 'openid'
      });
      mockOidcAuthService.getTokenClaims.and.returnValue({
        roles
      });
    }

    it('should return true when user has Admin role and required is Admin', () => {
      setupRoles(['Admin']);
      expect(service.hasRequiredRole('Admin')).toBeTrue();
    });

    it('should return true when user has Admin role and required is Viewer', () => {
      setupRoles(['Admin']);
      expect(service.hasRequiredRole('Viewer')).toBeTrue();
    });

    it('should return true when user has Admin role and required is Operator', () => {
      setupRoles(['Admin']);
      expect(service.hasRequiredRole('Operator')).toBeTrue();
    });

    it('should return true when user has Operator role and required is Viewer', () => {
      setupRoles(['Operator']);
      expect(service.hasRequiredRole('Viewer')).toBeTrue();
    });

    it('should return false when user has Viewer role and required is Admin', () => {
      setupRoles(['Viewer']);
      expect(service.hasRequiredRole('Admin')).toBeFalse();
    });

    it('should return false when user has Viewer role and required is Operator', () => {
      setupRoles(['Viewer']);
      expect(service.hasRequiredRole('Operator')).toBeFalse();
    });

    it('should return false when user has Operator role and required is Admin', () => {
      setupRoles(['Operator']);
      expect(service.hasRequiredRole('Admin')).toBeFalse();
    });

    it('should return false when user has no roles', () => {
      setupRoles([]);
      expect(service.hasRequiredRole('Viewer')).toBeFalse();
    });

    it('should return false when user is not authenticated', () => {
      mockOidcAuthService.getSession.and.returnValue(null);
      expect(service.hasRequiredRole('Viewer')).toBeFalse();
    });
  });

  describe('getUserProfile', () => {
    it('should return profile with given name and family name', () => {
      mockOidcAuthService.getSession.and.returnValue({
        accessToken: createTestJwt({ sub: 'user' }),
        expiresAtEpochSeconds: Math.floor(Date.now() / 1000) + 3600,
        refreshToken: null,
        refreshExpiresAtEpochSeconds: 0,
        tokenType: 'Bearer',
        scope: 'openid'
      });
      mockOidcAuthService.getTokenClaims.and.returnValue({
        given_name: 'John',
        family_name: 'Doe'
      });

      const profile = service.getUserProfile();

      expect(profile.fullName).toBe('John Doe');
    });

    it('should fall back to name claim when given/family name missing', () => {
      mockOidcAuthService.getSession.and.returnValue({
        accessToken: createTestJwt({ sub: 'user' }),
        expiresAtEpochSeconds: Math.floor(Date.now() / 1000) + 3600,
        refreshToken: null,
        refreshExpiresAtEpochSeconds: 0,
        tokenType: 'Bearer',
        scope: 'openid'
      });
      mockOidcAuthService.getTokenClaims.and.returnValue({
        name: 'John Doe'
      });

      const profile = service.getUserProfile();

      expect(profile.fullName).toBe('John Doe');
    });

    it('should fall back to preferred_username when name missing', () => {
      mockOidcAuthService.getSession.and.returnValue({
        accessToken: createTestJwt({ sub: 'user' }),
        expiresAtEpochSeconds: Math.floor(Date.now() / 1000) + 3600,
        refreshToken: null,
        refreshExpiresAtEpochSeconds: 0,
        tokenType: 'Bearer',
        scope: 'openid'
      });
      mockOidcAuthService.getTokenClaims.and.returnValue({
        preferred_username: 'jdoe'
      });

      const profile = service.getUserProfile();

      expect(profile.fullName).toBe('jdoe');
    });

    it('should use fallback name when no name claims exist', () => {
      mockOidcAuthService.getSession.and.returnValue({
        accessToken: createTestJwt({ sub: 'user' }),
        expiresAtEpochSeconds: Math.floor(Date.now() / 1000) + 3600,
        refreshToken: null,
        refreshExpiresAtEpochSeconds: 0,
        tokenType: 'Bearer',
        scope: 'openid'
      });
      mockOidcAuthService.getTokenClaims.and.returnValue({});

      const profile = service.getUserProfile();

      expect(profile.fullName).toBe('Authenticated User');
    });

    it('should return Guest User profile when not authenticated', () => {
      mockOidcAuthService.getSession.and.returnValue(null);

      const profile = service.getUserProfile();

      expect(profile.fullName).toBe('Guest User');
      expect(profile.primaryRole).toBeNull();
    });

    it('should set primary role to highest ranked role', () => {
      mockOidcAuthService.getSession.and.returnValue({
        accessToken: createTestJwt({ sub: 'user' }),
        expiresAtEpochSeconds: Math.floor(Date.now() / 1000) + 3600,
        refreshToken: null,
        refreshExpiresAtEpochSeconds: 0,
        tokenType: 'Bearer',
        scope: 'openid'
      });
      mockOidcAuthService.getTokenClaims.and.returnValue({
        roles: ['Viewer', 'Admin']
      });

      const profile = service.getUserProfile();

      expect(profile.primaryRole).toBe('Admin');
    });

    it('should set primary role to null when user has no roles', () => {
      mockOidcAuthService.getSession.and.returnValue({
        accessToken: createTestJwt({ sub: 'user' }),
        expiresAtEpochSeconds: Math.floor(Date.now() / 1000) + 3600,
        refreshToken: null,
        refreshExpiresAtEpochSeconds: 0,
        tokenType: 'Bearer',
        scope: 'openid'
      });
      mockOidcAuthService.getTokenClaims.and.returnValue({});

      const profile = service.getUserProfile();

      expect(profile.primaryRole).toBeNull();
    });
  });

  describe('getAccessToken', () => {
    it('should return access token when session is valid and not near expiry', () => {
      mockOidcAuthService.getSession.and.returnValue({
        accessToken: 'valid-access-token',
        expiresAtEpochSeconds: Math.floor(Date.now() / 1000) + 3600,
        refreshToken: null,
        refreshExpiresAtEpochSeconds: 0,
        tokenType: 'Bearer',
        scope: 'openid'
      });

      const token = service.getAccessToken();

      expect(token).toBe('valid-access-token');
      expect(mockOidcAuthService.refreshSession).not.toHaveBeenCalled();
    });

    it('should trigger refresh when token is near expiry', () => {
      mockOidcAuthService.getSession.and.callFake(() => ({
        accessToken: 'near-expiry-token',
        expiresAtEpochSeconds: Math.floor(Date.now() / 1000) + 30,
        refreshToken: 'refresh-token',
        refreshExpiresAtEpochSeconds: Math.floor(Date.now() / 1000) + 3600,
        tokenType: 'Bearer',
        scope: 'openid'
      }));
      mockOidcAuthService.refreshSession.and.resolveTo(true);

      const token = service.getAccessToken();

      expect(mockOidcAuthService.refreshSession).toHaveBeenCalled();
      expect(token).toBe('near-expiry-token');
    });

    it('should return null when no session exists', () => {
      mockOidcAuthService.getSession.and.returnValue(null);

      const token = service.getAccessToken();

      expect(token).toBeNull();
    });
  });
});
