import { CommonModule } from '@angular/common';
import { Component, computed, signal } from '@angular/core';
import { ButtonModule } from 'primeng/button';
import { ProgressBarModule } from 'primeng/progressbar';
import { TableModule } from 'primeng/table';
import { TagModule } from 'primeng/tag';
import { resolveApiError } from '../../../core/http/api-error-handler';
import {
  CampaignLifecycleState,
  TrackingPagePublishState,
  TrackingVisitTrendBucketWindow,
  TrackingAnalyticsService,
  type CampaignResult,
  type TrackingAnalyticsOverviewResult,
  type TrackingRecentVisitStreamItemResult,
  type TrackingTopPageResult,
  type TrackingVisitTrendPointResult
} from '../../../shared/proxy';
import { listCampaigns } from '../../campaigns/data-access';
import { listTrackingPages } from '../../tracking/data-access';

@Component({
  selector: 'app-dashboard-page',
  standalone: true,
  imports: [CommonModule, ButtonModule, TableModule, ProgressBarModule, TagModule],
  template: `
    <section class="mb-6 flex flex-wrap items-start justify-between gap-3">
      <div>
        <h1 class="page-title">Genel Uygulama Dashboard</h1>
        <p class="page-subtitle">
          Kampanya, tracking ve ziyaret analitiği metrikleri canlı backend verileriyle gösterilir.
        </p>
      </div>
      <button pButton type="button" icon="pi pi-refresh" label="Yenile" [loading]="isBusy()" (click)="refresh()"></button>
    </section>

    <section class="grid gap-4 md:grid-cols-2 xl:grid-cols-4">
      <article class="surface-card p-4">
        <p class="text-xs font-semibold uppercase tracking-wide text-slate-500">Toplam Kampanya</p>
        <p class="mt-2 text-3xl font-semibold text-slate-900">{{ campaigns().length }}</p>
        <p class="mt-1 text-xs text-slate-500">Aktif: {{ activeCampaignCount() }} · Taslak: {{ draftCampaignCount() }}</p>
      </article>

      <article class="surface-card p-4">
        <p class="text-xs font-semibold uppercase tracking-wide text-slate-500">Toplam Tracking Sayfası</p>
        <p class="mt-2 text-3xl font-semibold text-slate-900">{{ trackingPageCount() }}</p>
        <p class="mt-1 text-xs text-slate-500">Yayında: {{ publishedPageCount() }} · Arşiv: {{ archivedPageCount() }}</p>
      </article>

      <article class="surface-card p-4">
        <p class="text-xs font-semibold uppercase tracking-wide text-slate-500">Toplam Ziyaret</p>
        <p class="mt-2 text-3xl font-semibold text-slate-900">{{ summaryTotalVisits() }}</p>
        <p class="mt-1 text-xs text-slate-500">Benzersiz ziyaretçi: {{ summaryUniqueVisitors() }}</p>
      </article>

      <article class="surface-card p-4">
        <p class="text-xs font-semibold uppercase tracking-wide text-slate-500">Dönüşüm Yoğunluğu</p>
        <p class="mt-2 text-3xl font-semibold text-slate-900">{{ uniqueVisitorRate() }}%</p>
        <p class="mt-2 text-xs text-slate-500">Benzersiz ziyaretçi / toplam ziyaret oranı</p>
        <p-progressBar class="mt-2" [value]="uniqueVisitorRate()"></p-progressBar>
      </article>
    </section>

    <section class="mt-6 grid gap-4 xl:grid-cols-3">
      <article class="surface-card p-4 xl:col-span-2">
        <div class="mb-3 flex items-center justify-between gap-2">
          <h2 class="text-base font-semibold text-slate-900">Saatlik Trend Özeti</h2>
          <p-tag severity="contrast" [value]="trendWindowLabel()"></p-tag>
        </div>

        <div *ngIf="trendRows().length > 0; else noTrend" class="space-y-2">
          <div *ngFor="let row of trendRows()" class="rounded-xl border border-slate-200 bg-slate-50/60 px-3 py-2">
            <div class="mb-1 flex items-center justify-between text-xs text-slate-600">
              <span>{{ formatDateTime(row.bucketStartUtc) }}</span>
              <span>{{ row.totalVisits ?? 0 }} ziyaret</span>
            </div>
            <p-progressBar [value]="trendPercent(row)"></p-progressBar>
          </div>
        </div>

        <ng-template #noTrend>
          <p class="text-sm text-slate-500">Trend verisi henüz oluşmadı.</p>
        </ng-template>
      </article>

      <article class="surface-card p-4">
        <h2 class="text-base font-semibold text-slate-900">Kampanya Durumu</h2>
        <div class="mt-3 space-y-3 text-sm">
          <div class="flex items-center justify-between">
            <span class="text-slate-600">Aktif</span>
            <p-tag severity="success" [value]="activeCampaignCount().toString()"></p-tag>
          </div>
          <div class="flex items-center justify-between">
            <span class="text-slate-600">Planlandı</span>
            <p-tag severity="info" [value]="scheduledCampaignCount().toString()"></p-tag>
          </div>
          <div class="flex items-center justify-between">
            <span class="text-slate-600">Duraklatıldı</span>
            <p-tag severity="warn" [value]="pausedCampaignCount().toString()"></p-tag>
          </div>
          <div class="flex items-center justify-between">
            <span class="text-slate-600">Tamamlandı</span>
            <p-tag severity="secondary" [value]="completedCampaignCount().toString()"></p-tag>
          </div>
        </div>
      </article>
    </section>

    <section class="mt-6 grid gap-4 xl:grid-cols-2">
      <article class="surface-card p-4">
        <h2 class="text-base font-semibold text-slate-900">En Çok Ziyaret Alan Sayfalar</h2>
        <p-table class="mt-3" [value]="topPages()" [tableStyle]="{ 'min-width': '100%' }" size="small">
          <ng-template pTemplate="header">
            <tr>
              <th>Sayfa</th>
              <th class="w-28">Toplam</th>
              <th class="w-28">Benzersiz</th>
            </tr>
          </ng-template>
          <ng-template pTemplate="body" let-page>
            <tr>
              <td>
                <p class="font-medium text-slate-900">{{ page.title || page.slug || 'İsimsiz Sayfa' }}</p>
                <p class="text-xs text-slate-500">/{{ page.slug || '-' }}</p>
              </td>
              <td>{{ page.totalVisits ?? 0 }}</td>
              <td>{{ page.uniqueVisitors ?? 0 }}</td>
            </tr>
          </ng-template>
          <ng-template pTemplate="emptymessage">
            <tr><td colspan="3" class="text-sm text-slate-500">Gösterilecek sayfa analitiği bulunamadı.</td></tr>
          </ng-template>
        </p-table>
      </article>

      <article class="surface-card p-4">
        <h2 class="text-base font-semibold text-slate-900">Son Ziyaret Akışı</h2>
        <div class="mt-3 max-h-80 space-y-2 overflow-auto" *ngIf="recentVisits().length > 0; else noRecent">
          <article *ngFor="let visit of recentVisits()" class="rounded-xl border border-slate-200 bg-slate-50/70 px-3 py-2">
            <div class="flex items-start justify-between gap-2">
              <div>
                <p class="text-sm font-medium text-slate-900">/{{ visit.trackingPageSlug || '-' }}</p>
                <p class="text-xs text-slate-600">{{ visit.referrerUrl || 'Doğrudan trafik' }}</p>
              </div>
              <p class="text-xs text-slate-500">{{ formatDateTime(visit.occurredAtUtc) }}</p>
            </div>
            <p class="mt-1 text-xs text-slate-600">{{ visit.userAgent || 'Kullanıcı ajanı yok' }}</p>
          </article>
        </div>

        <ng-template #noRecent>
          <p class="text-sm text-slate-500">Son ziyaret verisi bulunmuyor.</p>
        </ng-template>
      </article>
    </section>

    <p *ngIf="feedback()" class="mt-4 rounded-xl border border-blue-100 bg-blue-50 px-3 py-2 text-sm text-blue-800">
      {{ feedback() }}
    </p>
  `
})
export class DashboardPageComponent {
  protected readonly isBusy = signal(false);
  protected readonly feedback = signal<string | null>(null);
  protected readonly campaigns = signal<CampaignResult[]>([]);
  protected readonly trackingPageCount = signal(0);
  protected readonly publishedPageCount = signal(0);
  protected readonly archivedPageCount = signal(0);
  protected readonly overview = signal<TrackingAnalyticsOverviewResult | null>(null);

