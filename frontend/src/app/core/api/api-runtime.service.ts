import { Injectable } from '@angular/core';

import { OpenAPI } from './generated';

@Injectable({
  providedIn: 'root'
})
export class ApiRuntimeService {
  private readonly gatewayUrlStorageKey = 'qphising_gateway_base_url';
  private readonly tokenStorageKey = 'qphising_access_token';

  configure(): void {
    OpenAPI.BASE = this.resolveGatewayBaseUrl();
    OpenAPI.TOKEN = () => Promise.resolve(this.resolveAccessToken());
  }

  private resolveGatewayBaseUrl(): string {
    if (typeof localStorage !== 'undefined') {
      const configured = localStorage.getItem(this.gatewayUrlStorageKey);
      if (configured && configured.trim().length > 0) {
        return configured.trim();
      }
    }

    return 'http://localhost:5001';
  }

  private resolveAccessToken(): string {
    if (typeof localStorage === 'undefined') {
      return '';
    }

    return localStorage.getItem(this.tokenStorageKey) ?? '';
  }
}
