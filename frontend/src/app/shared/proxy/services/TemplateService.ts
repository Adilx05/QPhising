/* generated using openapi-typescript-codegen -- do not edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { CreateTemplateRequest } from '../models/CreateTemplateRequest';
import type { TemplateResult } from '../models/TemplateResult';
import type { UpdateTemplateRequest } from '../models/UpdateTemplateRequest';
import type { CancelablePromise } from '../core/CancelablePromise';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';
export class TemplateService {
  /**
   * List templates with lifecycle and version metadata.
   * @returns TemplateResult OK
   * @throws ApiError
   */
  public static templateList(): CancelablePromise<Array<TemplateResult>> {
    return __request(OpenAPI, {
      method: 'GET',
      url: '/api/templates',
      errors: {
        400: `Bad Request`,
        401: `Unauthorized`,
        403: `Forbidden`,
        500: `Internal Server Error`,
      },
    });
  }
  /**
   * Create a new draft template.
   * @returns TemplateResult OK
   * @throws ApiError
   */
  public static templateCreate({
    requestBody,
  }: {
    requestBody?: CreateTemplateRequest,
  }): CancelablePromise<TemplateResult> {
    return __request(OpenAPI, {
      method: 'POST',
      url: '/api/templates',
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
   * Get a template by identifier.
   * @returns TemplateResult OK
   * @throws ApiError
   */
  public static templateGetById({
    templateId,
  }: {
    templateId: string,
  }): CancelablePromise<TemplateResult> {
    return __request(OpenAPI, {
      method: 'GET',
      url: '/api/templates/{templateId}',
      path: {
        'templateId': templateId,
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
   * Update mutable fields for an existing draft template.
   * @returns TemplateResult OK
   * @throws ApiError
   */
  public static templateUpdate({
    templateId,
    requestBody,
  }: {
    templateId: string,
    requestBody?: UpdateTemplateRequest,
  }): CancelablePromise<TemplateResult> {
    return __request(OpenAPI, {
      method: 'PUT',
      url: '/api/templates/{templateId}',
      path: {
        'templateId': templateId,
      },
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
   * Delete a template.
   * @returns void
   * @throws ApiError
   */
  public static templateDelete({
    templateId,
  }: {
    templateId: string,
  }): CancelablePromise<void> {
    return __request(OpenAPI, {
      method: 'DELETE',
      url: '/api/templates/{templateId}',
      path: {
        'templateId': templateId,
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
   * Publish a draft template for campaign usage.
   * @returns TemplateResult OK
   * @throws ApiError
   */
  public static templatePublish({
    templateId,
  }: {
    templateId: string,
  }): CancelablePromise<TemplateResult> {
    return __request(OpenAPI, {
      method: 'POST',
      url: '/api/templates/{templateId}/publish',
      path: {
        'templateId': templateId,
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
   * Archive a template to prevent future use.
   * @returns TemplateResult OK
   * @throws ApiError
   */
  public static templateArchive({
    templateId,
  }: {
    templateId: string,
  }): CancelablePromise<TemplateResult> {
    return __request(OpenAPI, {
      method: 'POST',
      url: '/api/templates/{templateId}/archive',
      path: {
        'templateId': templateId,
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
