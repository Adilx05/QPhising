/* generated using openapi-typescript-codegen -- do not edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { GenerateTrackingLinkApiRequest } from '../models/GenerateTrackingLinkApiRequest';
import type { GenerateTrackingLinkApiResponse } from '../models/GenerateTrackingLinkApiResponse';
import type { ProcessTrackingClickApiResponse } from '../models/ProcessTrackingClickApiResponse';
import type { CancelablePromise } from '../core/CancelablePromise';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';
export class TrackingService {
    /**
     * @returns GenerateTrackingLinkApiResponse Created
     * @throws ApiError
     */
    public static postApiTrackingLinks({
        requestBody,
        xApiVersion,
    }: {
        requestBody: GenerateTrackingLinkApiRequest,
        xApiVersion?: string,
    }): CancelablePromise<GenerateTrackingLinkApiResponse> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/api/Tracking/links',
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
     * @returns GenerateTrackingLinkApiResponse Created
     * @throws ApiError
     */
    public static postApiV1TrackingLinks({
        requestBody,
    }: {
        requestBody: GenerateTrackingLinkApiRequest,
    }): CancelablePromise<GenerateTrackingLinkApiResponse> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/api/v1/Tracking/links',
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
     * @returns ProcessTrackingClickApiResponse OK
     * @throws ApiError
     */
    public static getApiTrackingClick({
        campaignId,
        trackingToken,
        fingerprint,
        xApiVersion,
    }: {
        campaignId: string,
        trackingToken: string,
        fingerprint?: string,
        xApiVersion?: string,
    }): CancelablePromise<ProcessTrackingClickApiResponse> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/api/Tracking/click/{campaignId}/{trackingToken}',
            path: {
                'campaignId': campaignId,
                'trackingToken': trackingToken,
            },
            headers: {
                'x-api-version': xApiVersion,
            },
            query: {
                'fingerprint': fingerprint,
            },
            errors: {
                400: `Bad Request`,
                404: `Not Found`,
            },
        });
    }
    /**
     * @returns ProcessTrackingClickApiResponse OK
     * @throws ApiError
     */
    public static getApiV1TrackingClick({
        campaignId,
        trackingToken,
        fingerprint,
    }: {
        campaignId: string,
        trackingToken: string,
        fingerprint?: string,
    }): CancelablePromise<ProcessTrackingClickApiResponse> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/api/v1/Tracking/click/{campaignId}/{trackingToken}',
            path: {
                'campaignId': campaignId,
                'trackingToken': trackingToken,
            },
            query: {
                'fingerprint': fingerprint,
            },
            errors: {
                400: `Bad Request`,
                404: `Not Found`,
            },
        });
    }
}
