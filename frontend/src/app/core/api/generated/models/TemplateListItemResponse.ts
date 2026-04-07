/* generated using openapi-typescript-codegen -- do not edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { TemplateStatus } from './TemplateStatus';
import type { TemplateType } from './TemplateType';
export type TemplateListItemResponse = {
    id: string;
    name: string;
    type: TemplateType;
    status: TemplateStatus;
    version: number | string;
    variables: Array<string>;
};

