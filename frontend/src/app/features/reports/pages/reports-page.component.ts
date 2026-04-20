import { CommonModule } from '@angular/common';
import { Component, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { resolveApiError } from '../../../core/http/api-error-handler';
import { UserPreferencesService } from '../../../core/ui/user-preferences.service';
import { listTrackingPages } from '../../tracking/data-access';
import {
  TrackingReportDetailLevel,
  TrackingReportFormat,
  TrackingReportScope,
  type TrackingPageResult
} from '../../../shared/proxy';
import { exportTrackingReport, reportScopeRequiresPage } from '../data-access';

@Component({
  selector: 'app-reports-page',
  standalone: true,
  imports: [CommonModule, FormsModule, ButtonModule, InputTextModule],
  templateUrl: './reports-page.component.html'
})
export class ReportsPageComponent {
  protected readonly TrackingReportScope = TrackingReportScope;
  protected readonly TrackingReportDetailLevel = TrackingReportDetailLevel;
  protected readonly TrackingReportFormat = TrackingReportFormat;

  protected readonly isBusy = signal(false);
  protected readonly feedback = signal<string | null>(null);
  protected readonly trackingPages = signal<TrackingPageResult[]>([]);

  protected scope: TrackingReportScope = TrackingReportScope._0;
  protected detailLevel: TrackingReportDetailLevel = TrackingReportDetailLevel._0;
  protected includeVisitorBreakdown = false;
  protected format: TrackingReportFormat = TrackingReportFormat._0;
  protected rangePreset: 'all' | 'selected' | 'last7' | 'last30' = 'all';
  protected trackingPageId = '';
  protected fromUtc = '';
  protected toUtc = '';
  protected excludeBots = true;

  protected scopeRequiresPage(): boolean {
    return reportScopeRequiresPage(this.scope);
  }

  public constructor(private readonly userPreferencesService: UserPreferencesService) {
    void this.refreshPages();
  }

  protected tx(tr: string, en: string): string {
    return this.userPreferencesService.language() === 'tr' ? tr : en;
  }

  protected canExport(): boolean {
    if (this.scopeRequiresPage() && !this.trackingPageId) {
      return false;
    }

    if (this.rangePreset === 'selected') {
      return Boolean(this.fromUtc) && Boolean(this.toUtc);
    }

    return true;
  }

  protected onScopeChanged(): void {
    if (!this.scopeRequiresPage()) {
      this.trackingPageId = '';
      return;
    }

    if (this.trackingPages().length === 0) {
      void this.refreshPages();
    }
  }

  protected async refreshPages(): Promise<void> {
    this.feedback.set(null);
    this.isBusy.set(true);
    try {
      this.trackingPages.set(await listTrackingPages());
      this.feedback.set(this.tx('Rapor sayfası verileri güncellendi.', 'Report page data refreshed.'));
    } catch (error) {
      this.feedback.set(resolveApiError(error).message);
    } finally {
      this.isBusy.set(false);
    }
  }

  protected async export(): Promise<void> {
    if (!this.canExport()) {
      return;
    }

    this.feedback.set(null);
    this.isBusy.set(true);

    try {
      const [fromUtc, toUtc] = this.resolveDateRange();
      await exportTrackingReport({
        format: this.format,
        scope: this.scope,
        detailLevel: this.includeVisitorBreakdown ? TrackingReportDetailLevel._1 : this.detailLevel,
        trackingPageId: this.scopeRequiresPage() ? this.trackingPageId : undefined,
        fromUtc,
        toUtc,
        excludeBots: this.excludeBots,
        timezoneOffsetMinutes: -new Date().getTimezoneOffset()
      });

      this.feedback.set(this.tx('Rapor başarıyla indirildi.', 'Report downloaded successfully.'));
    } catch (error) {
      this.feedback.set(resolveApiError(error).message);
    } finally {
      this.isBusy.set(false);
    }
  }

  private resolveDateRange(): [string | undefined, string | undefined] {
    if (this.rangePreset === 'all') {
      return [undefined, undefined];
    }

    if (this.rangePreset === 'selected') {
      return [this.toIsoOrUndefined(this.fromUtc), this.toIsoOrUndefined(this.toUtc)];
    }

    const now = new Date();
    const from = new Date(now);
    from.setUTCDate(now.getUTCDate() - (this.rangePreset === 'last7' ? 7 : 30));
    return [from.toISOString(), now.toISOString()];
  }

  private toIsoOrUndefined(value: string): string | undefined {
    if (!value) {
      return undefined;
    }

    const date = new Date(value);
    if (Number.isNaN(date.getTime())) {
      return undefined;
    }

    return date.toISOString();
  }

  protected selectedTrackingPageLabel(): string {
    const selectedPage = this.trackingPages().find(page => page.id === this.trackingPageId);
    if (!selectedPage) {
      return this.tx('Seçilmedi', 'Not selected');
    }

    return `/${selectedPage.slug || selectedPage.title || selectedPage.id}`;
  }
}
