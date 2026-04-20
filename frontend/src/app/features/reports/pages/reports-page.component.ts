import { CommonModule } from '@angular/common';
import { Component, computed, signal } from '@angular/core';
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
  template: `
    <section class="mb-6 flex flex-wrap items-start justify-between gap-3">
      <div>
        <h1 class="page-title">{{ tx('Rapor Merkezi', 'Report Center') }}</h1>
        <p class="page-subtitle">{{ tx('CSV/PDF formatlarında kurumsal analitik raporlarını dışa aktarın.', 'Export enterprise analytics reports in CSV/PDF formats.') }}</p>
      </div>
      <button pButton type="button" icon="pi pi-refresh" [label]="tx('Yenile', 'Refresh')" [loading]="isBusy()" (click)="refreshPages()"></button>
    </section>

    <section class="surface-card p-5">
      <div class="grid gap-3 lg:grid-cols-2 xl:grid-cols-4">
        <label class="text-sm text-slate-600">
          {{ tx('Kapsam', 'Scope') }}
          <select class="mt-1 w-full rounded-xl border border-slate-300 bg-white px-3 py-2 text-sm" [(ngModel)]="scope" (ngModelChange)="onScopeChanged()">
            <option [ngValue]="TrackingReportScope._0">{{ tx('Genel (Tüm Sayfalar)', 'Global (All Pages)') }}</option>
            <option [ngValue]="TrackingReportScope._1">{{ tx('Seçili Takip Sayfası', 'Selected Tracking Page') }}</option>
          </select>
        </label>

        <label class="text-sm text-slate-600">
          {{ tx('Detay Seviyesi', 'Detail Level') }}
          <select class="mt-1 w-full rounded-xl border border-slate-300 bg-white px-3 py-2 text-sm" [(ngModel)]="detailLevel">
            <option [ngValue]="TrackingReportDetailLevel._0">{{ tx('Genel', 'Summary') }}</option>
            <option [ngValue]="TrackingReportDetailLevel._1">{{ tx('Detaylı', 'Detailed') }}</option>
          </select>
        </label>

        <label class="text-sm text-slate-600">
          {{ tx('Format', 'Format') }}
          <select class="mt-1 w-full rounded-xl border border-slate-300 bg-white px-3 py-2 text-sm" [(ngModel)]="format">
            <option [ngValue]="TrackingReportFormat._0">CSV</option>
            <option [ngValue]="TrackingReportFormat._1">PDF</option>
          </select>
        </label>

        <label class="text-sm text-slate-600">
          {{ tx('Zaman Aralığı', 'Time Range') }}
          <select class="mt-1 w-full rounded-xl border border-slate-300 bg-white px-3 py-2 text-sm" [(ngModel)]="rangePreset">
            <option value="all">{{ tx('Tüm Zamanlar', 'All Time') }}</option>
            <option value="selected">{{ tx('Seçili Aralık', 'Selected Range') }}</option>
            <option value="last7">{{ tx('Son 7 Gün', 'Last 7 Days') }}</option>
            <option value="last30">{{ tx('Son 30 Gün', 'Last 30 Days') }}</option>
          </select>
        </label>
      </div>

      <div class="mt-3 grid gap-3 lg:grid-cols-2" *ngIf="scopeRequiresPage()">
        <label class="text-sm text-slate-600">
          {{ tx('Takip Sayfası', 'Tracking Page') }}
          <select class="mt-1 w-full rounded-xl border border-slate-300 bg-white px-3 py-2 text-sm" [(ngModel)]="trackingPageId" [disabled]="trackingPages().length === 0">
            <option [ngValue]="''">{{ tx('Seçiniz', 'Select') }}</option>
            <option *ngFor="let page of trackingPages()" [ngValue]="page.id">/{{ page.slug || page.title || page.id }}</option>
          </select>
          <span *ngIf="trackingPages().length === 0" class="mt-1 block text-xs text-amber-700">
            {{ tx('Seçilecek takip sayfası bulunamadı. Önce takip sayfası oluşturun veya Yenile butonunu kullanın.', 'No tracking pages available. Create one first or use Refresh.') }}
          </span>
        </label>
        <label class="mt-6 inline-flex items-center gap-2 text-sm text-slate-600">
          <input type="checkbox" [(ngModel)]="excludeBots" />
          {{ tx('Bot trafiğini hariç tut', 'Exclude bot traffic') }}
        </label>
      </div>

      <div class="mt-2 grid gap-3 lg:grid-cols-2">
        <label class="inline-flex items-center gap-2 text-sm text-slate-600">
          <input type="checkbox" [(ngModel)]="includeVisitorBreakdown" />
          {{ tx('Kullanıcı/IP tıklama listesini ekle', 'Include user/IP click list') }}
        </label>
      </div>

      <div class="mt-3 grid gap-3 lg:grid-cols-2" *ngIf="rangePreset === 'selected'">
        <label class="text-sm text-slate-600">
          {{ tx('Başlangıç (UTC)', 'From (UTC)') }}
          <input pInputText class="mt-1 w-full" type="datetime-local" [(ngModel)]="fromUtc" />
        </label>
        <label class="text-sm text-slate-600">
          {{ tx('Bitiş (UTC)', 'To (UTC)') }}
          <input pInputText class="mt-1 w-full" type="datetime-local" [(ngModel)]="toUtc" />
        </label>
      </div>

      <div class="mt-4 flex flex-wrap items-center gap-2">
        <button pButton type="button" icon="pi pi-download" [label]="tx('Raporu Dışa Aktar', 'Export Report')" [disabled]="!canExport() || isBusy()" [loading]="isBusy()" (click)="export()"></button>
      </div>
    </section>

    <section class="surface-card mt-6 p-5">
      <h2 class="text-base font-semibold text-slate-900">{{ tx('Rapor İçeriği Önizleme', 'Report Content Preview') }}</h2>
      <div class="mt-3 grid gap-3 text-sm text-slate-700 md:grid-cols-2">
        <div class="rounded-xl border border-slate-200 bg-slate-50 px-3 py-2">
          <span class="font-medium">{{ tx('Seçilen Kapsam', 'Selected Scope') }}:</span>
          <span class="ml-1">{{ scopeRequiresPage() ? selectedTrackingPageLabel() : tx('Global', 'Global') }}</span>
        </div>
        <div class="rounded-xl border border-slate-200 bg-slate-50 px-3 py-2">
          <span class="font-medium">{{ tx('Kullanıcı Listesi', 'User List') }}:</span>
          <span class="ml-1">{{ includeVisitorBreakdown ? tx('Açık', 'Enabled') : tx('Kapalı', 'Disabled') }}</span>
        </div>
      </div>
      <ul class="mt-3 list-disc space-y-1 pl-5 text-sm text-slate-600">
        <li>{{ tx('Özet seviyede KPI, trend ve dağılım verileri yer alır.', 'Summary level includes KPI, trend, and distribution data.') }}</li>
        <li>{{ tx('Kullanıcı/IP listesi açıldığında ziyaretçi bazlı tıklama sayıları ve kimlik referansları eklenir.', 'When user/IP list is enabled, visitor click counts and identity references are added.') }}</li>
        <li>{{ tx('PDF çıktısı KPI, trend bar-chart, dağılım ve ziyaretçi tablosu bölümleriyle zenginleştirilir.', 'PDF export is enriched with KPI, trend bar chart, distribution, and visitor table sections.') }}</li>
      </ul>
    </section>

    <p *ngIf="feedback()" class="mt-4 rounded-xl border border-blue-100 bg-blue-50 px-3 py-2 text-sm text-blue-800">{{ feedback() }}</p>
  `
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

  protected readonly scopeRequiresPage = computed(() => reportScopeRequiresPage(this.scope));

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
