import { Injectable } from '@angular/core';

export interface OidcSession {
  accessToken: string;
  expiresAtEpochSeconds: number;
  tokenType: string;
  scope: string;
}

interface OidcDiscoveryDocument {
  authorization_endpoint?: string;
  token_endpoint?: string;
  end_session_endpoint?: string;
}

interface OidcRuntimeConfig {
  authority: string;
  realm: string;
  clientId: string;
  postLogoutRedirectUri: string;
  redirectUri: string;
  requestedScope: string;
}

interface OidcStoredAuthorizationState {
  codeVerifier: string;
  returnUrl: string;
}

const oidcStorageKeys = {
  session: 'qphising.oidc.session',
  authorizationState: 'qphising.oidc.authorization.state'
} as const;

const encoder = new TextEncoder();
const utf8Decoder = new TextDecoder('utf-8', { fatal: false });

const normalizeBase64Url = (bytes: Uint8Array): string => {
  let binary = '';

  for (const byte of bytes) {
    binary += String.fromCharCode(byte);
  }

  return globalThis
    .btoa(binary)
    .replace(/\+/g, '-')
    .replace(/\//g, '_')
    .replace(/=+$/g, '');
};

const encodeQuery = (value: string): string => encodeURIComponent(value);

const resolveAbsolutePath = (path: string): string => {
  const origin = globalThis.location?.origin ?? '';

  return `${origin}${path}`;
};

const resolveRuntimeConfig = (): OidcRuntimeConfig => {
  const runtime = globalThis as {
    __QPHISING_AUTHORITY__?: unknown;
    __QPHISING_REALM__?: unknown;
    __QPHISING_CLIENT_ID__?: unknown;
    __QPHISING_POST_LOGOUT_REDIRECT_URI__?: unknown;
    __QPHISING_AUTH_REDIRECT_URI__?: unknown;
    __QPHISING_AUTH_SCOPE__?: unknown;
  };

  const authority = typeof runtime.__QPHISING_AUTHORITY__ === 'string' && runtime.__QPHISING_AUTHORITY__.trim().length > 0
    ? runtime.__QPHISING_AUTHORITY__.trim()
    : 'http://localhost:6060';

  const realm = typeof runtime.__QPHISING_REALM__ === 'string' && runtime.__QPHISING_REALM__.trim().length > 0
    ? runtime.__QPHISING_REALM__.trim()
    : 'QPhising';

  const clientId = typeof runtime.__QPHISING_CLIENT_ID__ === 'string' && runtime.__QPHISING_CLIENT_ID__.trim().length > 0
    ? runtime.__QPHISING_CLIENT_ID__.trim()
    : 'qphising';

  const postLogoutRedirectUri =
    typeof runtime.__QPHISING_POST_LOGOUT_REDIRECT_URI__ === 'string' && runtime.__QPHISING_POST_LOGOUT_REDIRECT_URI__.trim().length > 0
      ? runtime.__QPHISING_POST_LOGOUT_REDIRECT_URI__.trim()
      : resolveAbsolutePath('/auth/unauthorized');

  const redirectUri =
    typeof runtime.__QPHISING_AUTH_REDIRECT_URI__ === 'string' && runtime.__QPHISING_AUTH_REDIRECT_URI__.trim().length > 0
      ? runtime.__QPHISING_AUTH_REDIRECT_URI__.trim()
      : resolveAbsolutePath('/auth/callback');

  const requestedScope = typeof runtime.__QPHISING_AUTH_SCOPE__ === 'string' && runtime.__QPHISING_AUTH_SCOPE__.trim().length > 0
    ? runtime.__QPHISING_AUTH_SCOPE__.trim()
    : 'openid profile email';

  return {
    authority,
    realm,
    clientId,
    postLogoutRedirectUri,
    redirectUri,
    requestedScope
  };
};

const decodeJwtPayload = (token: string): Record<string, unknown> | null => {
  const segments = token.split('.');

  if (segments.length < 2) {
    return null;
  }

  try {
    const payload = segments[1]
      .replace(/-/g, '+')
      .replace(/_/g, '/');
    const paddedPayload = payload.padEnd(Math.ceil(payload.length / 4) * 4, '=');
    const decodedBinary = globalThis.atob(paddedPayload);
    const decodedBytes = Uint8Array.from(decodedBinary, (character) => character.charCodeAt(0));
    const decodedJson = utf8Decoder.decode(decodedBytes);

    return JSON.parse(decodedJson) as Record<string, unknown>;
  } catch {
    return null;
  }
};

const readJsonFromStorage = <T>(storage: Storage, key: string): T | null => {
  try {
    const raw = storage.getItem(key);

    if (raw === null) {
      return null;
    }

    return JSON.parse(raw) as T;
  } catch {
    return null;
  }
};

@Injectable({ providedIn: 'root' })
export class OidcAuthService {
  private readonly runtimeConfig = resolveRuntimeConfig();
  private discoveryDocumentPromise: Promise<OidcDiscoveryDocument> | null = null;

  public getSession(): OidcSession | null {
    const session = readJsonFromStorage<OidcSession>(globalThis.sessionStorage, oidcStorageKeys.session);

    if (session === null || typeof session.accessToken !== 'string' || session.accessToken.length === 0) {
      return null;
    }

    if (session.expiresAtEpochSeconds <= Math.floor(Date.now() / 1000)) {
      this.clearSession();
      return null;
    }

    return session;
  }

  public async login(returnUrl = '/dashboard'): Promise<void> {
    const discovery = await this.resolveDiscoveryDocument();

    if (!discovery.authorization_endpoint) {
      throw new Error('OIDC authorization endpoint is not available.');
    }

    const state = this.generateRandomString(32);
    const codeVerifier = this.generateRandomString(64);
    const codeChallenge = await this.createCodeChallenge(codeVerifier);

    const payload: OidcStoredAuthorizationState = {
      codeVerifier,
      returnUrl
    };

    globalThis.sessionStorage.setItem(oidcStorageKeys.authorizationState, JSON.stringify({
      state,
      payload
    }));

    const authorizeUrl = `${discovery.authorization_endpoint}?response_type=code&client_id=${encodeQuery(this.runtimeConfig.clientId)}&redirect_uri=${encodeQuery(this.runtimeConfig.redirectUri)}&scope=${encodeQuery(this.runtimeConfig.requestedScope)}&state=${encodeQuery(state)}&code_challenge=${encodeQuery(codeChallenge)}&code_challenge_method=S256`;

    globalThis.location.assign(authorizeUrl);
  }

  public async handleCallback(callbackUrl: URL): Promise<string> {
    const code = callbackUrl.searchParams.get('code');
    const state = callbackUrl.searchParams.get('state');
    const error = callbackUrl.searchParams.get('error');

    if (error) {
      throw new Error(`OIDC provider returned error: ${error}`);
    }

    if (!code || !state) {
      throw new Error('OIDC callback did not include code/state query parameters.');
    }

    const persistedState = readJsonFromStorage<{ state?: string; payload?: OidcStoredAuthorizationState }>(
      globalThis.sessionStorage,
      oidcStorageKeys.authorizationState
    );

    if (!persistedState?.state || persistedState.state !== state || !persistedState.payload?.codeVerifier) {
      throw new Error('OIDC callback state mismatch.');
    }

    const discovery = await this.resolveDiscoveryDocument();

    if (!discovery.token_endpoint) {
      throw new Error('OIDC token endpoint is not available.');
    }

    const tokenResponse = await fetch(discovery.token_endpoint, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/x-www-form-urlencoded'
      },
      body: new URLSearchParams({
        grant_type: 'authorization_code',
        client_id: this.runtimeConfig.clientId,
        code,
        redirect_uri: this.runtimeConfig.redirectUri,
        code_verifier: persistedState.payload.codeVerifier
      })
    });

    if (!tokenResponse.ok) {
      throw new Error(`OIDC token exchange failed with status ${tokenResponse.status}.`);
    }

    const tokenPayload = (await tokenResponse.json()) as {
      access_token?: unknown;
      expires_in?: unknown;
      token_type?: unknown;
      scope?: unknown;
    };

    if (typeof tokenPayload.access_token !== 'string' || tokenPayload.access_token.length === 0) {
      throw new Error('OIDC token response did not include an access token.');
    }

    const expiresIn =
      typeof tokenPayload.expires_in === 'number' && tokenPayload.expires_in > 0
        ? tokenPayload.expires_in
        : 300;

    const session: OidcSession = {
      accessToken: tokenPayload.access_token,
      expiresAtEpochSeconds: Math.floor(Date.now() / 1000) + expiresIn,
      tokenType: typeof tokenPayload.token_type === 'string' ? tokenPayload.token_type : 'Bearer',
      scope: typeof tokenPayload.scope === 'string' ? tokenPayload.scope : this.runtimeConfig.requestedScope
    };

    globalThis.sessionStorage.setItem(oidcStorageKeys.session, JSON.stringify(session));
    globalThis.sessionStorage.removeItem(oidcStorageKeys.authorizationState);

    return persistedState.payload.returnUrl || '/dashboard';
  }

  public async logout(): Promise<void> {
    const discovery = await this.resolveDiscoveryDocument();
    const currentSession = this.getSession();

    this.clearSession();

    if (!discovery.end_session_endpoint) {
      globalThis.location.assign(this.runtimeConfig.postLogoutRedirectUri);
      return;
    }

    const logoutUrl = `${discovery.end_session_endpoint}?post_logout_redirect_uri=${encodeQuery(this.runtimeConfig.postLogoutRedirectUri)}${currentSession ? `&client_id=${encodeQuery(this.runtimeConfig.clientId)}` : ''}`;

    globalThis.location.assign(logoutUrl);
  }

  public getTokenClaims(): Record<string, unknown> {
    const session = this.getSession();

    if (!session) {
      return {};
    }

    return decodeJwtPayload(session.accessToken) ?? {};
  }

  private clearSession(): void {
    globalThis.sessionStorage.removeItem(oidcStorageKeys.session);
    globalThis.sessionStorage.removeItem(oidcStorageKeys.authorizationState);
  }

  private async createCodeChallenge(codeVerifier: string): Promise<string> {
    const digest = await globalThis.crypto.subtle.digest('SHA-256', encoder.encode(codeVerifier));

    return normalizeBase64Url(new Uint8Array(digest));
  }

  private generateRandomString(length: number): string {
    const bytes = new Uint8Array(length);
    globalThis.crypto.getRandomValues(bytes);

    return normalizeBase64Url(bytes);
  }

  private resolveDiscoveryDocument(): Promise<OidcDiscoveryDocument> {
    if (this.discoveryDocumentPromise === null) {
      const discoveryUrl = `${this.runtimeConfig.authority}/realms/${this.runtimeConfig.realm}/.well-known/openid-configuration`;

      this.discoveryDocumentPromise = fetch(discoveryUrl).then(async (response) => {
        if (!response.ok) {
          throw new Error(`OIDC discovery request failed with status ${response.status}.`);
        }

        return (await response.json()) as OidcDiscoveryDocument;
      });
    }

    return this.discoveryDocumentPromise;
  }
}
