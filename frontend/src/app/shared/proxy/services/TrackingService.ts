/* generated using openapi-typescript-codegen -- do not edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { CaptureVisitRequest } from '../models/CaptureVisitRequest';
import type { CreateTrackingPageRequest } from '../models/CreateTrackingPageRequest';
import type { TrackingPageAnalyticsResult } from '../models/TrackingPageAnalyticsResult';
import type { TrackingPageResult } from '../models/TrackingPageResult';
import type { UpdateTrackingPageRequest } from '../models/UpdateTrackingPageRequest';
import type { VisitIngestionResult } from '../models/VisitIngestionResult';
import type { CancelablePromise } from '../core/CancelablePromise';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';
export class TrackingService {
  /**
   * @returns TrackingPageResult OK
   * @throws ApiError
   */
  public static trackingPageList(): CancelablePromise<Array<TrackingPageResult>> {
    return __request(OpenAPI, {
      method: 'GET',
      url: '/api/tracking/pages',
      errors: {
        400: `Bad Request`,
        401: `Unauthorized`,
        403: `Forbidden`,
        500: `Internal Server Error`,
      },
    });
  }
  /**
   * @returns TrackingPageResult OK
   * @throws ApiError
   */
  public static trackingPageCreate({
    requestBody,
  }: {
    requestBody?: CreateTrackingPageRequest,
  }): CancelablePromise<TrackingPageResult> {
    return __request(OpenAPI, {
      method: 'POST',
      url: '/api/tracking/pages',
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
   * @returns TrackingPageResult OK
   * @throws ApiError
   */
  public static trackingPageGetById({
    trackingPageId,
  }: {
    trackingPageId: string,
  }): CancelablePromise<TrackingPageResult> {
    return __request(OpenAPI, {
      method: 'GET',
      url: '/api/tracking/pages/{trackingPageId}',
      path: {
        'trackingPageId': trackingPageId,
      },
      errors: {
        400: `Bad Request`,
        401: `Unauthorized`,
        403: `Forbidden`,
        404: `Not Found`,
        500: `Internal Server Error`,
      },
    });
  }
  /**
   * @returns TrackingPageResult OK
   * @throws ApiError
   */
  public static trackingPageUpdate({
    trackingPageId,
    requestBody,
  }: {
    trackingPageId: string,
    requestBody?: UpdateTrackingPageRequest,
  }): CancelablePromise<TrackingPageResult> {
    return __request(OpenAPI, {
      method: 'PUT',
      url: '/api/tracking/pages/{trackingPageId}',
      path: {
        'trackingPageId': trackingPageId,
      },
      body: requestBody,
      mediaType: 'application/json',
      errors: {
        400: `Bad Request`,
        401: `Unauthorized`,
        403: `Forbidden`,
        404: `Not Found`,
        500: `Internal Server Error`,
      },
    });
  }
  /**
   * @returns void
   * @throws ApiError
   */
  public static trackingPageDelete({
    trackingPageId,
  }: {
    trackingPageId: string,
  }): CancelablePromise<void> {
    return __request(OpenAPI, {
      method: 'DELETE',
      url: '/api/tracking/pages/{trackingPageId}',
      path: {
        'trackingPageId': trackingPageId,
      },
      errors: {
        400: `Bad Request`,
        401: `Unauthorized`,
        403: `Forbidden`,
        404: `Not Found`,
        500: `Internal Server Error`,
      },
    });
  }
  /**
   * @returns TrackingPageResult OK
   * @throws ApiError
   */
  public static trackingPagePublish({
    trackingPageId,
  }: {
    trackingPageId: string,
  }): CancelablePromise<TrackingPageResult> {
    return __request(OpenAPI, {
      method: 'POST',
      url: '/api/tracking/pages/{trackingPageId}/publish',
      path: {
        'trackingPageId': trackingPageId,
      },
      errors: {
        400: `Bad Request`,
        401: `Unauthorized`,
        403: `Forbidden`,
        404: `Not Found`,
        500: `Internal Server Error`,
      },
    });
  }
  /**
   * @returns TrackingPageResult OK
   * @throws ApiError
   */
  public static trackingPageArchive({
    trackingPageId,
  }: {
    trackingPageId: string,
  }): CancelablePromise<TrackingPageResult> {
    return __request(OpenAPI, {
      method: 'POST',
      url: '/api/tracking/pages/{trackingPageId}/archive',
      path: {
        'trackingPageId': trackingPageId,
      },
      errors: {
        400: `Bad Request`,
        401: `Unauthorized`,
        403: `Forbidden`,
        404: `Not Found`,
        500: `Internal Server Error`,
      },
    });
  }
  /**
   * @returns VisitIngestionResult OK
   * @throws ApiError
   */
  public static trackingPageCaptureVisit({
    trackingPageId,
    requestBody,
  }: {
    trackingPageId: string,
    requestBody?: CaptureVisitRequest,
  }): CancelablePromise<VisitIngestionResult> {
    return __request(OpenAPI, {
      method: 'POST',
      url: '/api/tracking/pages/{trackingPageId}/visits',
      path: {
        'trackingPageId': trackingPageId,
      },
      body: requestBody,
      mediaType: 'application/json',
      errors: {
        400: `Bad Request`,
        401: `Unauthorized`,
        403: `Forbidden`,
        404: `Not Found`,
        500: `Internal Server Error`,
      },
    });
  }
  /**
   * @returns TrackingPageAnalyticsResult OK
   * @throws ApiError
   */
  public static trackingPageGetAnalytics({
    trackingPageId,
    fromUtc,
    toUtc,
    trendBucketSizeMinutes = 60,
    recentVisitLimit = 25,
  }: {
    trackingPageId: string,
    fromUtc?: string,
    toUtc?: string,
    trendBucketSizeMinutes?: number,
    recentVisitLimit?: number,
  }): CancelablePromise<TrackingPageAnalyticsResult> {
    return __request(OpenAPI, {
      method: 'GET',
      url: '/api/tracking/pages/{trackingPageId}/analytics',
      path: {
        'trackingPageId': trackingPageId,
      },
      query: {
        'fromUtc': fromUtc,
        'toUtc': toUtc,
        'trendBucketSizeMinutes': trendBucketSizeMinutes,
        'recentVisitLimit': recentVisitLimit,
      },
      errors: {
        400: `Bad Request`,
        401: `Unauthorized`,
        403: `Forbidden`,
        404: `Not Found`,
        500: `Internal Server Error`,
      },
    });
  }
}
