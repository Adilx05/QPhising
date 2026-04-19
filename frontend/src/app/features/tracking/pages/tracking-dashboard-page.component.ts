import { CommonModule } from '@angular/common';
import { Component, computed, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { resolveApiError } from '../../../core/http/api-error-handler';
import { AuthSessionService } from '../../../core/auth/auth-session';
import { UserPreferencesService } from '../../../core/ui/user-preferences.service';
import {
  TrackingPagePublishState,
  type TrackingPageAnalyticsResult,
  type TrackingPageResult,
  type TrackingRecentVisitResult,
  type TrackingVisitTrendPointResult,
  type TemplateResult,
  IpAddressHashPolicy
} from '../../../shared/proxy';
import {
  archiveTrackingPage,
  createTrackingPage,
  deleteTrackingPage,
  getTrackingPageAnalytics,
  listTrackingPages,
  publishTrackingPage,
  updateTrackingPage,
  type UpsertTrackingPageInput
} from '../data-access';
import { listTemplates } from '../../templates/data-access';

interface TrackingPageDraft extends UpsertTrackingPageInput {}

interface TrackingGridQuery {
  search: string;
  status: 'all' | 'draft' | 'published' | 'archived';
  sortBy: 'updated' | 'created' | 'slug' | 'title';
  sortDirection: 'asc' | 'desc';
  page: number;
  pageSize: number;
}

interface AnalyticsFilters {
  fromUtc: string;
  toUtc: string;
  sourceReferrerFilter: string;
  userAgentFilter: string;
  trendBucketSizeMinutes: number;
  recentVisitLimit: number;
}

@Component({
  selector: 'app-tracking-dashboard-page',
  standalone: true,
  imports: [CommonModule, FormsModule, ButtonModule, InputTextModule],
  template: `
    <section class="mb-6">
      <h1 class="page-title">{{ tx('Takip Analitik Gösterge Paneli', 'Tracking Analytics Dashboard') }}</h1>
      <p class="page-subtitle">
        {{ tx('Takip sayfalarını, yayın yaşam döngüsünü ve ziyaret analitiğini yönetin.', 'Manage tracking pages, publish lifecycle, and visit analytics.') }}
      </p>
    </section>

    <section class="grid gap-4 md:grid-cols-2 xl:grid-cols-4">
      <article class="surface-card p-4">
        <p class="text-xs font-semibold uppercase text-slate-500">{{ tx('Toplam Sayfa', 'Total Pages') }}</p>
        <p class="mt-2 text-2xl font-semibold text-slate-900">{{ trackingPages().length }}</p>
      </article>

      <article class="surface-card p-4">
        <p class="text-xs font-semibold uppercase text-slate-500">{{ tx('Yayımlanan Sayfalar', 'Published Pages') }}</p>
        <p class="mt-2 text-2xl font-semibold text-emerald-700">{{ publishedCount() }}</p>
      </article>

      <article class="surface-card p-4">
        <p class="text-xs font-semibold uppercase text-slate-500">{{ tx('Taslak Sayfalar', 'Draft Pages') }}</p>
        <p class="mt-2 text-2xl font-semibold text-amber-700">{{ draftCount() }}</p>
      </article>

      <article class="surface-card p-4">
        <p class="text-xs font-semibold uppercase text-slate-500">{{ tx('Arşivlenen Sayfalar', 'Archived Pages') }}</p>
        <p class="mt-2 text-2xl font-semibold text-slate-700">{{ archivedCount() }}</p>
      </article>
    </section>

    <section class="surface-card mt-6 p-5">
      <h2 class="text-base font-semibold text-slate-900">{{ tx('Takip Sayfaları Tablosu', 'Tracking Pages Grid') }}</h2>

      <div class="mt-4 grid gap-3 xl:grid-cols-5">
        <input
          pInputText
          class="w-full xl:col-span-2"
          [placeholder]="tx('Kısa ad/başlık/sahip ile ara', 'Search by slug/title/owner')"
          [ngModel]="gridQuery().search"
          (ngModelChange)="setGridFilter('search', $event)"
        />

        <select class="rounded-xl border border-slate-300 bg-white px-3 py-2 text-sm" [ngModel]="gridQuery().status" (ngModelChange)="setGridFilter('status', $event)">
          <option value="all">{{ tx('Tüm durumlar', 'All status') }}</option>
          <option value="draft">{{ tx('Taslak', 'Draft') }}</option>
          <option value="published">{{ tx('Yayımlandı', 'Published') }}</option>
          <option value="archived">{{ tx('Arşivlendi', 'Archived') }}</option>
        </select>

        <select class="rounded-xl border border-slate-300 bg-white px-3 py-2 text-sm" [ngModel]="gridQuery().sortBy" (ngModelChange)="setGridFilter('sortBy', $event)">
          <option value="updated">{{ tx('Sıralama: Güncellenen', 'Sort: Updated') }}</option>
          <option value="created">{{ tx('Sıralama: Oluşturulan', 'Sort: Created') }}</option>
          <option value="slug">{{ tx('Sıralama: Kısa ad', 'Sort: Slug') }}</option>
          <option value="title">{{ tx('Sıralama: Başlık', 'Sort: Title') }}</option>
        </select>

        <button pButton type="button" icon="pi pi-refresh" [label]="tx('Yenile', 'Refresh')" [loading]="isBusy()" (click)="refresh()"></button>
      </div>

      <div class="mt-4 flex flex-wrap items-center justify-between gap-2 rounded-xl border border-slate-100 bg-slate-50/70 px-3 py-2">
        <div class="flex flex-wrap items-center gap-2">
        <button pButton type="button" size="small" [label]="tx('Artan', 'Asc')" severity="secondary" [outlined]="gridQuery().sortDirection === 'desc'" (click)="setGridFilter('sortDirection', 'asc')"></button>
        <button pButton type="button" size="small" [label]="tx('Azalan', 'Desc')" severity="secondary" [outlined]="gridQuery().sortDirection === 'asc'" (click)="setGridFilter('sortDirection', 'desc')"></button>
        <span class="text-xs font-medium text-slate-600">{{ tx('Sayfa', 'Page') }} {{ gridQuery().page }} / {{ maxPage() }}</span>
        </div>
        <div class="flex items-center gap-2">
        <button pButton type="button" size="small" icon="pi pi-angle-left" [disabled]="gridQuery().page <= 1" (click)="changePage(-1)"></button>
        <button pButton type="button" size="small" icon="pi pi-angle-right" [disabled]="gridQuery().page >= maxPage()" (click)="changePage(1)"></button>
        </div>
      </div>

      <div class="mt-4 grid gap-3">
        <article *ngFor="let page of pagedTrackingPages()" class="rounded-2xl border border-slate-200 p-4">
          <div class="flex flex-wrap items-start justify-between gap-3">
            <div>
              <h3 class="text-base font-semibold text-slate-900">{{ page.title || page.slug || tx('Başlıksız Sayfa', 'Untitled Page') }}</h3>
              <p class="text-sm text-slate-500">{{ tx('Kısa ad', 'Slug') }}: {{ page.slug || '-' }} · {{ tx('Sahip', 'Owner') }}: {{ page.ownerId || '-' }}</p>
              <p class="mt-1 text-xs text-slate-500">{{ tx('Güncellenme', 'Updated') }}: {{ page.updatedAtUtc || '-' }}</p>
            </div>

            <span class="status-chip" [ngClass]="pageStateChip(page.publishState)">{{ pageStateLabel(page.publishState) }}</span>
          </div>

          <div class="mt-4 flex flex-wrap gap-2">
            <button pButton type="button" size="small" [label]="tx('Seç', 'Select')" [outlined]="selectedPageId() !== page.id" (click)="selectPage(page.id)"></button>
            <button pButton type="button" size="small" severity="success" [label]="tx('Yayımla', 'Publish')" [disabled]="!canOperate() || isBusy() || !page.id" (click)="publish(page.id)"></button>
            <button pButton type="button" size="small" severity="secondary" [label]="tx('Arşivle', 'Archive')" [disabled]="!canOperate() || isBusy() || !page.id" (click)="archive(page.id)"></button>
            <button pButton type="button" size="small" severity="danger" [label]="tx('Sil', 'Delete')" [disabled]="!canOperate() || isBusy() || !page.id" (click)="remove(page.id)"></button>
          </div>
        </article>

        <article *ngIf="pagedTrackingPages().length === 0" class="rounded-xl border border-dashed border-slate-300 bg-slate-50 px-4 py-6 text-sm text-slate-500">
          {{ tx('Mevcut filtre için takip sayfası bulunamadı.', 'No tracking page found for current filter.') }}
        </article>
      </div>
    </section>

    <section class="mt-6 grid gap-4 2xl:grid-cols-2">
      <article class="surface-card p-5">
        <h2 class="text-base font-semibold text-slate-900">{{ tx('Takip Sayfası Düzenleyici', 'Tracking Page Editor') }}</h2>
        <p class="mt-1 text-sm text-slate-500">{{ tx('Kısa ad, metadata ve ayarları oluşturun/güncelleyin; yayın yaşam döngüsünü yönetin.', 'Create or update slug/metadata/settings and manage publish lifecycle.') }}</p>

        <div class="mt-4 grid gap-3">
          <input pInputText class="w-full" placeholder="Slug" [(ngModel)]="editorDraft.slug" />
          <input pInputText class="w-full" placeholder="Title" [(ngModel)]="editorDraft.title" />
          <input pInputText class="w-full" placeholder="Description" [(ngModel)]="editorDraft.description" />
          <input pInputText class="w-full" [placeholder]="tx('Sahip Kimliği', 'Owner Id')" [(ngModel)]="editorDraft.ownerId" />
          <select class="rounded-xl border border-slate-300 bg-white px-3 py-2 text-sm" [(ngModel)]="editorDraft.templateId">
            <option [ngValue]="''">{{ tx('Şablon yok', 'No template') }}</option>
            <option *ngFor="let templateItem of templates()" [ngValue]="templateItem.id">{{ templateItem.name || templateItem.id }}</option>
          </select>
          <input pInputText class="w-full" type="number" min="1" [placeholder]="tx('Saklama Gün Sayısı', 'Retention Days')" [(ngModel)]="editorDraft.retentionDays" />
        </div>

        <div class="mt-3 grid gap-2 sm:grid-cols-3">
          <label class="flex items-center gap-2 rounded-lg border border-slate-200 px-3 py-2 text-sm">
            <input type="checkbox" [(ngModel)]="editorDraft.captureIpAddress" /> {{ tx('IP Kaydet', 'Capture IP') }}
          </label>
          <label class="flex items-center gap-2 rounded-lg border border-slate-200 px-3 py-2 text-sm">
            <span>{{ tx('IP Saklama', 'IP Storage') }}</span>
            <select class="rounded border border-slate-300 bg-white px-2 py-1 text-xs" [(ngModel)]="editorDraft.ipAddressHashPolicy" [disabled]="!editorDraft.captureIpAddress">
              <option [ngValue]="IpAddressHashPolicy._1">Plain</option>
              <option [ngValue]="IpAddressHashPolicy._2">SHA-256</option>
            </select>
          </label>
          <label class="flex items-center gap-2 rounded-lg border border-slate-200 px-3 py-2 text-sm">
            <input type="checkbox" [(ngModel)]="editorDraft.enableBotFiltering" /> {{ tx('Bot Filtresi', 'Bot Filter') }}
          </label>
          <label class="flex items-center gap-2 rounded-lg border border-slate-200 px-3 py-2 text-sm">
            <input type="checkbox" [(ngModel)]="editorDraft.captureUtmParameters" /> {{ tx('UTM Kaydet', 'Capture UTM') }}
          </label>
        </div>

        <div class="mt-4 flex flex-wrap gap-2">
          <button pButton type="button" [label]="tx('Oluştur', 'Create')" icon="pi pi-plus" [disabled]="!canOperate() || isBusy()" (click)="create()"></button>
          <button pButton type="button" [label]="tx('Güncelle', 'Update')" severity="secondary" [disabled]="!canOperate() || isBusy() || !selectedPageId()" (click)="save()"></button>
          <button pButton type="button" [label]="tx('Sıfırla', 'Reset')" severity="contrast" [disabled]="isBusy()" (click)="resetEditor()"></button>
        </div>
      </article>

      <article class="surface-card p-5">
        <h2 class="text-base font-semibold text-slate-900">{{ tx('Analitik Detayı', 'Analytics Detail') }}</h2>
        <p class="mt-1 text-sm text-slate-500">{{ tx('Bir sayfa seçin ve özet, trend ve son olaylar için filtre uygulayın.', 'Select a page and apply filters for summary, trends and recent events.') }}</p>

        <div class="mt-4 grid gap-3 lg:grid-cols-2">
          <select class="rounded-xl border border-slate-300 bg-white px-3 py-2 text-sm" [ngModel]="selectedPageId()" (ngModelChange)="selectPage($event)">
            <option [ngValue]="null">{{ tx('Takip sayfası seçin', 'Choose tracking page') }}</option>
            <option *ngFor="let page of trackingPages()" [ngValue]="page.id">{{ page.slug || page.title || page.id }}</option>
          </select>

          <select class="rounded-xl border border-slate-300 bg-white px-3 py-2 text-sm" [(ngModel)]="analyticsFilters.trendBucketSizeMinutes">
            <option [ngValue]="15">{{ tx('15 dk kovalar', '15 min buckets') }}</option>
            <option [ngValue]="30">{{ tx('30 dk kovalar', '30 min buckets') }}</option>
            <option [ngValue]="60">{{ tx('60 dk kovalar', '60 min buckets') }}</option>
            <option [ngValue]="180">{{ tx('3 saat kovalar', '3 hour buckets') }}</option>
          </select>

          <input pInputText class="w-full" type="datetime-local" [(ngModel)]="analyticsFilters.fromUtc" />
          <input pInputText class="w-full" type="datetime-local" [(ngModel)]="analyticsFilters.toUtc" />
          <input pInputText class="w-full" [placeholder]="tx('Yönlendiren/kaynak filtresi', 'Referrer/source filter')" [(ngModel)]="analyticsFilters.sourceReferrerFilter" />
          <input pInputText class="w-full" [placeholder]="tx('Cihaz/Kullanıcı Aracı filtresi', 'Device/User-Agent filter')" [(ngModel)]="analyticsFilters.userAgentFilter" />
        </div>

        <div class="mt-3 flex flex-wrap items-center gap-2">
          <button pButton type="button" [label]="tx('Analitiği Yükle', 'Load Analytics')" icon="pi pi-chart-line" [disabled]="isBusy() || !selectedPageId()" (click)="loadAnalytics()"></button>
          <label class="inline-flex items-center gap-2 text-xs text-slate-600">
            {{ tx('Son kayıt limiti', 'Recent limit') }}
            <input pInputText class="w-24" type="number" min="5" max="200" [(ngModel)]="analyticsFilters.recentVisitLimit" />
          </label>
        </div>

        <div class="mt-4 grid gap-3 sm:grid-cols-3" *ngIf="analytics() as data">
          <div class="rounded-xl border border-slate-200 bg-slate-50/80 px-3 py-3">
            <p class="text-xs uppercase text-slate-500">{{ tx('Toplam Ziyaret', 'Total Visits') }}</p>
            <p class="mt-1 text-xl font-semibold text-slate-900">{{ data.summary?.totalVisits ?? 0 }}</p>
          </div>
          <div class="rounded-xl border border-slate-200 bg-slate-50/80 px-3 py-3">
            <p class="text-xs uppercase text-slate-500">{{ tx('Benzersiz Ziyaretçi', 'Unique Visitors') }}</p>
            <p class="mt-1 text-xl font-semibold text-slate-900">{{ data.summary?.uniqueVisitors ?? 0 }}</p>
          </div>
          <div class="rounded-xl border border-slate-200 bg-slate-50/80 px-3 py-3">
            <p class="text-xs uppercase text-slate-500">{{ tx('Son Ziyaret', 'Last Visit') }}</p>
            <p class="mt-1 text-xs font-medium text-slate-700">{{ data.summary?.lastVisitAtUtc || '-' }}</p>
          </div>
        </div>

        <div class="mt-4 grid gap-4 xl:grid-cols-2" *ngIf="analytics() as data">
          <div class="rounded-xl border border-slate-200 p-3">
            <p class="text-xs font-semibold uppercase text-slate-500">Trend Chart ({{ tx('Toplam Ziyaret', 'Total Visits') }})</p>
            <div class="mt-3 space-y-2" *ngIf="filteredTrends().length > 0; else noTrends">
              <div *ngFor="let point of filteredTrends()" class="flex items-center gap-2">
                <span class="w-24 shrink-0 text-[11px] text-slate-500">{{ point.bucketStartUtc | date: 'MM-dd HH:mm' }}</span>
                <div class="h-2 flex-1 rounded-full bg-slate-100">
                  <div class="h-2 rounded-full bg-blue-500" [style.width.%]="trendBarWidth(point)"></div>
                </div>
                <span class="w-10 text-right text-xs font-semibold text-slate-700">{{ point.totalVisits ?? 0 }}</span>
              </div>
            </div>
            <ng-template #noTrends>
              <p class="mt-2 text-sm text-slate-500">{{ tx('Aktif filtreler için trend verisi yok.', 'No trend data for active filters.') }}</p>
            </ng-template>
          </div>

          <div class="rounded-xl border border-slate-200 p-3">
            <p class="text-xs font-semibold uppercase text-slate-500">{{ tx('Ziyaret Dağılımı (Yönlendiren)', 'Visit Distribution (Referrer)') }}</p>
            <div class="mt-3 space-y-2" *ngIf="sourceDistribution().length > 0; else noSource">
              <div *ngFor="let bucket of sourceDistribution()" class="flex items-center gap-2">
                <span class="w-28 truncate text-[11px] text-slate-600" [title]="bucket.label">{{ bucket.label }}</span>
                <div class="h-2 flex-1 rounded-full bg-slate-100">
                  <div class="h-2 rounded-full bg-emerald-500" [style.width.%]="distributionWidth(bucket.value, sourceDistribution())"></div>
                </div>
                <span class="w-8 text-right text-xs font-semibold text-slate-700">{{ bucket.value }}</span>
              </div>
            </div>
            <ng-template #noSource>
              <p class="mt-2 text-sm text-slate-500">{{ tx('Aktif filtreler için yönlendiren dağılımı yok.', 'No referrer distribution for active filters.') }}</p>
            </ng-template>
          </div>

          <div class="rounded-xl border border-slate-200 p-3 xl:col-span-2">
            <p class="text-xs font-semibold uppercase text-slate-500">{{ tx('Ziyaret Dağılımı (Cihaz / Kullanıcı Aracı)', 'Visit Distribution (Device / User-Agent)') }}</p>
            <div class="mt-3 grid gap-2 lg:grid-cols-2" *ngIf="deviceDistribution().length > 0; else noDevice">
              <div *ngFor="let bucket of deviceDistribution()" class="flex items-center gap-2">
                <span class="w-28 truncate text-[11px] text-slate-600" [title]="bucket.label">{{ bucket.label }}</span>
                <div class="h-2 flex-1 rounded-full bg-slate-100">
                  <div class="h-2 rounded-full bg-violet-500" [style.width.%]="distributionWidth(bucket.value, deviceDistribution())"></div>
                </div>
                <span class="w-8 text-right text-xs font-semibold text-slate-700">{{ bucket.value }}</span>
              </div>
            </div>
            <ng-template #noDevice>
              <p class="mt-2 text-sm text-slate-500">{{ tx('Aktif filtreler için cihaz dağılımı yok.', 'No device distribution for active filters.') }}</p>
            </ng-template>
          </div>
        </div>

        <div class="mt-4 rounded-xl border border-slate-200 p-3" *ngIf="analytics() as data">
          <p class="text-xs font-semibold uppercase text-slate-500">{{ tx('Son Olaylar', 'Recent Events') }}</p>
          <div class="mt-2 max-h-72 space-y-2 overflow-auto" *ngIf="filteredRecentVisits(data.recentVisits || []).length > 0; else noRecent">
            <article *ngFor="let visit of filteredRecentVisits(data.recentVisits || [])" class="rounded-lg border border-slate-100 bg-slate-50/60 p-2 text-xs text-slate-700">
              <p class="font-medium text-slate-900">{{ visit.occurredAtUtc || '-' }}</p>
              <p>{{ tx('Yönlendiren', 'Referrer') }}: {{ visit.referrerUrl || tx('doğrudan', 'direct') }}</p>
              <p>{{ tx('Kullanıcı Aracı', 'User-Agent') }}: {{ visit.userAgent || '-' }}</p>
              <p>{{ tx('Oturum', 'Session') }}: {{ visit.sessionId || '-' }}</p>
            </article>
          </div>
          <ng-template #noRecent>
            <p class="mt-2 text-sm text-slate-500">{{ tx('Aktif filtreler için son ziyaret yok.', 'No recent visits for active filters.') }}</p>
          </ng-template>
        </div>
      </article>
    </section>

    <p *ngIf="feedback()" class="mt-4 rounded-xl border border-blue-100 bg-blue-50 px-3 py-2 text-sm text-blue-800">
      {{ feedback() }}
    </p>
  `
})
export class TrackingDashboardPageComponent {
  protected readonly IpAddressHashPolicy = IpAddressHashPolicy;
  protected readonly isBusy = signal(false);
  protected readonly feedback = signal<string | null>(null);
  protected readonly trackingPages = signal<TrackingPageResult[]>([]);
  protected readonly templates = signal<TemplateResult[]>([]);
  protected readonly selectedPageId = signal<string | null>(null);
  protected readonly analytics = signal<TrackingPageAnalyticsResult | null>(null);
  protected readonly canOperate = computed(() => this.authSessionService.hasRequiredRole('Operator'));

  protected readonly gridQuery = signal<TrackingGridQuery>({
    search: '',
    status: 'all',
    sortBy: 'updated',
    sortDirection: 'desc',
    page: 1,
    pageSize: 6
  });

  protected readonly editorDraft: TrackingPageDraft = {
    slug: '',
    title: '',
    description: '',
    customHtmlContent: '',
    validFromUtc: '',
    validUntilUtc: '',
    ownerId: '',
    templateId: '',
    retentionDays: 30,
    captureIpAddress: true,
    ipAddressHashPolicy: IpAddressHashPolicy._2,
    enableBotFiltering: true,
    captureUtmParameters: true
  };

  protected readonly analyticsFilters: AnalyticsFilters = {
    fromUtc: '',
    toUtc: '',
    sourceReferrerFilter: '',
    userAgentFilter: '',
    trendBucketSizeMinutes: 60,
    recentVisitLimit: 25
  };

  protected readonly filteredTrackingPages = computed(() => {
    const pages = this.trackingPages();
    const query = this.gridQuery();
    const search = query.search.trim().toLowerCase();

    return pages
      .filter((item) => this.matchesStatus(item, query.status))
      .filter((item) => {
        if (search.length === 0) {
          return true;
        }

        return [item.slug, item.title, item.ownerId]
          .map((value) => (value ?? '').toLowerCase())
          .some((value) => value.includes(search));
      })
      .sort((left, right) => this.comparePages(left, right, query.sortBy, query.sortDirection));
  });

  protected readonly pagedTrackingPages = computed(() => {
    const query = this.gridQuery();
    const pages = this.filteredTrackingPages();
    const start = (query.page - 1) * query.pageSize;
    return pages.slice(start, start + query.pageSize);
  });

  protected readonly maxPage = computed(() => {
    const total = this.filteredTrackingPages().length;
    const pageSize = this.gridQuery().pageSize;
    return Math.max(1, Math.ceil(total / pageSize));
  });

  protected readonly publishedCount = computed(
    () => this.trackingPages().filter((item) => item.publishState === TrackingPagePublishState._1).length
  );

  protected readonly draftCount = computed(
    () => this.trackingPages().filter((item) => item.publishState === TrackingPagePublishState._0).length
  );

  protected readonly archivedCount = computed(
    () => this.trackingPages().filter((item) => item.publishState === TrackingPagePublishState._2).length
  );

  protected readonly filteredTrends = computed(() => {
    const trends = this.analytics()?.trends ?? [];
    return trends.filter((trend) => this.isWithinDateRange(trend.bucketStartUtc));
  });

  protected readonly sourceDistribution = computed(() =>
    this.toDistribution(
      this.filteredRecentVisits(this.analytics()?.recentVisits ?? []),
      (visit) => this.normalizeDistributionLabel(visit.referrerUrl, this.tx('doğrudan', 'direct'))
    )
  );

  protected readonly deviceDistribution = computed(() =>
    this.toDistribution(
      this.filteredRecentVisits(this.analytics()?.recentVisits ?? []),
      (visit) => this.normalizeDistributionLabel(this.toDeviceBucket(visit.userAgent), this.tx('bilinmeyen-cihaz', 'unknown-device'))
    )
  );

  public constructor(
    private readonly authSessionService: AuthSessionService,
    private readonly userPreferencesService: UserPreferencesService
  ) {
    void this.refresh();
  }

  protected tx(tr: string, en: string): string {
    return this.userPreferencesService.language() === 'tr' ? tr : en;
  }

  protected async refresh(): Promise<void> {
    await this.execute(async () => {
      this.trackingPages.set(await listTrackingPages());
      this.feedback.set(this.tx('Takip sayfası listesi güncellendi.', 'Tracking page list refreshed.'));
      this.ensurePageBounds();
      this.autoSelectFirstPage();
    });
  }

  protected setGridFilter<K extends keyof TrackingGridQuery>(field: K, value: TrackingGridQuery[K]): void {
    this.gridQuery.set({
      ...this.gridQuery(),
      [field]: value,
      page: field === 'page' ? (value as number) : 1
    });
    this.ensurePageBounds();
  }

  protected changePage(delta: number): void {
    const nextPage = Math.min(this.maxPage(), Math.max(1, this.gridQuery().page + delta));
    this.gridQuery.set({ ...this.gridQuery(), page: nextPage });
  }

  protected async create(): Promise<void> {
    const input = this.toValidatedInput();
    if (!input) {
      return;
    }

    await this.execute(async () => {
      const created = await createTrackingPage(input);
      this.trackingPages.set([created, ...this.trackingPages()]);
      this.selectPage(created.id ?? null);
      this.feedback.set(this.tx('Takip sayfası oluşturuldu.', 'Tracking page created.'));
    });
  }

  protected async save(): Promise<void> {
    const pageId = this.selectedPageId();
    if (!pageId) {
      this.feedback.set(this.tx('Güncelleme için önce bir takip sayfası seçin.', 'Select a tracking page before updating.'));
      return;
    }

    const input = this.toValidatedInput();
    if (!input) {
      return;
    }

    await this.execute(async () => {
      const updated = await updateTrackingPage(pageId, input);
      this.applyUpdatedPage(updated);
      this.feedback.set(this.tx('Takip sayfası güncellendi.', 'Tracking page updated.'));
    });
  }

  protected async publish(pageId: string | undefined): Promise<void> {
    if (!pageId) {
      return;
    }

    await this.execute(async () => {
      this.applyUpdatedPage(await publishTrackingPage(pageId));
      this.feedback.set(this.tx('Takip sayfası yayımlandı.', 'Tracking page published.'));
    });
  }

  protected async archive(pageId: string | undefined): Promise<void> {
    if (!pageId) {
      return;
    }

    await this.execute(async () => {
      this.applyUpdatedPage(await archiveTrackingPage(pageId));
      this.feedback.set(this.tx('Takip sayfası arşivlendi.', 'Tracking page archived.'));
    });
  }

  protected async remove(pageId: string | undefined): Promise<void> {
    if (!pageId) {
      return;
    }

    await this.execute(async () => {
      await deleteTrackingPage(pageId);
      this.trackingPages.set(this.trackingPages().filter((item) => item.id !== pageId));

      if (this.selectedPageId() === pageId) {
        this.selectedPageId.set(null);
        this.analytics.set(null);
      }

      this.autoSelectFirstPage();
      this.feedback.set(this.tx('Takip sayfası silindi.', 'Tracking page deleted.'));
    });
  }

  protected selectPage(pageId: string | null | undefined): void {
    if (!pageId) {
      this.selectedPageId.set(null);
      this.analytics.set(null);
      this.resetEditor();
      return;
    }

    const selected = this.trackingPages().find((item) => item.id === pageId);
    if (!selected) {
      return;
    }

    this.selectedPageId.set(pageId);
    this.mapPageToEditor(selected);
  }

  protected async loadAnalytics(): Promise<void> {
    const pageId = this.selectedPageId();
    if (!pageId) {
      this.feedback.set(this.tx('Analitik için bir takip sayfası seçin.', 'Select a tracking page for analytics.'));
      return;
    }

    await this.execute(async () => {
      const analytics = await getTrackingPageAnalytics({
        trackingPageId: pageId,
        fromUtc: this.toUtcIso(this.analyticsFilters.fromUtc),
        toUtc: this.toUtcIso(this.analyticsFilters.toUtc),
        trendBucketSizeMinutes: this.analyticsFilters.trendBucketSizeMinutes,
        recentVisitLimit: this.analyticsFilters.recentVisitLimit
      });

      this.analytics.set(analytics);
      this.feedback.set(this.tx('Analitik verisi güncellendi.', 'Analytics data refreshed.'));
    });
  }

  protected resetEditor(): void {
    this.editorDraft.slug = '';
    this.editorDraft.title = '';
    this.editorDraft.description = '';
    this.editorDraft.customHtmlContent = '';
    this.editorDraft.validFromUtc = '';
    this.editorDraft.validUntilUtc = '';
    this.editorDraft.ownerId = '';
    this.editorDraft.templateId = '';
    this.editorDraft.retentionDays = 30;
    this.editorDraft.captureIpAddress = true;
    this.editorDraft.ipAddressHashPolicy = IpAddressHashPolicy._2;
    this.editorDraft.enableBotFiltering = true;
    this.editorDraft.captureUtmParameters = true;
  }

  protected pageStateLabel(state: TrackingPagePublishState | undefined): string {
    switch (state) {
      case TrackingPagePublishState._1:
        return this.tx('Yayımlandı', 'Published');
      case TrackingPagePublishState._2:
        return this.tx('Arşivlendi', 'Archived');
      default:
        return this.tx('Taslak', 'Draft');
    }
  }

  protected pageStateChip(state: TrackingPagePublishState | undefined): string {
    switch (state) {
      case TrackingPagePublishState._1:
        return 'status-chip-success';
      case TrackingPagePublishState._2:
        return 'status-chip-muted';
      default:
        return 'status-chip-warning';
    }
  }

  protected trendBarWidth(point: TrackingVisitTrendPointResult): number {
    const max = Math.max(1, ...this.filteredTrends().map((item) => item.totalVisits ?? 0));
    return ((point.totalVisits ?? 0) / max) * 100;
  }

  protected distributionWidth(
    value: number,
    buckets: Array<{ label: string; value: number }>
  ): number {
    const max = Math.max(1, ...buckets.map((bucket) => bucket.value));
    return (value / max) * 100;
  }

  protected filteredRecentVisits(visits: TrackingRecentVisitResult[]): TrackingRecentVisitResult[] {
    const sourceFilter = this.analyticsFilters.sourceReferrerFilter.trim().toLowerCase();
    const userAgentFilter = this.analyticsFilters.userAgentFilter.trim().toLowerCase();

    return visits.filter((visit) => {
      if (!this.isWithinDateRange(visit.occurredAtUtc)) {
        return false;
      }

      const referrer = (visit.referrerUrl ?? 'direct').toLowerCase();
      const userAgent = (visit.userAgent ?? '').toLowerCase();

      const matchesSource = sourceFilter.length === 0 || referrer.includes(sourceFilter);
      const matchesUserAgent = userAgentFilter.length === 0 || userAgent.includes(userAgentFilter);

      return matchesSource && matchesUserAgent;
    });
  }

  private autoSelectFirstPage(): void {
    if (this.selectedPageId()) {
      return;
    }

    const firstPageId = this.trackingPages()[0]?.id ?? null;
    if (firstPageId) {
      this.selectPage(firstPageId);
    }
  }

  private applyUpdatedPage(updatedPage: TrackingPageResult): void {
    const updatedId = updatedPage.id;
    if (!updatedId) {
      return;
    }

    const pages = this.trackingPages();
    this.trackingPages.set(
      pages.some((item) => item.id === updatedId)
        ? pages.map((item) => (item.id === updatedId ? updatedPage : item))
        : [updatedPage, ...pages]
    );

    if (this.selectedPageId() === updatedId) {
      this.mapPageToEditor(updatedPage);
    }
  }

  private mapPageToEditor(page: TrackingPageResult): void {
    this.editorDraft.slug = page.slug ?? '';
    this.editorDraft.title = page.title ?? '';
    this.editorDraft.description = page.description ?? '';
    this.editorDraft.customHtmlContent = page.customHtmlContent ?? '';
    this.editorDraft.validFromUtc = page.validFromUtc ?? '';
    this.editorDraft.validUntilUtc = page.validUntilUtc ?? '';
    this.editorDraft.ownerId = page.ownerId ?? '';
    this.editorDraft.templateId = page.templateId ?? '';
    this.editorDraft.retentionDays = page.settings?.retentionDays ?? 30;
    this.editorDraft.captureIpAddress = page.settings?.captureIpAddress ?? true;
    this.editorDraft.ipAddressHashPolicy = this.editorDraft.captureIpAddress
      ? (page.settings?.ipAddressHashPolicy ?? IpAddressHashPolicy._2)
      : IpAddressHashPolicy._0;
    this.editorDraft.enableBotFiltering = page.settings?.enableBotFiltering ?? true;
    this.editorDraft.captureUtmParameters = page.settings?.captureUtmParameters ?? true;
  }

  private toValidatedInput(): UpsertTrackingPageInput | null {
    const slug = this.editorDraft.slug.trim();
    const title = this.editorDraft.title.trim();
    if (slug.length === 0 || title.length === 0) {
      this.feedback.set(this.tx('Kısa ad ve başlık zorunludur.', 'Slug and title are required.'));
      return null;
    }

    return {
      slug,
      title,
      description: this.editorDraft.description.trim(),
      customHtmlContent: this.editorDraft.customHtmlContent.trim(),
      validFromUtc: this.toUtcIso(this.editorDraft.validFromUtc),
      validUntilUtc: this.toUtcIso(this.editorDraft.validUntilUtc),
      ownerId: this.editorDraft.ownerId.trim(),
      templateId: this.editorDraft.templateId.trim(),
      retentionDays: Number(this.editorDraft.retentionDays) || 30,
      captureIpAddress: this.editorDraft.captureIpAddress,
      ipAddressHashPolicy: this.editorDraft.captureIpAddress ? this.editorDraft.ipAddressHashPolicy : IpAddressHashPolicy._0,
      enableBotFiltering: this.editorDraft.enableBotFiltering,
      captureUtmParameters: this.editorDraft.captureUtmParameters
    };
  }

  private matchesStatus(
    page: TrackingPageResult,
    status: TrackingGridQuery['status']
  ): boolean {
    if (status === 'all') {
      return true;
    }

    const mappedStatus =
      page.publishState === TrackingPagePublishState._1
        ? 'published'
        : page.publishState === TrackingPagePublishState._2
          ? 'archived'
          : 'draft';

    return mappedStatus === status;
  }

  private comparePages(
    left: TrackingPageResult,
    right: TrackingPageResult,
    sortBy: TrackingGridQuery['sortBy'],
    direction: TrackingGridQuery['sortDirection']
  ): number {
    const factor = direction === 'asc' ? 1 : -1;
    const toComparable = (value: string | null | undefined): string => (value ?? '').toLowerCase();

    let comparison = 0;

    switch (sortBy) {
      case 'created':
        comparison = toComparable(left.createdAtUtc).localeCompare(toComparable(right.createdAtUtc));
        break;
      case 'slug':
        comparison = toComparable(left.slug).localeCompare(toComparable(right.slug));
        break;
      case 'title':
        comparison = toComparable(left.title).localeCompare(toComparable(right.title));
        break;
      default:
        comparison = toComparable(left.updatedAtUtc).localeCompare(toComparable(right.updatedAtUtc));
        break;
    }

    return comparison * factor;
  }

  private ensurePageBounds(): void {
    const page = this.gridQuery().page;
    const max = this.maxPage();

    if (page > max) {
      this.gridQuery.set({ ...this.gridQuery(), page: max });
    }
  }

  private toUtcIso(localDateTime: string | null): string | null {
    if (!localDateTime) {
      return null;
    }
    if (localDateTime.trim().length === 0) {
      return null;
    }

    const date = new Date(localDateTime);
    if (Number.isNaN(date.getTime())) {
      return null;
    }

    return date.toISOString();
  }

  private isWithinDateRange(value: string | null | undefined): boolean {
    if (!value) {
      return true;
    }

    const current = new Date(value).getTime();
    if (Number.isNaN(current)) {
      return true;
    }

    const from = this.toUtcIso(this.analyticsFilters.fromUtc);
    const to = this.toUtcIso(this.analyticsFilters.toUtc);
    const fromTime = from ? new Date(from).getTime() : Number.NEGATIVE_INFINITY;
    const toTime = to ? new Date(to).getTime() : Number.POSITIVE_INFINITY;

    return current >= fromTime && current <= toTime;
  }

  private toDistribution(
    visits: TrackingRecentVisitResult[],
    selector: (visit: TrackingRecentVisitResult) => string
  ): Array<{ label: string; value: number }> {
    const map = new Map<string, number>();

    for (const visit of visits) {
      const key = selector(visit);
      map.set(key, (map.get(key) ?? 0) + 1);
    }

    return [...map.entries()]
      .map(([label, value]) => ({ label, value }))
      .sort((left, right) => right.value - left.value)
      .slice(0, 8);
  }

  private normalizeDistributionLabel(value: string | null | undefined, fallback: string): string {
    const normalized = (value ?? '').trim();
    return normalized.length === 0 ? fallback : normalized;
  }

  private toDeviceBucket(userAgent: string | null | undefined): string {
    const normalized = (userAgent ?? '').toLowerCase();

    if (normalized.includes('mobile')) {
      return 'mobile';
    }

    if (normalized.includes('tablet')) {
      return 'tablet';
    }

    if (normalized.includes('bot') || normalized.includes('crawler') || normalized.includes('spider')) {
      return 'bot';
    }

    if (normalized.length > 0) {
      return 'desktop';
    }

    return 'unknown';
  }

  private async execute(action: () => Promise<void>): Promise<void> {
    this.feedback.set(null);
    this.isBusy.set(true);

    try {
      await action();
    } catch (error) {
      this.feedback.set(resolveApiError(error).message);
    } finally {
      this.isBusy.set(false);
    }
  }
}
