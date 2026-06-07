import { TestBed } from '@angular/core/testing';
import { OidcAuthService, type OidcSession } from './oidc-auth.service';

const DISCOVERY_DOCUMENT = {
  authorization_endpoint: 'http://localhost:6060/realms/QPhising/protocol/openid-connect/auth',
  token_endpoint: 'http://localhost:6060/realms/QPhising/protocol/openid-connect/token',
  end_session_endpoint: 'http://localhost:6060/realms/QPhising/protocol/openid-connect/logout'
};

const SESSION_KEY = 'qphising.oidc.session';
const AUTH_STATE_KEY = 'qphising.oidc.authorization.state';

function createTestJwt(payload: Record<string, unknown>): string {
  const header = btoa(JSON.stringify({ alg: 'HS256', typ: 'JWT' }));
  const encodedPayload = btoa(JSON.stringify(payload));
  return `${header}.${encodedPayload}.test-signature`;
}

const runtime = (): Record<string, unknown> => globalThis as Record<string, unknown>;

function mockGetRandomValues(): void {
  spyOn(globalThis.crypto, 'getRandomValues').and.callFake(
    <T extends ArrayBufferView | null>(array: T): T => {
      if (array !== null) {
        for (let i = 0; i < (array as Uint8Array).length; i++) {
          (array as Uint8Array)[i] = i % 256;
        }
      }
      return array;
    }
  );
}

