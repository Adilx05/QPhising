/* generated using openapi-typescript-codegen -- do not edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { CampaignResult } from '../models/CampaignResult';
import type { CreateCampaignRequest } from '../models/CreateCampaignRequest';
import type { ScheduleCampaignRequest } from '../models/ScheduleCampaignRequest';
import type { UpdateCampaignRequest } from '../models/UpdateCampaignRequest';
import type { CancelablePromise } from '../core/CancelablePromise';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';
export class CampaignService {
  /**
   * @returns CampaignResult OK
   * @throws ApiError
   */
  public static campaignList(): CancelablePromise<Array<CampaignResult>> {
    return __request(OpenAPI, {
      method: 'GET',
      url: '/api/campaigns',
      errors: {
        400: `Bad Request`,
        401: `Unauthorized`,
        403: `Forbidden`,
        500: `Internal Server Error`,
      },
    });
  }
  /**
   * @returns CampaignResult OK
   * @throws ApiError
   */
  public static campaignCreate({
    requestBody,
  }: {
    requestBody?: CreateCampaignRequest,
  }): CancelablePromise<CampaignResult> {
    return __request(OpenAPI, {
      method: 'POST',
      url: '/api/campaigns',
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
   * @returns CampaignResult OK
   * @throws ApiError
   */
  public static campaignGetById({
    campaignId,
  }: {
    campaignId: string,
  }): CancelablePromise<CampaignResult> {
    return __request(OpenAPI, {
      method: 'GET',
      url: '/api/campaigns/{campaignId}',
      path: {
        'campaignId': campaignId,
      },
      errors: {
        400: `Bad Request`,
        401: `Unauthorized`,
        403: `Forbidden`,
        500: `Internal Server Error`,
      },
    });
  }
  /**
   * @returns CampaignResult OK
   * @throws ApiError
   */
  public static campaignUpdate({
    campaignId,
    requestBody,
  }: {
    campaignId: string,
    requestBody?: UpdateCampaignRequest,
  }): CancelablePromise<CampaignResult> {
    return __request(OpenAPI, {
      method: 'PUT',
      url: '/api/campaigns/{campaignId}',
      path: {
        'campaignId': campaignId,
      },
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
   * @returns void
   * @throws ApiError
   */
  public static campaignDelete({
    campaignId,
  }: {
    campaignId: string,
  }): CancelablePromise<void> {
    return __request(OpenAPI, {
      method: 'DELETE',
      url: '/api/campaigns/{campaignId}',
      path: {
        'campaignId': campaignId,
      },
      errors: {
        400: `Bad Request`,
        401: `Unauthorized`,
        403: `Forbidden`,
        500: `Internal Server Error`,
      },
    });
  }
  /**
   * @returns CampaignResult OK
   * @throws ApiError
   */
  public static campaignSchedule({
    campaignId,
    requestBody,
  }: {
    campaignId: string,
    requestBody?: ScheduleCampaignRequest,
  }): CancelablePromise<CampaignResult> {
    return __request(OpenAPI, {
      method: 'POST',
      url: '/api/campaigns/{campaignId}/schedule',
      path: {
        'campaignId': campaignId,
      },
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
   * @returns CampaignResult OK
   * @throws ApiError
   */
  public static campaignStart({
    campaignId,
  }: {
    campaignId: string,
  }): CancelablePromise<CampaignResult> {
    return __request(OpenAPI, {
      method: 'POST',
      url: '/api/campaigns/{campaignId}/start',
      path: {
        'campaignId': campaignId,
      },
      errors: {
        400: `Bad Request`,
        401: `Unauthorized`,
        403: `Forbidden`,
        500: `Internal Server Error`,
      },
    });
  }
  /**
   * @returns CampaignResult OK
   * @throws ApiError
   */
  public static campaignPause({
    campaignId,
  }: {
    campaignId: string,
  }): CancelablePromise<CampaignResult> {
    return __request(OpenAPI, {
      method: 'POST',
      url: '/api/campaigns/{campaignId}/pause',
      path: {
        'campaignId': campaignId,
      },
      errors: {
        400: `Bad Request`,
        401: `Unauthorized`,
        403: `Forbidden`,
        500: `Internal Server Error`,
      },
    });
  }
  /**
   * @returns CampaignResult OK
   * @throws ApiError
   */
  public static campaignComplete({
    campaignId,
  }: {
    campaignId: string,
  }): CancelablePromise<CampaignResult> {
    return __request(OpenAPI, {
      method: 'POST',
      url: '/api/campaigns/{campaignId}/complete',
      path: {
        'campaignId': campaignId,
      },
      errors: {
        400: `Bad Request`,
        401: `Unauthorized`,
        403: `Forbidden`,
        500: `Internal Server Error`,
      },
    });
  }
  /**
   * @returns CampaignResult OK
   * @throws ApiError
   */
  public static campaignCancel({
    campaignId,
  }: {
    campaignId: string,
  }): CancelablePromise<CampaignResult> {
    return __request(OpenAPI, {
      method: 'POST',
      url: '/api/campaigns/{campaignId}/cancel',
      path: {
        'campaignId': campaignId,
      },
      errors: {
        400: `Bad Request`,
        401: `Unauthorized`,
        403: `Forbidden`,
        500: `Internal Server Error`,
      },
    });
  }
}
