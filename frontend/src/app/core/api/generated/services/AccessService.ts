/* generated using openapi-typescript-codegen -- do not edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { CancelablePromise } from '../core/CancelablePromise';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';
export class AccessService {
    /**
     * @returns any OK
     * @throws ApiError
     */
    public static getApiAccessAdmin({
        xApiVersion,
    }: {
        xApiVersion?: string,
    }): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/api/access/admin',
            headers: {
                'x-api-version': xApiVersion,
            },
            errors: {
                401: `Unauthorized`,
                403: `Forbidden`,
            },
        });
    }
    /**
     * @returns any OK
     * @throws ApiError
     */
    public static getApiV1AccessAdmin(): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/api/v1/access/admin',
            errors: {
                401: `Unauthorized`,
                403: `Forbidden`,
            },
        });
    }
    /**
     * @returns any OK
     * @throws ApiError
     */
    public static getApiAccessOperator({
        xApiVersion,
    }: {
        xApiVersion?: string,
    }): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/api/access/operator',
            headers: {
                'x-api-version': xApiVersion,
            },
            errors: {
                401: `Unauthorized`,
                403: `Forbidden`,
            },
        });
    }
    /**
     * @returns any OK
     * @throws ApiError
     */
    public static getApiV1AccessOperator(): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/api/v1/access/operator',
            errors: {
                401: `Unauthorized`,
                403: `Forbidden`,
            },
        });
    }
    /**
     * @returns any OK
     * @throws ApiError
     */
    public static getApiAccessViewer({
        xApiVersion,
    }: {
        xApiVersion?: string,
    }): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/api/access/viewer',
            headers: {
                'x-api-version': xApiVersion,
            },
            errors: {
                401: `Unauthorized`,
                403: `Forbidden`,
            },
        });
    }
    /**
     * @returns any OK
     * @throws ApiError
     */
    public static getApiV1AccessViewer(): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/api/v1/access/viewer',
            errors: {
                401: `Unauthorized`,
                403: `Forbidden`,
            },
        });
    }
}
