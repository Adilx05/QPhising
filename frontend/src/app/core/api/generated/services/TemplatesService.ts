/* generated using openapi-typescript-codegen -- do not edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { ArchiveTemplateResponse } from '../models/ArchiveTemplateResponse';
import type { CreateTemplateRequest } from '../models/CreateTemplateRequest';
import type { CreateTemplateResponse } from '../models/CreateTemplateResponse';
import type { ListTemplatesResponse } from '../models/ListTemplatesResponse';
import type { PublishTemplateResponse } from '../models/PublishTemplateResponse';
import type { TemplateDetailResponse } from '../models/TemplateDetailResponse';
import type { TemplateStatus } from '../models/TemplateStatus';
import type { TemplateType } from '../models/TemplateType';
import type { UpdateTemplateRequest } from '../models/UpdateTemplateRequest';
import type { UpdateTemplateResponse } from '../models/UpdateTemplateResponse';
import type { CancelablePromise } from '../core/CancelablePromise';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';
export class TemplatesService {
    /**
     * @returns ListTemplatesResponse OK
     * @throws ApiError
     */
    public static getApiTemplates({
        status,
        type,
        searchTerm,
        pageNumber,
        pageSize,
        xApiVersion,
    }: {
        status?: TemplateStatus,
        type?: TemplateType,
        searchTerm?: string,
        pageNumber?: number | string,
        pageSize?: number | string,
        xApiVersion?: string,
    }): CancelablePromise<ListTemplatesResponse> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/api/Templates',
            headers: {
                'x-api-version': xApiVersion,
            },
            query: {
                'status': status,
                'type': type,
                'searchTerm': searchTerm,
                'pageNumber': pageNumber,
                'pageSize': pageSize,
            },
            errors: {
                400: `Bad Request`,
                401: `Unauthorized`,
                403: `Forbidden`,
            },
        });
    }
    /**
     * @returns CreateTemplateResponse Created
     * @throws ApiError
     */
    public static postApiTemplates({
        requestBody,
        xApiVersion,
    }: {
        requestBody: CreateTemplateRequest,
        xApiVersion?: string,
    }): CancelablePromise<CreateTemplateResponse> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/api/Templates',
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
     * @returns ListTemplatesResponse OK
     * @throws ApiError
     */
    public static getApiV1Templates({
        status,
        type,
        searchTerm,
        pageNumber,
        pageSize,
    }: {
        status?: TemplateStatus,
        type?: TemplateType,
        searchTerm?: string,
        pageNumber?: number | string,
        pageSize?: number | string,
    }): CancelablePromise<ListTemplatesResponse> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/api/v1/Templates',
            query: {
                'status': status,
                'type': type,
                'searchTerm': searchTerm,
                'pageNumber': pageNumber,
                'pageSize': pageSize,
            },
            errors: {
                400: `Bad Request`,
                401: `Unauthorized`,
                403: `Forbidden`,
            },
        });
    }
    /**
     * @returns CreateTemplateResponse Created
     * @throws ApiError
     */
    public static postApiV1Templates({
        requestBody,
    }: {
        requestBody: CreateTemplateRequest,
    }): CancelablePromise<CreateTemplateResponse> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/api/v1/Templates',
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
     * @returns TemplateDetailResponse OK
     * @throws ApiError
     */
    public static getApiTemplates1({
        templateId,
        xApiVersion,
    }: {
        templateId: string,
        xApiVersion?: string,
    }): CancelablePromise<TemplateDetailResponse> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/api/Templates/{templateId}',
            path: {
                'templateId': templateId,
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
     * @returns UpdateTemplateResponse OK
     * @throws ApiError
     */
    public static putApiTemplates({
        templateId,
        requestBody,
        xApiVersion,
    }: {
        templateId: string,
        requestBody: UpdateTemplateRequest,
        xApiVersion?: string,
    }): CancelablePromise<UpdateTemplateResponse> {
        return __request(OpenAPI, {
            method: 'PUT',
            url: '/api/Templates/{templateId}',
            path: {
                'templateId': templateId,
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
     * @returns TemplateDetailResponse OK
     * @throws ApiError
     */
    public static getApiV1Templates1({
        templateId,
    }: {
        templateId: string,
    }): CancelablePromise<TemplateDetailResponse> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/api/v1/Templates/{templateId}',
            path: {
                'templateId': templateId,
            },
            errors: {
                401: `Unauthorized`,
                403: `Forbidden`,
                404: `Not Found`,
            },
        });
    }
    /**
     * @returns UpdateTemplateResponse OK
     * @throws ApiError
     */
    public static putApiV1Templates({
        templateId,
        requestBody,
    }: {
        templateId: string,
        requestBody: UpdateTemplateRequest,
    }): CancelablePromise<UpdateTemplateResponse> {
        return __request(OpenAPI, {
            method: 'PUT',
            url: '/api/v1/Templates/{templateId}',
            path: {
                'templateId': templateId,
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
     * @returns PublishTemplateResponse OK
     * @throws ApiError
     */
    public static postApiTemplatesPublish({
        templateId,
        xApiVersion,
    }: {
        templateId: string,
        xApiVersion?: string,
    }): CancelablePromise<PublishTemplateResponse> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/api/Templates/{templateId}/publish',
            path: {
                'templateId': templateId,
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
     * @returns PublishTemplateResponse OK
     * @throws ApiError
     */
    public static postApiV1TemplatesPublish({
        templateId,
    }: {
        templateId: string,
    }): CancelablePromise<PublishTemplateResponse> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/api/v1/Templates/{templateId}/publish',
            path: {
                'templateId': templateId,
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
     * @returns ArchiveTemplateResponse OK
     * @throws ApiError
     */
    public static postApiTemplatesArchive({
        templateId,
        xApiVersion,
    }: {
        templateId: string,
        xApiVersion?: string,
    }): CancelablePromise<ArchiveTemplateResponse> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/api/Templates/{templateId}/archive',
            path: {
                'templateId': templateId,
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
     * @returns ArchiveTemplateResponse OK
     * @throws ApiError
     */
    public static postApiV1TemplatesArchive({
        templateId,
    }: {
        templateId: string,
    }): CancelablePromise<ArchiveTemplateResponse> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/api/v1/Templates/{templateId}/archive',
            path: {
                'templateId': templateId,
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
