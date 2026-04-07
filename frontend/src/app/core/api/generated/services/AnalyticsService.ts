/* generated using openapi-typescript-codegen -- do not edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { AnalyticsTimeGrain } from '../models/AnalyticsTimeGrain';
import type { CampaignStatus } from '../models/CampaignStatus';
import type { DashboardKpisResponse } from '../models/DashboardKpisResponse';
import type { TemplateType } from '../models/TemplateType';
import type { CancelablePromise } from '../core/CancelablePromise';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';
export class AnalyticsService {
    /**
     * @returns DashboardKpisResponse OK
     * @throws ApiError
     */
    public static getApiAnalyticsDashboardKpis({
        from,
        to,
        timeGrain = 2,
        timeZone = 'UTC',
        campaignIds,
        templateTypes,
        campaignStatuses,
        xApiVersion,
    }: {
        from?: string,
        to?: string,
        timeGrain?: AnalyticsTimeGrain,
        timeZone?: string,
        campaignIds?: Array<string>,
        templateTypes?: Array<TemplateType>,
        campaignStatuses?: Array<CampaignStatus>,
        xApiVersion?: string,
    }): CancelablePromise<DashboardKpisResponse> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/api/Analytics/dashboard-kpis',
            headers: {
                'x-api-version': xApiVersion,
            },
            query: {
                'from': from,
                'to': to,
                'timeGrain': timeGrain,
                'timeZone': timeZone,
                'campaignIds': campaignIds,
                'templateTypes': templateTypes,
                'campaignStatuses': campaignStatuses,
            },
            errors: {
                400: `Bad Request`,
                401: `Unauthorized`,
                403: `Forbidden`,
            },
        });
    }
    /**
     * @returns DashboardKpisResponse OK
     * @throws ApiError
     */
    public static getApiV1AnalyticsDashboardKpis({
        from,
        to,
        timeGrain = 2,
        timeZone = 'UTC',
        campaignIds,
        templateTypes,
        campaignStatuses,
    }: {
        from?: string,
        to?: string,
        timeGrain?: AnalyticsTimeGrain,
        timeZone?: string,
        campaignIds?: Array<string>,
        templateTypes?: Array<TemplateType>,
        campaignStatuses?: Array<CampaignStatus>,
    }): CancelablePromise<DashboardKpisResponse> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/api/v1/Analytics/dashboard-kpis',
            query: {
                'from': from,
                'to': to,
                'timeGrain': timeGrain,
                'timeZone': timeZone,
                'campaignIds': campaignIds,
                'templateTypes': templateTypes,
                'campaignStatuses': campaignStatuses,
            },
            errors: {
                400: `Bad Request`,
                401: `Unauthorized`,
                403: `Forbidden`,
            },
        });
    }
}
