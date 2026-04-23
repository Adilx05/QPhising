import { CommonModule } from '@angular/common';
import { Component, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { resolveApiError } from '../../../core/http/api-error-handler';
import { UserPreferencesService } from '../../../core/ui/user-preferences.service';
import {
  AuditLogEntryResult,
  AuditLogSortField,
  SortDirection
} from '../../../shared/proxy';
import { listAuditLogs } from '../data-access';

@Component({
  selector: 'app-audit-logs-page',
  standalone: true,
  imports: [CommonModule, FormsModule, ButtonModule, InputTextModule],
  templateUrl: './audit-logs-page.component.html'
})
export class AuditLogsPageComponent {
  protected readonly logs = signal<AuditLogEntryResult[]>([]);
  protected readonly selectedLog = signal<AuditLogEntryResult | null>(null);
  protected readonly isBusy = signal(false);
  protected readonly feedback = signal<string | null>(null);
  protected readonly totalCount = signal(0);

  protected fromUtc = '';
  protected toUtc = '';
  protected actor = '';
  protected endpoint = '';
  protected correlationId = '';
  protected outcomeCode = '';
  protected page = 1;
  protected pageSize = 25;
  protected sortBy: AuditLogSortField = AuditLogSortField._0;
  protected sortDirection: SortDirection = SortDirection._0;

  protected readonly sortByOptions: Array<{ value: AuditLogSortField; label: string }> = [
    { value: AuditLogSortField._0, label: 'Timestamp' },
    { value: AuditLogSortField._1, label: 'Actor' },
    { value: AuditLogSortField._2, label: 'Action' },
    { value: AuditLogSortField._3, label: 'Resource' },
    { value: AuditLogSortField._4, label: 'Outcome Code' }
  ];

  public constructor(private readonly userPreferencesService: UserPreferencesService) {
    void this.search();
  }

  protected tx(tr: string, en: string): string {
    return this.userPreferencesService.language() === 'tr' ? tr : en;
  }

  protected async search(page = 1): Promise<void> {
    this.isBusy.set(true);
    this.feedback.set(null);

    try {
      this.page = page;
      const response = await listAuditLogs({
        fromUtc: this.toIsoOrUndefined(this.fromUtc),
        toUtc: this.toIsoOrUndefined(this.toUtc),
        actor: this.actor || undefined,
        endpoint: this.endpoint || undefined,
        correlationId: this.correlationId || undefined,
        outcomeCode: this.toNumberOrUndefined(this.outcomeCode),
        page: this.page,
        pageSize: this.pageSize,
        sortBy: this.sortBy,
        sortDirection: this.sortDirection
      });

      this.logs.set(response.items ?? []);
      this.totalCount.set(response.totalCount ?? 0);
    } catch (error) {
      this.feedback.set(resolveApiError(error).message);
    } finally {
      this.isBusy.set(false);
    }
  }

  protected clearFilters(): void {
    this.fromUtc = '';
    this.toUtc = '';
    this.actor = '';
    this.endpoint = '';
    this.correlationId = '';
    this.outcomeCode = '';
    this.page = 1;
    void this.search(1);
  }

  protected openDetails(entry: AuditLogEntryResult): void {
    this.selectedLog.set(entry);
  }

  protected closeDetails(): void {
    this.selectedLog.set(null);
  }

  protected nextPage(): void {
    if ((this.page * this.pageSize) >= this.totalCount()) {
      return;
    }

    void this.search(this.page + 1);
  }

  protected previousPage(): void {
    if (this.page <= 1) {
      return;
    }

    void this.search(this.page - 1);
  }

  private toIsoOrUndefined(value: string): string | undefined {
    if (!value) {
      return undefined;
    }

    const date = new Date(value);
    return Number.isNaN(date.getTime()) ? undefined : date.toISOString();
  }

  private toNumberOrUndefined(value: string): number | undefined {
    if (!value) {
      return undefined;
    }

    const numberValue = Number(value);
    return Number.isInteger(numberValue) ? numberValue : undefined;
  }
}
