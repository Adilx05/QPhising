/* generated using openapi-typescript-codegen -- do not edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { ExportFormat } from './ExportFormat';
import type { ExportJobStatus } from './ExportJobStatus';
import type { ExportType } from './ExportType';
export type ExportJobContract = {
    id: string;
    ownerUserId: string;
    exportType: ExportType;
    format: ExportFormat;
    status: ExportJobStatus;
    requestedAt: string;
    queuedAt: string | null;
    processingStartedAt: string | null;
    completedAt: string | null;
    failedAt: string | null;
    canceledAt: string | null;
    expiresAt: string | null;
    fileName: string | null;
    storagePath: string | null;
    contentType: string | null;
    fileSizeBytes: number | string | null;
    errorMessage: string | null;
    correlationId: string | null;
};

