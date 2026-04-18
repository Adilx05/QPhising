/* generated using openapi-typescript-codegen -- do not edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { GatewayRoutePolicyCompositionResult } from '../models/GatewayRoutePolicyCompositionResult';
import type { CancelablePromise } from '../core/CancelablePromise';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';
export class GatewayService {
  /**
   * @returns GatewayRoutePolicyCompositionResult OK
   * @throws ApiError
   */
  public static getGatewayRoutePolicies(): CancelablePromise<GatewayRoutePolicyCompositionResult> {
    return __request(OpenAPI, {
      method: 'GET',
      url: '/api/gateway/route-policies',
      errors: {
        400: `Bad Request`,
        401: `Unauthorized`,
        403: `Forbidden`,
        500: `Internal Server Error`,
      },
    });
  }
}
