/* generated using openapi-typescript-codegen -- do not edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { RuntimeConfigurationResult } from '../models/RuntimeConfigurationResult';
import type { SaveRuntimeConfigurationRequest } from '../models/SaveRuntimeConfigurationRequest';
import type { UpdateRuntimeConfigurationRequest } from '../models/UpdateRuntimeConfigurationRequest';
import type { CancelablePromise } from '../core/CancelablePromise';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';
export class ConfigurationService {
  /**
   * Get current persisted runtime configuration readiness state.
   * @returns RuntimeConfigurationResult OK
   * @throws ApiError
   */
  public static configurationGetCurrent(): CancelablePromise<RuntimeConfigurationResult> {
    return __request(OpenAPI, {
      method: 'GET',
      url: '/api/configuration',
      errors: {
        400: `Bad Request`,
        401: `Unauthorized`,
        403: `Forbidden`,
        500: `Internal Server Error`,
      },
    });
  }
  /**
   * Persist full runtime configuration payload.
   * @returns RuntimeConfigurationResult OK
   * @throws ApiError
   */
  public static configurationSave({
    requestBody,
  }: {
    requestBody?: SaveRuntimeConfigurationRequest,
  }): CancelablePromise<RuntimeConfigurationResult> {
    return __request(OpenAPI, {
      method: 'POST',
      url: '/api/configuration',
      body: requestBody,
      mediaType: 'application/json',
      errors: {
        400: `Bad Request`,
        401: `Unauthorized`,
        403: `Forbidden`,
        500: `Internal Server Error`,
      },
    });
  }
  /**
   * Update runtime configuration values selectively.
   * @returns RuntimeConfigurationResult OK
   * @throws ApiError
   */
  public static configurationUpdate({
    requestBody,
  }: {
    requestBody?: UpdateRuntimeConfigurationRequest,
  }): CancelablePromise<RuntimeConfigurationResult> {
    return __request(OpenAPI, {
      method: 'PATCH',
      url: '/api/configuration',
      body: requestBody,
      mediaType: 'application/json',
      errors: {
        400: `Bad Request`,
        401: `Unauthorized`,
        403: `Forbidden`,
        500: `Internal Server Error`,
      },
    });
  }
}
