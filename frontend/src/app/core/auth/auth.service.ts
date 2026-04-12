import { Injectable } from '@angular/core';
import { Router } from '@angular/router';

import { APP_ROLES, AppRole, AppStateStore, TokenClaims } from '../state/app-state.store';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private readonly tokenStorageKey = 'qphising_access_token';
  private readonly loginReturnUrlKey = 'qphising_login_return_url';
  private readonly keycloakAuthorizeUrlStorageKey = 'qphising_keycloak_authorize_url';
  private readonly keycloakLogoutUrlStorageKey = 'qphising_keycloak_logout_url';
  private readonly keycloakClientIdStorageKey = 'qphising_keycloak_client_id';

  constructor(
    private readonly appStateStore: AppStateStore,
    private readonly router: Router
  ) {}

  async initialize(): Promise<void> {
    this.processAuthorizeCallback();
    const tokenClaims = this.resolveTokenClaims();
    if (!tokenClaims) {
      this.clearAccessToken();
    }

    this.appStateStore.hydrateSessionFromTokenClaims(tokenClaims);

    const callbackReturnUrl = this.resolveAndConsumeReturnUrl();
    if (callbackReturnUrl && this.router.url !== callbackReturnUrl) {
      await this.router.navigateByUrl(callbackReturnUrl);
    }
  }

  login(returnUrl?: string): void {
    if (typeof window === 'undefined') {
      return;
    }

    const normalizedReturnUrl = returnUrl?.trim() || this.router.url || '/';
    this.persistReturnUrl(normalizedReturnUrl);

    const authorizeUrl = new URL(this.resolveAuthorizeEndpoint());
    authorizeUrl.searchParams.set('client_id', this.resolveClientId());
    authorizeUrl.searchParams.set('redirect_uri', `${window.location.origin}/`);
    authorizeUrl.searchParams.set('response_type', 'token');
    authorizeUrl.searchParams.set('scope', 'openid profile email');
    authorizeUrl.searchParams.set('state', normalizedReturnUrl);

    window.location.assign(authorizeUrl.toString());
  }

  logout(): void {
    this.clearAccessToken();
    this.clearReturnUrl();
    this.appStateStore.resetSession();

    if (typeof window === 'undefined') {
      return;
    }

    const logoutUrl = new URL(this.resolveLogoutEndpoint());
    logoutUrl.searchParams.set('client_id', this.resolveClientId());
    logoutUrl.searchParams.set('post_logout_redirect_uri', `${window.location.origin}/`);

    window.location.assign(logoutUrl.toString());
  }

  isAuthenticated(): boolean {
    return this.appStateStore.isAuthenticated();
  }

  getUserRoles(): AppRole[] {
    const role = this.appStateStore.currentRole();
    return role ? [role] : [];
  }

  private processAuthorizeCallback(): void {
    if (typeof window === 'undefined') {
      return;
    }

    const rawHash = window.location.hash.startsWith('#') ? window.location.hash.slice(1) : window.location.hash;
    if (!rawHash) {
      return;
    }

    const hashParams = new URLSearchParams(rawHash);
    const accessToken = hashParams.get('access_token');
    if (!accessToken) {
      return;
    }

    localStorage.setItem(this.tokenStorageKey, accessToken);

    const stateReturnUrl = hashParams.get('state')?.trim();
    if (stateReturnUrl) {
      this.persistReturnUrl(stateReturnUrl);
    }

    window.history.replaceState({}, document.title, `${window.location.pathname}${window.location.search}`);
  }

  private resolveAndConsumeReturnUrl(): string | null {
    if (typeof sessionStorage === 'undefined') {
      return null;
    }

    const returnUrl = sessionStorage.getItem(this.loginReturnUrlKey)?.trim();
    sessionStorage.removeItem(this.loginReturnUrlKey);

    if (!returnUrl || !returnUrl.startsWith('/')) {
      return null;
    }

    return returnUrl;
  }

  private persistReturnUrl(returnUrl: string): void {
    if (typeof sessionStorage === 'undefined') {
      return;
    }

    sessionStorage.setItem(this.loginReturnUrlKey, returnUrl.startsWith('/') ? returnUrl : `/${returnUrl}`);
  }

  private clearReturnUrl(): void {
    if (typeof sessionStorage === 'undefined') {
      return;
    }

    sessionStorage.removeItem(this.loginReturnUrlKey);
  }

  private clearAccessToken(): void {
    if (typeof localStorage === 'undefined') {
      return;
    }

    localStorage.removeItem(this.tokenStorageKey);
  }

  private resolveTokenClaims(): TokenClaims | null {
    if (typeof localStorage === 'undefined') {
      return null;
    }

    const token = localStorage.getItem(this.tokenStorageKey);
    if (!token) {
      return null;
    }

    const segments = token.split('.');
    if (segments.length < 2) {
      return null;
    }

    try {
      const claimsJson = this.decodeBase64Url(segments[1]);
      const parsedClaims = JSON.parse(claimsJson) as TokenClaims;
      if (!this.isTokenTemporallyValid(parsedClaims)) {
        return null;
      }

      return parsedClaims;
    } catch {
      return null;
    }
  }

  private isTokenTemporallyValid(claims: TokenClaims): boolean {
    const nowEpochSeconds = Math.floor(Date.now() / 1000);

    if (typeof claims.nbf === 'number' && claims.nbf > nowEpochSeconds) {
      return false;
    }

    if (typeof claims.exp === 'number' && claims.exp <= nowEpochSeconds) {
      return false;
    }

    return true;
  }

  private resolveAuthorizeEndpoint(): string {
    if (typeof localStorage !== 'undefined') {
      const configuredAuthorizeUrl = localStorage.getItem(this.keycloakAuthorizeUrlStorageKey)?.trim();
      if (configuredAuthorizeUrl) {
        return configuredAuthorizeUrl;
      }
    }

    return 'http://localhost:8080/realms/qphising/protocol/openid-connect/auth';
  }

  private resolveLogoutEndpoint(): string {
    if (typeof localStorage !== 'undefined') {
      const configuredLogoutUrl = localStorage.getItem(this.keycloakLogoutUrlStorageKey)?.trim();
      if (configuredLogoutUrl) {
        return configuredLogoutUrl;
      }
    }

    return 'http://localhost:8080/realms/qphising/protocol/openid-connect/logout';
  }

  private resolveClientId(): string {
    if (typeof localStorage !== 'undefined') {
      const configuredClientId = localStorage.getItem(this.keycloakClientIdStorageKey)?.trim();
      if (configuredClientId) {
        return configuredClientId;
      }
    }

    return 'qphising-frontend';
  }

  private decodeBase64Url(payload: string): string {
    const base64Payload = payload.replace(/-/g, '+').replace(/_/g, '/');
    const padding = '='.repeat((4 - (base64Payload.length % 4)) % 4);
    return atob(base64Payload + padding);
  }
}