describe('OidcAuthService', () => {
  let service: OidcAuthService;

  beforeEach(() => {
    sessionStorage.clear();

    runtime()['__QPHISING_AUTHORITY__'] = 'http://localhost:6060';
    runtime()['__QPHISING_REALM__'] = 'QPhising';
    runtime()['__QPHISING_CLIENT_ID__'] = 'qphising';
    runtime()['__QPHISING_POST_LOGOUT_REDIRECT_URI__'] = '/auth/unauthorized';
    runtime()['__QPHISING_AUTH_REDIRECT_URI__'] = '/auth/callback';
    runtime()['__QPHISING_AUTH_SCOPE__'] = 'openid profile email';

    TestBed.configureTestingModule({
      providers: [OidcAuthService]
    });

    service = TestBed.inject(OidcAuthService);
  });

  afterEach(() => {
    delete runtime()['__QPHISING_AUTHORITY__'];
    delete runtime()['__QPHISING_REALM__'];
    delete runtime()['__QPHISING_CLIENT_ID__'];
    delete runtime()['__QPHISING_POST_LOGOUT_REDIRECT_URI__'];
    delete runtime()['__QPHISING_AUTH_REDIRECT_URI__'];
    delete runtime()['__QPHISING_AUTH_SCOPE__'];
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  describe('login', () => {
    it('should initiate PKCE flow and redirect to authorization endpoint', async () => {
      const fetchSpy = spyOn(globalThis, 'fetch').and.resolveTo(
        new Response(JSON.stringify(DISCOVERY_DOCUMENT), { status: 200 })
      );

      const navigateSpy = spyOn(service as unknown as { navigate: (url: string) => void }, 'navigate');
      mockGetRandomValues();

      spyOn(globalThis.crypto.subtle, 'digest').and.resolveTo(
        new Uint8Array(32).fill(7).buffer as ArrayBuffer
      );

      await service.login('/dashboard');

      expect(fetchSpy).toHaveBeenCalledWith(
        'http://localhost:6060/realms/QPhising/.well-known/openid-configuration'
      );

      const authStateRaw = sessionStorage.getItem(AUTH_STATE_KEY);
      expect(authStateRaw).not.toBeNull();
      const authState = JSON.parse(authStateRaw!);
      expect(authState.payload.returnUrl).toBe('/dashboard');
      expect(authState.payload.codeVerifier).toBeDefined();
      expect(typeof authState.payload.codeVerifier).toBe('string');
      expect(authState.state).toBeDefined();

      expect(navigateSpy).toHaveBeenCalledTimes(1);
      const redirectUrl = navigateSpy.calls.first().args[0] as string;
      expect(redirectUrl).toContain('response_type=code');
      expect(redirectUrl).toContain('client_id=qphising');
      expect(redirectUrl).toContain('redirect_uri=');
      expect(redirectUrl).toContain('scope=');
      expect(redirectUrl).toContain('state=');
      expect(redirectUrl).toContain('code_challenge=');
      expect(redirectUrl).toContain('code_challenge_method=S256');
    });

    it('should throw when authorization endpoint is not available', async () => {
      spyOn(globalThis, 'fetch').and.resolveTo(
        new Response(JSON.stringify({}), { status: 200 })
      );

      spyOn(service as unknown as { navigate: (url: string) => void }, 'navigate');

      await expectAsync(service.login('/dashboard')).toBeRejectedWithError(
        'OIDC authorization endpoint is not available.'
      );
    });
  });

  describe('handleCallback', () => {
    const returnUrl = '/dashboard';
    const codeVerifier = 'test-code-verifier-123456789012345678901234567890123456789012345678901234567890';

    beforeEach(() => {
      spyOn(service as unknown as { navigate: (url: string) => void }, 'navigate');
      mockGetRandomValues();

      const statePayload = {
        state: 'test-state-value',
        payload: {
          codeVerifier,
          returnUrl
        }
      };
      sessionStorage.setItem(AUTH_STATE_KEY, JSON.stringify(statePayload));
    });

    it('should exchange code for tokens and return returnUrl', async () => {
      const fetchSpy = spyOn(globalThis, 'fetch');
      fetchSpy.and.callFake((url: string | Request | URL) => {
        const urlStr = typeof url === 'string' ? url : url.toString();
        if (urlStr.includes('.well-known/openid-configuration')) {
          return Promise.resolve(
            new Response(JSON.stringify(DISCOVERY_DOCUMENT), { status: 200 })
          );
        }
        if (urlStr.includes('token')) {
          return Promise.resolve(
            new Response(JSON.stringify({
              access_token: createTestJwt({ sub: 'user', roles: ['Admin'] }),
              expires_in: 3600,
              refresh_token: 'new-refresh-token',
              refresh_expires_in: 1800,
              token_type: 'Bearer',
              scope: 'openid profile email'
            }), { status: 200 })
          );
        }
        return Promise.reject(new Error('unexpected URL'));
      });

      const callbackUrl = new URL('http://localhost:4200/auth/callback?code=test-code&state=test-state-value');

      const result = await service.handleCallback(callbackUrl);

      expect(result).toBe('/dashboard');

      const sessionRaw = sessionStorage.getItem(SESSION_KEY);
      expect(sessionRaw).not.toBeNull();
      const session: OidcSession = JSON.parse(sessionRaw!);
      expect(session.accessToken).toBeDefined();
      expect(session.tokenType).toBe('Bearer');
      expect(session.scope).toBe('openid profile email');
      expect(session.refreshToken).toBe('new-refresh-token');

      expect(sessionStorage.getItem(AUTH_STATE_KEY)).toBeNull();
    });

    it('should throw when callback has error parameter', async () => {
      const callbackUrl = new URL('http://localhost:4200/auth/callback?error=access_denied');

      await expectAsync(service.handleCallback(callbackUrl)).toBeRejectedWithError(
        'OIDC provider returned error: access_denied'
      );
    });

    it('should throw when callback has no code or state', async () => {
      const callbackUrl = new URL('http://localhost:4200/auth/callback');

      await expectAsync(service.handleCallback(callbackUrl)).toBeRejectedWithError(
        'OIDC callback did not include code/state query parameters.'
      );
    });

    it('should throw when state does not match', async () => {
      const callbackUrl = new URL('http://localhost:4200/auth/callback?code=test-code&state=wrong-state');

      await expectAsync(service.handleCallback(callbackUrl)).toBeRejectedWithError(
        'OIDC callback state mismatch.'
      );
    });

    it('should throw when stored state is missing', async () => {
      sessionStorage.removeItem(AUTH_STATE_KEY);
      const callbackUrl = new URL('http://localhost:4200/auth/callback?code=test-code&state=test-state-value');

      await expectAsync(service.handleCallback(callbackUrl)).toBeRejectedWithError(
        'OIDC callback state mismatch.'
      );
    });

    it('should throw when token endpoint is not available', async () => {
      spyOn(globalThis, 'fetch').and.resolveTo(
        new Response(JSON.stringify({}), { status: 200 })
      );

      const callbackUrl = new URL('http://localhost:4200/auth/callback?code=test-code&state=test-state-value');

      await expectAsync(service.handleCallback(callbackUrl)).toBeRejectedWithError(
        'OIDC token endpoint is not available.'
      );
    });

    it('should throw when token exchange fails', async () => {
      const fetchSpy = spyOn(globalThis, 'fetch');
      fetchSpy.and.callFake((url: string | Request | URL) => {
        const urlStr = typeof url === 'string' ? url : url.toString();
        if (urlStr.includes('.well-known/openid-configuration')) {
          return Promise.resolve(
            new Response(JSON.stringify(DISCOVERY_DOCUMENT), { status: 200 })
          );
        }
        return Promise.resolve(
          new Response('{}', { status: 400 })
        );
      });

      const callbackUrl = new URL('http://localhost:4200/auth/callback?code=test-code&state=test-state-value');

      await expectAsync(service.handleCallback(callbackUrl)).toBeRejectedWithError(
        'OIDC token exchange failed with status 400.'
      );
    });

    it('should throw when token response has no access_token', async () => {
      const fetchSpy = spyOn(globalThis, 'fetch');
      fetchSpy.and.callFake((url: string | Request | URL) => {
        const urlStr = typeof url === 'string' ? url : url.toString();
        if (urlStr.includes('.well-known/openid-configuration')) {
          return Promise.resolve(
            new Response(JSON.stringify(DISCOVERY_DOCUMENT), { status: 200 })
          );
        }
        return Promise.resolve(
          new Response(JSON.stringify({}), { status: 200 })
        );
      });

      const callbackUrl = new URL('http://localhost:4200/auth/callback?code=test-code&state=test-state-value');

      await expectAsync(service.handleCallback(callbackUrl)).toBeRejectedWithError(
        'OIDC token response did not include an access token.'
      );
    });
  });

  describe('getSession', () => {
    it('should return session when access token is valid and not expired', () => {
      const futureEpoch = Math.floor(Date.now() / 1000) + 3600;
      const session: OidcSession = {
        accessToken: createTestJwt({ sub: 'user' }),
        expiresAtEpochSeconds: futureEpoch,
        refreshToken: null,
        refreshExpiresAtEpochSeconds: 0,
        tokenType: 'Bearer',
        scope: 'openid'
      };
      sessionStorage.setItem(SESSION_KEY, JSON.stringify(session));

      const result = service.getSession();

      expect(result).not.toBeNull();
      expect(result!.accessToken).toBe(session.accessToken);
    });

    it('should return null when session is expired', () => {
      const pastEpoch = Math.floor(Date.now() / 1000) - 100;
      const session: OidcSession = {
        accessToken: createTestJwt({ sub: 'user' }),
        expiresAtEpochSeconds: pastEpoch,
        refreshToken: null,
        refreshExpiresAtEpochSeconds: 0,
        tokenType: 'Bearer',
        scope: 'openid'
      };
      sessionStorage.setItem(SESSION_KEY, JSON.stringify(session));

      const result = service.getSession();

      expect(result).toBeNull();
      expect(sessionStorage.getItem(SESSION_KEY)).toBeNull();
    });

    it('should return null when no session stored', () => {
      const result = service.getSession();
      expect(result).toBeNull();
    });

    it('should return null when access token is empty string', () => {
      const session: OidcSession = {
        accessToken: '',
        expiresAtEpochSeconds: Math.floor(Date.now() / 1000) + 3600,
        refreshToken: null,
        refreshExpiresAtEpochSeconds: 0,
        tokenType: 'Bearer',
        scope: 'openid'
      };
      sessionStorage.setItem(SESSION_KEY, JSON.stringify(session));

      const result = service.getSession();
      expect(result).toBeNull();
    });

    it('should return null when stored value is not valid JSON', () => {
      sessionStorage.setItem(SESSION_KEY, 'not-json');
      const result = service.getSession();
      expect(result).toBeNull();
    });
  });

  describe('logout', () => {
    it('should clear session and redirect to end_session_endpoint', async () => {
      const futureEpoch = Math.floor(Date.now() / 1000) + 3600;
      const session: OidcSession = {
        accessToken: createTestJwt({ sub: 'user' }),
        expiresAtEpochSeconds: futureEpoch,
        refreshToken: 'refresh',
        refreshExpiresAtEpochSeconds: futureEpoch,
        tokenType: 'Bearer',
        scope: 'openid'
      };
      sessionStorage.setItem(SESSION_KEY, JSON.stringify(session));

      spyOn(globalThis, 'fetch').and.resolveTo(
        new Response(JSON.stringify(DISCOVERY_DOCUMENT), { status: 200 })
      );

      const navigateSpy = spyOn(service as unknown as { navigate: (url: string) => void }, 'navigate');

      await service.logout();

      expect(sessionStorage.getItem(SESSION_KEY)).toBeNull();

      expect(navigateSpy).toHaveBeenCalledTimes(1);
      const logoutUrl = navigateSpy.calls.first().args[0] as string;
      expect(logoutUrl).toContain(DISCOVERY_DOCUMENT.end_session_endpoint);
      expect(logoutUrl).toContain('post_logout_redirect_uri=');
      expect(logoutUrl).toContain('client_id=');
    });

    it('should redirect to postLogoutRedirectUri when end_session_endpoint unavailable', async () => {
      spyOn(globalThis, 'fetch').and.resolveTo(
        new Response(JSON.stringify({}), { status: 200 })
      );

      const navigateSpy = spyOn(service as unknown as { navigate: (url: string) => void }, 'navigate');

      await service.logout();

      expect(navigateSpy).toHaveBeenCalledWith('/auth/unauthorized');
    });
  });

  describe('hasValidRefreshToken', () => {
    it('should return true when valid refresh token exists', () => {
      const futureEpoch = Math.floor(Date.now() / 1000) + 3600;
      const session: OidcSession = {
        accessToken: createTestJwt({ sub: 'user' }),
        expiresAtEpochSeconds: futureEpoch,
        refreshToken: 'valid-refresh-token',
        refreshExpiresAtEpochSeconds: futureEpoch,
        tokenType: 'Bearer',
        scope: 'openid'
      };
      sessionStorage.setItem(SESSION_KEY, JSON.stringify(session));

      expect(service.hasValidRefreshToken()).toBeTrue();
    });

    it('should return false when session is missing', () => {
      expect(service.hasValidRefreshToken()).toBeFalse();
    });

    it('should return false when refresh token is empty', () => {
      const session: OidcSession = {
        accessToken: createTestJwt({ sub: 'user' }),
        expiresAtEpochSeconds: Math.floor(Date.now() / 1000) + 3600,
        refreshToken: '',
        refreshExpiresAtEpochSeconds: 0,
        tokenType: 'Bearer',
        scope: 'openid'
      };
      sessionStorage.setItem(SESSION_KEY, JSON.stringify(session));

      expect(service.hasValidRefreshToken()).toBeFalse();
    });

    it('should return false when refresh token is null', () => {
      const session: OidcSession = {
        accessToken: createTestJwt({ sub: 'user' }),
        expiresAtEpochSeconds: Math.floor(Date.now() / 1000) + 3600,
        refreshToken: null,
        refreshExpiresAtEpochSeconds: 0,
        tokenType: 'Bearer',
        scope: 'openid'
      };
      sessionStorage.setItem(SESSION_KEY, JSON.stringify(session));

      expect(service.hasValidRefreshToken()).toBeFalse();
    });

    it('should return false when refresh token is expired', () => {
      const pastEpoch = Math.floor(Date.now() / 1000) - 100;
      const session: OidcSession = {
        accessToken: createTestJwt({ sub: 'user' }),
        expiresAtEpochSeconds: Math.floor(Date.now() / 1000) + 3600,
        refreshToken: 'expired-refresh-token',
        refreshExpiresAtEpochSeconds: pastEpoch,
        tokenType: 'Bearer',
        scope: 'openid'
      };
      sessionStorage.setItem(SESSION_KEY, JSON.stringify(session));

      expect(service.hasValidRefreshToken()).toBeFalse();
    });
  });

  describe('refreshSession', () => {
    it('should refresh token and update session', async () => {
      const futureEpoch = Math.floor(Date.now() / 1000) + 3600;
      const session: OidcSession = {
        accessToken: createTestJwt({ sub: 'user' }),
        expiresAtEpochSeconds: futureEpoch,
        refreshToken: 'current-refresh-token',
        refreshExpiresAtEpochSeconds: futureEpoch,
        tokenType: 'Bearer',
        scope: 'openid'
      };
      sessionStorage.setItem(SESSION_KEY, JSON.stringify(session));

      const newAccessToken = createTestJwt({ sub: 'user', roles: ['Admin'] });

      const fetchSpy = spyOn(globalThis, 'fetch');
      fetchSpy.and.callFake((url: string | Request | URL) => {
        const urlStr = typeof url === 'string' ? url : url.toString();
        if (urlStr.includes('.well-known/openid-configuration')) {
          return Promise.resolve(
            new Response(JSON.stringify(DISCOVERY_DOCUMENT), { status: 200 })
          );
        }
        if (urlStr.includes('token')) {
          return Promise.resolve(
            new Response(JSON.stringify({
              access_token: newAccessToken,
              expires_in: 3600,
              refresh_token: 'rotated-refresh-token',
              refresh_expires_in: 1800,
              token_type: 'Bearer',
              scope: 'openid profile email'
            }), { status: 200 })
          );
        }
        return Promise.reject(new Error('unexpected URL'));
      });

      const result = await service.refreshSession();

      expect(result).toBeTrue();

      const updatedSessionRaw = sessionStorage.getItem(SESSION_KEY);
      expect(updatedSessionRaw).not.toBeNull();
      const updatedSession: OidcSession = JSON.parse(updatedSessionRaw!);
      expect(updatedSession.accessToken).toBe(newAccessToken);
      expect(updatedSession.refreshToken).toBe('rotated-refresh-token');
    });

    it('should return false when no session exists', async () => {
      const result = await service.refreshSession();
      expect(result).toBeFalse();
    });

    it('should return false when refresh token is empty', async () => {
      const session: OidcSession = {
        accessToken: createTestJwt({ sub: 'user' }),
        expiresAtEpochSeconds: Math.floor(Date.now() / 1000) + 3600,
        refreshToken: '',
        refreshExpiresAtEpochSeconds: 0,
        tokenType: 'Bearer',
        scope: 'openid'
      };
      sessionStorage.setItem(SESSION_KEY, JSON.stringify(session));

      const result = await service.refreshSession();
      expect(result).toBeFalse();
    });

    it('should clear session and return false when refresh token is expired', async () => {
      const pastEpoch = Math.floor(Date.now() / 1000) - 100;
      const session: OidcSession = {
        accessToken: createTestJwt({ sub: 'user' }),
        expiresAtEpochSeconds: Math.floor(Date.now() / 1000) + 3600,
        refreshToken: 'expired-refresh-token',
        refreshExpiresAtEpochSeconds: pastEpoch,
        tokenType: 'Bearer',
        scope: 'openid'
      };
      sessionStorage.setItem(SESSION_KEY, JSON.stringify(session));

      const result = await service.refreshSession();

      expect(result).toBeFalse();
      expect(sessionStorage.getItem(SESSION_KEY)).toBeNull();
    });

    it('should return false when token endpoint is not available', async () => {
      const futureEpoch = Math.floor(Date.now() / 1000) + 3600;
      const session: OidcSession = {
        accessToken: createTestJwt({ sub: 'user' }),
        expiresAtEpochSeconds: futureEpoch,
        refreshToken: 'valid-refresh-token',
        refreshExpiresAtEpochSeconds: futureEpoch,
        tokenType: 'Bearer',
        scope: 'openid'
      };
      sessionStorage.setItem(SESSION_KEY, JSON.stringify(session));

      spyOn(globalThis, 'fetch').and.resolveTo(
        new Response(JSON.stringify({}), { status: 200 })
      );

      const result = await service.refreshSession();
      expect(result).toBeFalse();
    });

    it('should clear session and return false when token response is not ok', async () => {
      const futureEpoch = Math.floor(Date.now() / 1000) + 3600;
      const session: OidcSession = {
        accessToken: createTestJwt({ sub: 'user' }),
        expiresAtEpochSeconds: futureEpoch,
        refreshToken: 'valid-refresh-token',
        refreshExpiresAtEpochSeconds: futureEpoch,
        tokenType: 'Bearer',
        scope: 'openid'
      };
      sessionStorage.setItem(SESSION_KEY, JSON.stringify(session));

      const fetchSpy = spyOn(globalThis, 'fetch');
      fetchSpy.and.callFake((url: string | Request | URL) => {
        const urlStr = typeof url === 'string' ? url : url.toString();
        if (urlStr.includes('.well-known/openid-configuration')) {
          return Promise.resolve(
            new Response(JSON.stringify(DISCOVERY_DOCUMENT), { status: 200 })
          );
        }
        return Promise.resolve(
          new Response('{}', { status: 401 })
        );
      });

      const result = await service.refreshSession();

      expect(result).toBeFalse();
      expect(sessionStorage.getItem(SESSION_KEY)).toBeNull();
    });

    it('should clear session when token response has no access_token', async () => {
      const futureEpoch = Math.floor(Date.now() / 1000) + 3600;
      const session: OidcSession = {
        accessToken: createTestJwt({ sub: 'user' }),
        expiresAtEpochSeconds: futureEpoch,
        refreshToken: 'valid-refresh-token',
        refreshExpiresAtEpochSeconds: futureEpoch,
        tokenType: 'Bearer',
        scope: 'openid'
      };
      sessionStorage.setItem(SESSION_KEY, JSON.stringify(session));

      const fetchSpy = spyOn(globalThis, 'fetch');
      fetchSpy.and.callFake((url: string | Request | URL) => {
        const urlStr = typeof url === 'string' ? url : url.toString();
        if (urlStr.includes('.well-known/openid-configuration')) {
          return Promise.resolve(
            new Response(JSON.stringify(DISCOVERY_DOCUMENT), { status: 200 })
          );
        }
        return Promise.resolve(
          new Response(JSON.stringify({ something: 'else' }), { status: 200 })
        );
      });

      const result = await service.refreshSession();

      expect(result).toBeFalse();
      expect(sessionStorage.getItem(SESSION_KEY)).toBeNull();
    });
  });

  describe('getTokenClaims', () => {
    it('should decode JWT and return claims', () => {
      const claims = {
        sub: 'test-user',
        name: 'Test User',
        preferred_username: 'testuser',
        roles: ['Admin', 'Operator'],
        realm_access: { roles: ['Admin'] }
      };
      const futureEpoch = Math.floor(Date.now() / 1000) + 3600;
      const session: OidcSession = {
        accessToken: createTestJwt(claims),
        expiresAtEpochSeconds: futureEpoch,
        refreshToken: null,
        refreshExpiresAtEpochSeconds: 0,
        tokenType: 'Bearer',
        scope: 'openid'
      };
      sessionStorage.setItem(SESSION_KEY, JSON.stringify(session));

      const result = service.getTokenClaims();

      expect(result).toEqual(jasmine.objectContaining({
        sub: 'test-user',
        name: 'Test User',
        preferred_username: 'testuser'
      }));
    });

    it('should return empty object when no session', () => {
      const result = service.getTokenClaims();
      expect(result).toEqual({});
    });

    it('should return empty object when JWT is malformed', () => {
      const futureEpoch = Math.floor(Date.now() / 1000) + 3600;
      const session: OidcSession = {
        accessToken: 'not-a-valid-jwt',
        expiresAtEpochSeconds: futureEpoch,
        refreshToken: null,
        refreshExpiresAtEpochSeconds: 0,
        tokenType: 'Bearer',
        scope: 'openid'
      };
      sessionStorage.setItem(SESSION_KEY, JSON.stringify(session));

      const result = service.getTokenClaims();
      expect(result).toEqual({});
    });
  });
});
