/* generated using openapi-typescript-codegen -- do not edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { AuditLogPageResult } from '../models/AuditLogPageResult';
import type { AuditLogSortField } from '../models/AuditLogSortField';
import type { SortDirection } from '../models/SortDirection';
import type { CancelablePromise } from '../core/CancelablePromise';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';
export class AuditService {
  /**
   * @returns AuditLogPageResult OK
   * @throws ApiError
   */
  public static auditListLogs({
    fromUtc,
    toUtc,
    actor,
    endpoint,
    outcomeCode,
    correlationId,
    page,
    pageSize,
    sortBy,
    sortDirection,
  }: {
    fromUtc?: string,
    toUtc?: string,
    actor?: string,
    endpoint?: string,
    outcomeCode?: number,
    correlationId?: string,
    page?: number,
    pageSize?: number,
    sortBy?: AuditLogSortField,
    sortDirection?: SortDirection,
  }): CancelablePromise<AuditLogPageResult> {
    return __request(OpenAPI, {
      method: 'GET',
      url: '/api/audit/logs',
      query: {
        'FromUtc': fromUtc,
        'ToUtc': toUtc,
        'Actor': actor,
        'Endpoint': endpoint,
        'OutcomeCode': outcomeCode,
        'CorrelationId': correlationId,
        'Page': page,
        'PageSize': pageSize,
        'SortBy': sortBy,
        'SortDirection': sortDirection,
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
