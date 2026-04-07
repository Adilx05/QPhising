/* generated using openapi-typescript-codegen -- do not edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { ExportFormat } from './ExportFormat';
import type { ExportType } from './ExportType';
export type QueueExportJobApiRequest = {
    exportType: ExportType;
    format: ExportFormat;
    correlationId?: string | null;
};