  protected readonly topPages = computed<TrackingTopPageResult[]>(() => this.overview()?.topPages ?? []);
  protected readonly recentVisits = computed<TrackingRecentVisitStreamItemResult[]>(() => this.overview()?.recentVisits ?? []);
  protected readonly trendRows = computed<TrackingVisitTrendPointResult[]>(() => this.overview()?.trends ?? []);
  protected readonly summaryTotalVisits = computed(() => this.overview()?.summary?.totalVisits ?? 0);
  protected readonly summaryUniqueVisitors = computed(() => this.overview()?.summary?.uniqueVisitors ?? 0);

  public constructor() {
    void this.refresh();
  }

  protected activeCampaignCount(): number {
    return this.countCampaignsByState(CampaignLifecycleState._2);
  }

  protected draftCampaignCount(): number {
    return this.countCampaignsByState(CampaignLifecycleState._0);
  }

  protected scheduledCampaignCount(): number {
    return this.countCampaignsByState(CampaignLifecycleState._1);
  }

  protected pausedCampaignCount(): number {
    return this.countCampaignsByState(CampaignLifecycleState._3);
  }

  protected completedCampaignCount(): number {
    return this.countCampaignsByState(CampaignLifecycleState._4);
  }

  protected uniqueVisitorRate(): number {
    const total = this.summaryTotalVisits();
    if (total <= 0) {
      return 0;
    }

    return Math.min(100, Math.round((this.summaryUniqueVisitors() / total) * 100));
  }

