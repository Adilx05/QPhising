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
   * @returns RuntimeConfigurationResult OK
   * @throws ApiError
   */
  public static getApiConfiguration(): CancelablePromise<RuntimeConfigurationResult> {
    return __request(OpenAPI, {
      method: 'GET',
      url: '/api/configuration',
    });
  }
  /**
   * @returns RuntimeConfigurationResult OK
   * @throws ApiError
   */
  public static postApiConfiguration({
    requestBody,
  }: {
    requestBody?: SaveRuntimeConfigurationRequest,
  }): CancelablePromise<RuntimeConfigurationResult> {
    return __request(OpenAPI, {
      method: 'POST',
      url: '/api/configuration',
      body: requestBody,
      mediaType: 'application/json',
    });
  }
  /**
   * @returns RuntimeConfigurationResult OK
   * @throws ApiError
   */
  public static patchApiConfiguration({
    requestBody,
  }: {
    requestBody?: UpdateRuntimeConfigurationRequest,
  }): CancelablePromise<RuntimeConfigurationResult> {
    return __request(OpenAPI, {
      method: 'PATCH',
      url: '/api/configuration',
      body: requestBody,
      mediaType: 'application/json',
    });
  }
}
