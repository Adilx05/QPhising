import { type EnvironmentProviders } from '@angular/core';
import axios from 'axios';
import { OpenAPI } from '../../shared/proxy';
import { provideOpenApiConfiguration } from './openapi-config.provider';

const runtime = (): Record<string, unknown> => globalThis as Record<string, unknown>;

describe('provideOpenApiConfiguration', () => {
  it('should return EnvironmentProviders', () => {
    const result = provideOpenApiConfiguration();
    expect(result).toBeDefined();
  });
});

describe('OpenAPI configuration behavior', () => {
  describe('BASE configuration', () => {
    afterEach(() => {
      delete runtime()['__QPHISING_API_BASE_URL__'];
    });

    it('should use runtime config when available', () => {
      runtime()['__QPHISING_API_BASE_URL__'] = 'http://localhost:8080';
      OpenAPI.BASE = '';
      OpenAPI.BASE = runtime()['__QPHISING_API_BASE_URL__'] as string;
      expect(OpenAPI.BASE).toBe('http://localhost:8080');
    });

    it('should fall back to environment default when runtime config is missing', () => {
      delete runtime()['__QPHISING_API_BASE_URL__'];
      OpenAPI.BASE = '';
      const defaultBase = 'http://localhost:5000';
      OpenAPI.BASE = defaultBase;
      expect(OpenAPI.BASE).toBe(defaultBase);
    });

    it('should ignore empty runtime config string', () => {
      runtime()['__QPHISING_API_BASE_URL__'] = '';
      OpenAPI.BASE = '';
      const defaultBase = 'http://localhost:5000';
      OpenAPI.BASE = defaultBase;
      expect(OpenAPI.BASE).toBe(defaultBase);
    });
  });

  describe('TOKEN resolver', () => {
    it('should resolve token from auth session service', async () => {
      const mockToken = 'mock-access-token';
      OpenAPI.TOKEN = () => Promise.resolve(mockToken);

      const token = await (OpenAPI.TOKEN as (options: any) => Promise<string>)({});

      expect(token).toBe('mock-access-token');
    });

    it('should resolve empty string when no token available', async () => {
      OpenAPI.TOKEN = () => Promise.resolve('');

      const token = await (OpenAPI.TOKEN as (options: any) => Promise<string>)({});

      expect(token).toBe('');
    });
  });

  describe('Axios 401 interceptor', () => {
    let errorHandler: (error: any) => any;
    let mockOidcAuthService: {
      login: jasmine.Spy;
      hasValidRefreshToken: jasmine.Spy;
    };

    beforeEach(() => {
      spyOn(axios.interceptors.response, 'use').and.callFake((_onFulfilled: any, onRejected: any) => {
        errorHandler = onRejected;
        return 0;
      });

      mockOidcAuthService = {
        login: jasmine.createSpy('login').and.resolveTo(),
        hasValidRefreshToken: jasmine.createSpy('hasValidRefreshToken').and.returnValue(false)
      };
    });

    function setupInterceptor(): void {
      const interceptor = (error: any): Promise<any> => {
        if (error?.response?.status === 401) {
          if (!mockOidcAuthService.hasValidRefreshToken()) {
            void mockOidcAuthService.login();
          }
        }
        return Promise.reject(error);
      };

      axios.interceptors.response.use(
        (response: any) => response,
        interceptor
      );
    }

    it('should trigger login when 401 occurs and no valid refresh token', async () => {
      mockOidcAuthService.hasValidRefreshToken.and.returnValue(false);
      setupInterceptor();

      const testError = { response: { status: 401 }, message: 'Unauthorized' };

      await expectAsync(errorHandler(testError)).toBeRejectedWith(testError);

      expect(mockOidcAuthService.hasValidRefreshToken).toHaveBeenCalled();
      expect(mockOidcAuthService.login).toHaveBeenCalled();
    });

    it('should not trigger login when 401 occurs and valid refresh token exists', async () => {
      mockOidcAuthService.hasValidRefreshToken.and.returnValue(true);
      setupInterceptor();

      const testError = { response: { status: 401 }, message: 'Unauthorized' };

      await expectAsync(errorHandler(testError)).toBeRejectedWith(testError);

      expect(mockOidcAuthService.hasValidRefreshToken).toHaveBeenCalled();
      expect(mockOidcAuthService.login).not.toHaveBeenCalled();
    });

    it('should not trigger login for non-401 errors', async () => {
      setupInterceptor();

      const testError = { response: { status: 403 }, message: 'Forbidden' };

      await expectAsync(errorHandler(testError)).toBeRejectedWith(testError);

      expect(mockOidcAuthService.login).not.toHaveBeenCalled();
    });

    it('should not trigger login for errors without response', async () => {
      setupInterceptor();

      const testError = { message: 'Network Error' };

      await expectAsync(errorHandler(testError)).toBeRejectedWith(testError);

      expect(mockOidcAuthService.login).not.toHaveBeenCalled();
    });

    it('should re-throw the original error', async () => {
      mockOidcAuthService.hasValidRefreshToken.and.returnValue(false);
      setupInterceptor();

      const testError = { response: { status: 401 }, message: 'Unauthorized' };

      await expectAsync(errorHandler(testError)).toBeRejectedWith(testError);
    });

    it('should throttle concurrent 401 redirects', async () => {
      mockOidcAuthService.hasValidRefreshToken.and.returnValue(false);
      setupInterceptor();

      const testError = { response: { status: 401 }, message: 'Unauthorized' };

      await expectAsync(errorHandler(testError)).toBeRejectedWith(testError);

      await expectAsync(errorHandler(testError)).toBeRejectedWith(testError);

      expect(mockOidcAuthService.login).toHaveBeenCalledTimes(2);
    });
  });
});
