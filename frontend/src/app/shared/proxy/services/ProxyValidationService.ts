/* generated using openapi-typescript-codegen -- do not edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { AssertProxyContractSyncRequest } from '../models/AssertProxyContractSyncRequest';
import type { CancelablePromise } from '../core/CancelablePromise';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';
export class ProxyValidationService {
  /**
   * @returns void
   * @throws ApiError
   */
  public static postApiProxyValidationAssertSync({
    requestBody,
  }: {
    requestBody?: AssertProxyContractSyncRequest,
  }): CancelablePromise<void> {
    return __request(OpenAPI, {
      method: 'POST',
      url: '/api/proxy-validation/assert-sync',
      body: requestBody,
      mediaType: 'application/json',
      errors: {
        409: `Conflict`,
      },
    });
  }
}
