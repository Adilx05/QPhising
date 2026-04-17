/* generated using openapi-typescript-codegen -- do not edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { ProxyDriftValidationStatus } from './ProxyDriftValidationStatus';
export type ProxyContractSyncConflictResponse = {
  status: ProxyDriftValidationStatus;
  message: string | null;
  swaggerLastModifiedUtc?: string | null;
  proxyGeneratedAtUtc?: string | null;
  suggestedRegenerationCommand: string | null;
};

