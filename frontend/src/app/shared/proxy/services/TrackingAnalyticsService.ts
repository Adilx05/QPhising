/* generated using openapi-typescript-codegen -- do not edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { TrackingAnalyticsOverviewResult } from '../models/TrackingAnalyticsOverviewResult';
import type { TrackingReportDetailLevel } from '../models/TrackingReportDetailLevel';
import type { TrackingReportFormat } from '../models/TrackingReportFormat';
import type { TrackingReportScope } from '../models/TrackingReportScope';
import type { TrackingVisitTrendBucketWindow } from '../models/TrackingVisitTrendBucketWindow';
import type { CancelablePromise } from '../core/CancelablePromise';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';
export class TrackingAnalyticsService {
  /**
   * @returns TrackingAnalyticsOverviewResult OK
   * @throws ApiError
   */
  public static trackingAnalyticsGetOverview({
    fromUtc,
    toUtc,
    trendWindow,
    timezoneOffsetMinutes,
    excludeBots = true,
    topPagesLimit = 10,
    recentVisitLimit = 25,
  }: {
    fromUtc?: string,
    toUtc?: string,
    trendWindow?: TrackingVisitTrendBucketWindow,
    timezoneOffsetMinutes?: number,
    excludeBots?: boolean,
    topPagesLimit?: number,
    recentVisitLimit?: number,
  }): CancelablePromise<TrackingAnalyticsOverviewResult> {
    return __request(OpenAPI, {
      method: 'GET',
      url: '/api/tracking/analytics/overview',
      query: {
        'fromUtc': fromUtc,
        'toUtc': toUtc,
        'trendWindow': trendWindow,
        'timezoneOffsetMinutes': timezoneOffsetMinutes,
        'excludeBots': excludeBots,
        'topPagesLimit': topPagesLimit,
        'recentVisitLimit': recentVisitLimit,
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
   * @returns any OK
   * @throws ApiError
   */
  public static trackingAnalyticsExportReport({
    format,
    scope,
    detailLevel,
    trackingPageId,
    fromUtc,
    toUtc,
    excludeBots = true,
    timezoneOffsetMinutes,
    language = 'en',
  }: {
    format?: TrackingReportFormat,
    scope?: TrackingReportScope,
    detailLevel?: TrackingReportDetailLevel,
    trackingPageId?: string,
    fromUtc?: string,
    toUtc?: string,
    excludeBots?: boolean,
    timezoneOffsetMinutes?: number,
    language?: string,
  }): CancelablePromise<any> {
    return __request(OpenAPI, {
      method: 'GET',
      url: '/api/tracking/analytics/reports/export',
      query: {
        'format': format,
        'scope': scope,
        'detailLevel': detailLevel,
        'trackingPageId': trackingPageId,
        'fromUtc': fromUtc,
        'toUtc': toUtc,
        'excludeBots': excludeBots,
        'timezoneOffsetMinutes': timezoneOffsetMinutes,
        'language': language,
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
