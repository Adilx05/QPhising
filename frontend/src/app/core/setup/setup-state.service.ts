import { Injectable, signal } from '@angular/core';

import { OpenAPI } from '../api/generated';
import { AuthService } from '../auth/auth.service';

interface SetupStatusApiResponse {
  isCompleted: boolean;
  completedAtUtc: string | null;
}

export interface SetupStateSnapshot {
  isLoading: boolean;
  isCompleted: boolean;
  completedAtUtc: string | null;
  lastCheckedAtUtc: string | null;
  errorMessage: string | null;
}

@Injectable({
  providedIn: 'root'
})
export class SetupStateService {
  private readonly tokenStorageKey = 'qphising_access_token';

  readonly state = signal<SetupStateSnapshot>({
    isLoading: false,
    isCompleted: false,
    completedAtUtc: null,
    lastCheckedAtUtc: null,
    errorMessage: null
  });

  constructor(private readonly authService: AuthService) {}

  async refreshStatus(): Promise<SetupStateSnapshot> {
    this.state.update((current) => ({ ...current, isLoading: true, errorMessage: null }));

    try {
      if (!this.authService.isAuthenticated()) {
        const anonymousState: SetupStateSnapshot = {
          isLoading: false,
          isCompleted: true,
          completedAtUtc: null,
          lastCheckedAtUtc: new Date().toISOString(),
          errorMessage: null
        };

        this.state.set(anonymousState);
        return anonymousState;
      }

      const response = await fetch(`${OpenAPI.BASE}/api/setup/status`, {
        method: 'GET',
        headers: {
          Authorization: `Bearer ${this.resolveAccessToken()}`
        }
      });

      if (!response.ok) {
        throw new Error(`Setup status request failed with HTTP ${response.status}`);
      }

      const payload = (await response.json()) as SetupStatusApiResponse;
      const nextState: SetupStateSnapshot = {
        isLoading: false,
        isCompleted: payload.isCompleted,
        completedAtUtc: payload.completedAtUtc,
        lastCheckedAtUtc: new Date().toISOString(),
        errorMessage: null
      };

      this.state.set(nextState);
      return nextState;
    } catch (error) {
      const failedState: SetupStateSnapshot = {
        ...this.state(),
        isLoading: false,
        isCompleted: false,
        lastCheckedAtUtc: new Date().toISOString(),
        errorMessage: error instanceof Error ? error.message : 'Unable to load setup status.'
      };

      this.state.set(failedState);
      return failedState;
    }
  }

  async isSetupCompleted(): Promise<boolean> {
    const status = await this.refreshStatus();
    return status.isCompleted;
  }

  private resolveAccessToken(): string {
    if (typeof localStorage === 'undefined') {
      return '';
    }

    return localStorage.getItem(this.tokenStorageKey) ?? '';
  }
}
