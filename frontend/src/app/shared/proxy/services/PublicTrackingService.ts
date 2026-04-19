/* generated using openapi-typescript-codegen -- do not edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { TrackingLandingPageResult } from '../models/TrackingLandingPageResult';
import type { CancelablePromise } from '../core/CancelablePromise';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';
export class PublicTrackingService {
  /**
   * @returns TrackingLandingPageResult OK
   * @throws ApiError
   */
  public static trackingPublicLandingBySlug({
    slug,
    id,
    campaign,
  }: {
    slug: string,
    id?: string,
    campaign?: string,
  }): CancelablePromise<TrackingLandingPageResult> {
    return __request(OpenAPI, {
      method: 'GET',
      url: '/p/{slug}',
      path: {
        'slug': slug,
      },
      query: {
        'id': id,
        'campaign': campaign,
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
