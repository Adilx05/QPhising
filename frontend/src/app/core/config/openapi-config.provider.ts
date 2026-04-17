import { APP_INITIALIZER, EnvironmentProviders, makeEnvironmentProviders } from '@angular/core';
import { environment } from '../../../environments/environment';
import { OpenAPI } from '../../shared/proxy';

const resolveApiBaseUrl = (): string => {
  const runtimeConfig = (globalThis as { __QPHISING_API_BASE_URL__?: unknown }).__QPHISING_API_BASE_URL__;

  if (typeof runtimeConfig === 'string' && runtimeConfig.trim().length > 0) {
    return runtimeConfig;
  }

  return environment.apiBaseUrl;
};

const configureOpenApiClient = (): void => {
  OpenAPI.BASE = resolveApiBaseUrl();
};

export const provideOpenApiConfiguration = (): EnvironmentProviders =>
  makeEnvironmentProviders([
    {
      provide: APP_INITIALIZER,
      multi: true,
      useFactory: () => () => configureOpenApiClient()
    }
  ]);
