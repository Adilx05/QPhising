import { APP_INITIALIZER, EnvironmentProviders, inject, makeEnvironmentProviders, NgZone } from '@angular/core';
import { Router } from '@angular/router';
import axios from 'axios';
import { environment } from '../../../environments/environment';
import { AuthSessionService } from '../auth/auth-session';
import { OidcAuthService } from '../auth/oidc-auth.service';
import { OpenAPI } from '../../shared/proxy';

const resolveApiBaseUrl = (): string => {
  const runtimeConfig = (globalThis as { __QPHISING_API_BASE_URL__?: unknown }).__QPHISING_API_BASE_URL__;

  if (typeof runtimeConfig === 'string' && runtimeConfig.trim().length > 0) {
    return runtimeConfig;
  }

  return environment.apiBaseUrl;
};

let unauthorisedRedirectInProgress = false;

const configureOpenApiClient = (
  authSessionService: AuthSessionService,
  oidcAuthService: OidcAuthService,
  router: Router,
  ngZone: NgZone
): void => {
  OpenAPI.BASE = resolveApiBaseUrl();
  OpenAPI.TOKEN = () => Promise.resolve(authSessionService.getAccessToken() ?? '');

  axios.interceptors.response.use(
    (response) => response,
    (error) => {
      if (error?.response?.status === 401 && !unauthorisedRedirectInProgress) {
        unauthorisedRedirectInProgress = true;

        ngZone.run(() => {
          if (!oidcAuthService.hasValidRefreshToken()) {
            void oidcAuthService.login(router.url).finally(() => {
              unauthorisedRedirectInProgress = false;
            });
          } else {
            unauthorisedRedirectInProgress = false;
          }
        });
      }

      return Promise.reject(error);
    }
  );
};

export const provideOpenApiConfiguration = (): EnvironmentProviders =>
  makeEnvironmentProviders([
    {
      provide: APP_INITIALIZER,
      multi: true,
      useFactory: () => {
        const authSessionService = inject(AuthSessionService);
        const oidcAuthService = inject(OidcAuthService);
        const router = inject(Router);
        const ngZone = inject(NgZone);

        return () => configureOpenApiClient(authSessionService, oidcAuthService, router, ngZone);
      }
    }
  ]);
