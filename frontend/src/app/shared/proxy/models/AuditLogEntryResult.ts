/* generated using openapi-typescript-codegen -- do not edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
export type AuditLogEntryResult = {
  id?: string;
  timestampUtc?: string;
  actor?: string;
  action?: string;
  resource?: string;
  outcome?: string;
  outcomeCode?: number;
  correlationId?: string;
  ipHash?: string | null;
};
