/* generated using openapi-typescript-codegen -- do not edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { ActivateCampaignResponse } from '../models/ActivateCampaignResponse';
import type { CampaignDetailResponse } from '../models/CampaignDetailResponse';
import type { CampaignStatus } from '../models/CampaignStatus';
import type { CreateCampaignRequest } from '../models/CreateCampaignRequest';
import type { CreateCampaignResponse } from '../models/CreateCampaignResponse';
import type { ListCampaignsResponse } from '../models/ListCampaignsResponse';
import type { ScheduleCampaignResponse } from '../models/ScheduleCampaignResponse';
import type { TemplateType } from '../models/TemplateType';
import type { UpdateCampaignRequest } from '../models/UpdateCampaignRequest';
import type { UpdateCampaignResponse } from '../models/UpdateCampaignResponse';
import type { CancelablePromise } from '../core/CancelablePromise';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';
export class CampaignsService {
    /**
     * @returns ListCampaignsResponse OK
     * @throws ApiError
     */
    public static getApiCampaigns({
        statuses,
        templateTypes,
        startsOnOrAfter,
        endsOnOrBefore,
        skip,
        take,
        xApiVersion,
    }: {
        statuses?: Array<CampaignStatus>,
        templateTypes?: Array<TemplateType>,
        startsOnOrAfter?: string,
        endsOnOrBefore?: string,
        skip?: number | string,
        take?: number | string,
        xApiVersion?: string,
    }): CancelablePromise<ListCampaignsResponse> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/api/Campaigns',
            headers: {
                'x-api-version': xApiVersion,
            },
            query: {
                'statuses': statuses,
                'templateTypes': templateTypes,
                'startsOnOrAfter': startsOnOrAfter,
                'endsOnOrBefore': endsOnOrBefore,
                'skip': skip,
                'take': take,
            },
            errors: {
                400: `Bad Request`,
                401: `Unauthorized`,
                403: `Forbidden`,
            },
        });
    }
    /**
     * @returns CreateCampaignResponse Created
     * @throws ApiError
     */
    public static postApiCampaigns({
        requestBody,
        xApiVersion,
    }: {
        requestBody: CreateCampaignRequest,
        xApiVersion?: string,
    }): CancelablePromise<CreateCampaignResponse> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/api/Campaigns',
            headers: {
                'x-api-version': xApiVersion,
            },
            body: requestBody,
            mediaType: 'application/json',
            errors: {
                400: `Bad Request`,
                401: `Unauthorized`,
                403: `Forbidden`,
            },
        });
    }
    /**
     * @returns ListCampaignsResponse OK
     * @throws ApiError
     */
    public static getApiV1Campaigns({
        statuses,
        templateTypes,
        startsOnOrAfter,
        endsOnOrBefore,
        skip,
        take,
    }: {
        statuses?: Array<CampaignStatus>,
        templateTypes?: Array<TemplateType>,
        startsOnOrAfter?: string,
        endsOnOrBefore?: string,
        skip?: number | string,
        take?: number | string,
    }): CancelablePromise<ListCampaignsResponse> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/api/v1/Campaigns',
            query: {
                'statuses': statuses,
                'templateTypes': templateTypes,
                'startsOnOrAfter': startsOnOrAfter,
                'endsOnOrBefore': endsOnOrBefore,
                'skip': skip,
                'take': take,
            },
            errors: {
                400: `Bad Request`,
                401: `Unauthorized`,
                403: `Forbidden`,
            },
        });
    }
    /**
     * @returns CreateCampaignResponse Created
     * @throws ApiError
     */
    public static postApiV1Campaigns({
        requestBody,
    }: {
        requestBody: CreateCampaignRequest,
    }): CancelablePromise<CreateCampaignResponse> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/api/v1/Campaigns',
            body: requestBody,
            mediaType: 'application/json',
            errors: {
                400: `Bad Request`,
                401: `Unauthorized`,
                403: `Forbidden`,
            },
        });
    }
    /**
     * @returns CampaignDetailResponse OK
     * @throws ApiError
     */
    public static getApiCampaigns1({
        campaignId,
        xApiVersion,
    }: {
        campaignId: string,
        xApiVersion?: string,
    }): CancelablePromise<CampaignDetailResponse> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/api/Campaigns/{campaignId}',
            path: {
                'campaignId': campaignId,
            },
            headers: {
                'x-api-version': xApiVersion,
            },
            errors: {
                401: `Unauthorized`,
                403: `Forbidden`,
                404: `Not Found`,
            },
        });
    }
    /**
     * @returns UpdateCampaignResponse OK
     * @throws ApiError
     */
    public static putApiCampaigns({
        campaignId,
        requestBody,
        xApiVersion,
    }: {
        campaignId: string,
        requestBody: UpdateCampaignRequest,
        xApiVersion?: string,
    }): CancelablePromise<UpdateCampaignResponse> {
        return __request(OpenAPI, {
            method: 'PUT',
            url: '/api/Campaigns/{campaignId}',
            path: {
                'campaignId': campaignId,
            },
            headers: {
                'x-api-version': xApiVersion,
            },
            body: requestBody,
            mediaType: 'application/json',
            errors: {
                400: `Bad Request`,
                401: `Unauthorized`,
                403: `Forbidden`,
                404: `Not Found`,
            },
        });
    }
    /**
     * @returns CampaignDetailResponse OK
     * @throws ApiError
     */
    public static getApiV1Campaigns1({
        campaignId,
    }: {
        campaignId: string,
    }): CancelablePromise<CampaignDetailResponse> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/api/v1/Campaigns/{campaignId}',
            path: {
                'campaignId': campaignId,
            },
            errors: {
                401: `Unauthorized`,
                403: `Forbidden`,
                404: `Not Found`,
            },
        });
    }
    /**
     * @returns UpdateCampaignResponse OK
     * @throws ApiError
     */
    public static putApiV1Campaigns({
        campaignId,
        requestBody,
    }: {
        campaignId: string,
        requestBody: UpdateCampaignRequest,
    }): CancelablePromise<UpdateCampaignResponse> {
        return __request(OpenAPI, {
            method: 'PUT',
            url: '/api/v1/Campaigns/{campaignId}',
            path: {
                'campaignId': campaignId,
            },
            body: requestBody,
            mediaType: 'application/json',
            errors: {
                400: `Bad Request`,
                401: `Unauthorized`,
                403: `Forbidden`,
                404: `Not Found`,
            },
        });
    }
    /**
     * @returns ScheduleCampaignResponse OK
     * @throws ApiError
     */
    public static postApiCampaignsSchedule({
        campaignId,
        xApiVersion,
    }: {
        campaignId: string,
        xApiVersion?: string,
    }): CancelablePromise<ScheduleCampaignResponse> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/api/Campaigns/{campaignId}/schedule',
            path: {
                'campaignId': campaignId,
            },
            headers: {
                'x-api-version': xApiVersion,
            },
            errors: {
                400: `Bad Request`,
                401: `Unauthorized`,
                403: `Forbidden`,
                404: `Not Found`,
            },
        });
    }
    /**
     * @returns ScheduleCampaignResponse OK
     * @throws ApiError
     */
    public static postApiV1CampaignsSchedule({
        campaignId,
    }: {
        campaignId: string,
    }): CancelablePromise<ScheduleCampaignResponse> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/api/v1/Campaigns/{campaignId}/schedule',
            path: {
                'campaignId': campaignId,
            },
            errors: {
                400: `Bad Request`,
                401: `Unauthorized`,
                403: `Forbidden`,
                404: `Not Found`,
            },
        });
    }
    /**
     * @returns ActivateCampaignResponse OK
     * @throws ApiError
     */
    public static postApiCampaignsActivate({
        campaignId,
        xApiVersion,
    }: {
        campaignId: string,
        xApiVersion?: string,
    }): CancelablePromise<ActivateCampaignResponse> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/api/Campaigns/{campaignId}/activate',
            path: {
                'campaignId': campaignId,
            },
            headers: {
                'x-api-version': xApiVersion,
            },
            errors: {
                400: `Bad Request`,
                401: `Unauthorized`,
                403: `Forbidden`,
                404: `Not Found`,
            },
        });
    }
    /**
     * @returns ActivateCampaignResponse OK
     * @throws ApiError
     */
    public static postApiV1CampaignsActivate({
        campaignId,
    }: {
        campaignId: string,
    }): CancelablePromise<ActivateCampaignResponse> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/api/v1/Campaigns/{campaignId}/activate',
            path: {
                'campaignId': campaignId,
            },
            errors: {
                400: `Bad Request`,
                401: `Unauthorized`,
                403: `Forbidden`,
                404: `Not Found`,
            },
        });
    }
}