  protected trendPercent(point: TrackingVisitTrendPointResult): number {
    const maxVisits = Math.max(...this.trendRows().map((item) => item.totalVisits ?? 0), 0);
    if (maxVisits <= 0) {
      return 0;
    }

    return Math.round(((point.totalVisits ?? 0) / maxVisits) * 100);
  }

  protected trendWindowLabel(): string {
    return 'Son 24 Saat';
  }

  protected formatDateTime(value?: string | null): string {
    if (!value) {
      return '-';
    }

    const date = new Date(value);
    if (Number.isNaN(date.getTime())) {
      return '-';
    }

    return new Intl.DateTimeFormat('tr-TR', {
      dateStyle: 'short',
      timeStyle: 'short'
    }).format(date);
  }

  protected async refresh(): Promise<void> {
    this.feedback.set(null);
    this.isBusy.set(true);

    try {
      const [campaigns, trackingPages, overview] = await Promise.all([
        listCampaigns(),
        listTrackingPages(),
        TrackingAnalyticsService.trackingAnalyticsGetOverview({
          trendWindow: TrackingVisitTrendBucketWindow._1,
          topPagesLimit: 6,
          recentVisitLimit: 8,
          excludeBots: true,
          timezoneOffsetMinutes: -new Date().getTimezoneOffset()
        })
      ]);

      this.campaigns.set(campaigns);
      this.trackingPageCount.set(trackingPages.length);
      this.publishedPageCount.set(
        trackingPages.filter((page) => page.publishState === TrackingPagePublishState._1).length
      );
      this.archivedPageCount.set(
        trackingPages.filter((page) => page.publishState === TrackingPagePublishState._2).length
      );
      this.overview.set(overview);
      this.feedback.set('Dashboard verileri başarıyla güncellendi.');
    } catch (error) {
      this.feedback.set(resolveApiError(error).message);
    } finally {
      this.isBusy.set(false);
    }
  }

  private countCampaignsByState(targetState: CampaignLifecycleState): number {
    return this.campaigns().filter((campaign) => campaign.lifecycleState === targetState).length;
  }
}
