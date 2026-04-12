import { Injectable } from '@angular/core';

import { OpenAPI } from '../api/generated';
import { AuthService } from '../auth/auth.service';

interface SetupStatusApiResponse {
  isCompleted: boolean;
  completedAtUtc: string | null;
}

@Injectable({
  providedIn: 'root'
})
export class SetupStatusService {
  private readonly tokenStorageKey = 'qphising_access_token';

  constructor(private readonly authService: AuthService) {}

  async isSetupCompleted(): Promise<boolean> {
    if (!this.authService.isAuthenticated()) {
      return true;
    }

    const response = await fetch(`${OpenAPI.BASE}/api/setup/status`, {
      method: 'GET',
      headers: {
        Authorization: `Bearer ${this.resolveAccessToken()}`
      }
    });

    if (!response.ok) {
      return true;
    }

    const payload = (await response.json()) as SetupStatusApiResponse;
    return payload.isCompleted;
  }

  private resolveAccessToken(): string {
    if (typeof localStorage === 'undefined') {
      return '';
    }

    return localStorage.getItem(this.tokenStorageKey) ?? '';
  }
}
