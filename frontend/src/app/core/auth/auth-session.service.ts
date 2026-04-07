import { Injectable } from '@angular/core';

import { AppStateStore, TokenClaims } from '../state/app-state.store';

@Injectable({
  providedIn: 'root'
})
export class AuthSessionService {
  private readonly tokenStorageKey = 'qphising_access_token';

  constructor(private readonly appStateStore: AppStateStore) {}

  bootstrapSessionFromTokenClaims(): void {
    const claims = this.resolveTokenClaims();
    this.appStateStore.hydrateSessionFromTokenClaims(claims);
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
      return JSON.parse(claimsJson) as TokenClaims;
    } catch {
      return null;
    }
  }

  private decodeBase64Url(payload: string): string {
    const base64Payload = payload.replace(/-/g, '+').replace(/_/g, '/');
    const padding = '='.repeat((4 - (base64Payload.length % 4)) % 4);
    return atob(base64Payload + padding);
  }
}
