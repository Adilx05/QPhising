/* generated using openapi-typescript-codegen -- do not edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { SaveSetupConfigurationRequest } from '../models/SaveSetupConfigurationRequest';
import type { SetupDependencyTestResult } from '../models/SetupDependencyTestResult';
import type { SetupGuardDecisionResult } from '../models/SetupGuardDecisionResult';
import type { SetupStatusResult } from '../models/SetupStatusResult';
import type { TestDatabaseConnectionRequest } from '../models/TestDatabaseConnectionRequest';
import type { TestKeycloakConnectionRequest } from '../models/TestKeycloakConnectionRequest';
import type { TestRedisConnectionRequest } from '../models/TestRedisConnectionRequest';
import type { CancelablePromise } from '../core/CancelablePromise';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';
export class SetupService {
  /**
   * Get current setup readiness status.
   * @returns SetupStatusResult OK
   * @throws ApiError
   */
  public static getApiSetupStatus(): CancelablePromise<SetupStatusResult> {
    return __request(OpenAPI, {
      method: 'GET',
      url: '/api/setup/status',
    });
  }
  /**
   * Resolve setup-gating decision for setup wizard and main application access.
   * @returns SetupGuardDecisionResult OK
   * @throws ApiError
   */
  public static getApiSetupGuardDecision(): CancelablePromise<SetupGuardDecisionResult> {
    return __request(OpenAPI, {
      method: 'GET',
      url: '/api/setup/guard-decision',
    });
  }
  /**
   * Validate database connectivity using the supplied connection string.
   * @returns SetupDependencyTestResult OK
   * @throws ApiError
   */
  public static postApiSetupTestDb({
    requestBody,
  }: {
    requestBody?: TestDatabaseConnectionRequest,
  }): CancelablePromise<SetupDependencyTestResult> {
    return __request(OpenAPI, {
      method: 'POST',
      url: '/api/setup/test-db',
      body: requestBody,
      mediaType: 'application/json',
    });
  }
  /**
   * Validate Redis connectivity using the supplied connection string.
   * @returns SetupDependencyTestResult OK
   * @throws ApiError
   */
  public static postApiSetupTestRedis({
    requestBody,
  }: {
    requestBody?: TestRedisConnectionRequest,
  }): CancelablePromise<SetupDependencyTestResult> {
    return __request(OpenAPI, {
      method: 'POST',
      url: '/api/setup/test-redis',
      body: requestBody,
      mediaType: 'application/json',
    });
  }
  /**
   * Validate Keycloak authority, realm, and client credentials.
   * @returns SetupDependencyTestResult OK
   * @throws ApiError
   */
  public static postApiSetupTestKeycloak({
    requestBody,
  }: {
    requestBody?: TestKeycloakConnectionRequest,
  }): CancelablePromise<SetupDependencyTestResult> {
    return __request(OpenAPI, {
      method: 'POST',
      url: '/api/setup/test-keycloak',
      body: requestBody,
      mediaType: 'application/json',
    });
  }
  /**
   * Persist setup configuration after successful dependency checks.
   * @returns SetupStatusResult OK
   * @throws ApiError
   */
  public static postApiSetupSave({
    requestBody,
  }: {
    requestBody?: SaveSetupConfigurationRequest,
  }): CancelablePromise<SetupStatusResult> {
    return __request(OpenAPI, {
      method: 'POST',
      url: '/api/setup/save',
      body: requestBody,
      mediaType: 'application/json',
    });
  }
}
