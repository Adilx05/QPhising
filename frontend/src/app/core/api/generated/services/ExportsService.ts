/* generated using openapi-typescript-codegen -- do not edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { ExportJobContract } from '../models/ExportJobContract';
import type { FileContentResult } from '../models/FileContentResult';
import type { QueueExportJobApiRequest } from '../models/QueueExportJobApiRequest';
import type { CancelablePromise } from '../core/CancelablePromise';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';
export class ExportsService {
    /**
     * @returns ExportJobContract Created
     * @throws ApiError
     */
    public static postApiExports({
        requestBody,
        xApiVersion,
    }: {
        requestBody: QueueExportJobApiRequest,
        xApiVersion?: string,
    }): CancelablePromise<ExportJobContract> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/api/Exports',
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
     * @returns ExportJobContract Created
     * @throws ApiError
     */
    public static postApiV1Exports({
        requestBody,
    }: {
        requestBody: QueueExportJobApiRequest,
    }): CancelablePromise<ExportJobContract> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/api/v1/Exports',
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
     * @returns ExportJobContract OK
     * @throws ApiError
     */
    public static getApiExports({
        exportJobId,
        xApiVersion,
    }: {
        exportJobId: string,
        xApiVersion?: string,
    }): CancelablePromise<ExportJobContract> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/api/Exports/{exportJobId}',
            path: {
                'exportJobId': exportJobId,
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
     * @returns ExportJobContract OK
     * @throws ApiError
     */
    public static getApiV1Exports({
        exportJobId,
    }: {
        exportJobId: string,
    }): CancelablePromise<ExportJobContract> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/api/v1/Exports/{exportJobId}',
            path: {
                'exportJobId': exportJobId,
            },
            errors: {
                401: `Unauthorized`,
                403: `Forbidden`,
                404: `Not Found`,
            },
        });
    }
    /**
     * @returns FileContentResult OK
     * @throws ApiError
     */
    public static getApiExportsDownload({
        exportJobId,
        xApiVersion,
    }: {
        exportJobId: string,
        xApiVersion?: string,
    }): CancelablePromise<FileContentResult> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/api/Exports/{exportJobId}/download',
            path: {
                'exportJobId': exportJobId,
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
     * @returns FileContentResult OK
     * @throws ApiError
     */
    public static getApiV1ExportsDownload({
        exportJobId,
    }: {
        exportJobId: string,
    }): CancelablePromise<FileContentResult> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/api/v1/Exports/{exportJobId}/download',
            path: {
                'exportJobId': exportJobId,
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
