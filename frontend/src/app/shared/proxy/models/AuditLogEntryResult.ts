/* generated using openapi-typescript-codegen -- do not edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
export type AuditLogEntryResult = {
  id?: string;
  timestampUtc?: string;
  actor?: string | null;
  action?: string | null;
  resource?: string | null;
  outcome?: string | null;
  outcomeCode?: number;
  correlationId?: string | null;
  ipHash?: string | null;
};

