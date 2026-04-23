import {
  AuditLogPageResult,
  AuditLogSortField,
  AuditService,
  SortDirection
} from '../../../shared/proxy';

export interface AuditLogQueryInput {
  fromUtc?: string;
  toUtc?: string;
  actor?: string;
  endpoint?: string;
  outcomeCode?: number;
  correlationId?: string;
  page: number;
  pageSize: number;
  sortBy: AuditLogSortField;
  sortDirection: SortDirection;
}

export const listAuditLogs = async (input: AuditLogQueryInput): Promise<AuditLogPageResult> =>
  AuditService.auditListLogs({
    fromUtc: input.fromUtc,
    toUtc: input.toUtc,
    actor: input.actor,
    endpoint: input.endpoint,
    outcomeCode: input.outcomeCode,
    correlationId: input.correlationId,
    page: input.page,
    pageSize: input.pageSize,
    sortBy: input.sortBy,
    sortDirection: input.sortDirection
  });
