import { CommonModule } from '@angular/common';
import { Component, OnDestroy, signal } from '@angular/core';
import { DomSanitizer, SafeResourceUrl } from '@angular/platform-browser';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { resolveApiError } from '../../../core/http/api-error-handler';
import { AppLanguage, UserPreferencesService } from '../../../core/ui/user-preferences.service';
import {
  PublicTrackingService,
  TrackingService,
  type TrackingPageAnalyticsResult,
  type CampaignResult,
  type TrackingLandingPageResult,
  type TrackingPageResult
} from '../../../shared/proxy';
import { getCampaignById } from '../data-access';

@Component({
  selector: 'app-campaign-detail-page',
  standalone: true,
  imports: [CommonModule, RouterLink, ButtonModule],
  template: `
    <section class="mb-6 flex items-center justify-between gap-3">
      <div>
        <h1 class="page-title">{{ activeLanguage() === 'tr' ? 'Senaryo Detayı' : 'Campaign Detail' }}</h1>
        <p class="page-subtitle">{{ activeLanguage() === 'tr' ? 'Senaryo, bağlı takip sayfası, genel bağlantılar ve içerik önizlemesini görüntüleyebilirsiniz.' : 'View campaign, linked tracking page, public links, and content preview.' }}</p>
      </div>
      <a pButton type="button" severity="secondary" [routerLink]="['/campaigns']" [label]="activeLanguage() === 'tr' ? 'Geri' : 'Back'"></a>
    </section>

    <section *ngIf="campaign(); else loadingState" class="surface-card p-5">
      <h2 class="text-xl font-semibold text-slate-900">{{ campaign()?.name }}</h2>
      <div class="mt-4 grid gap-3 md:grid-cols-2">
        <div class="rounded-xl border border-slate-200 bg-slate-50 p-4">
          <p class="text-xs font-semibold uppercase text-slate-500">{{ activeLanguage() === 'tr' ? 'Senaryo Kimliği' : 'Campaign Id' }}</p>
          <p class="mt-1 text-sm text-slate-700">{{ campaign()?.id }}</p>
        </div>
        <div class="rounded-xl border border-slate-200 bg-slate-50 p-4">
          <p class="text-xs font-semibold uppercase text-slate-500">{{ activeLanguage() === 'tr' ? 'Takip Sayfası Kimliği' : 'Tracking Page Id' }}</p>
          <p class="mt-1 text-sm text-slate-700">{{ campaign()?.trackingPageId }}</p>
        </div>
      </div>

      <div class="mt-4 grid gap-3 md:grid-cols-2">
        <div class="rounded-xl border border-slate-200 bg-slate-50 p-4">
          <p class="text-xs font-semibold uppercase text-slate-500">{{ activeLanguage() === 'tr' ? 'Başlangıç' : 'Starts At' }}</p>
          <p class="mt-1 text-sm text-slate-700">{{ resolveStartsAtUtc() }}</p>
        </div>
        <div class="rounded-xl border border-slate-200 bg-slate-50 p-4">
          <p class="text-xs font-semibold uppercase text-slate-500">{{ activeLanguage() === 'tr' ? 'Bitiş' : 'Ends At' }}</p>
          <p class="mt-1 text-sm text-slate-700">{{ resolveEndsAtUtc() }}</p>
        </div>
      </div>

      <div *ngIf="trackingPage() as page" class="mt-4 grid gap-3 md:grid-cols-2">
        <div class="rounded-xl border border-slate-200 bg-slate-50 p-4">
          <p class="text-xs font-semibold uppercase text-slate-500">Slug</p>
          <p class="mt-1 text-sm text-slate-700">{{ page.slug }}</p>
        </div>
        <div class="rounded-xl border border-slate-200 bg-slate-50 p-4">
          <p class="text-xs font-semibold uppercase text-slate-500">{{ activeLanguage() === 'tr' ? 'Yayın Durumu' : 'Publish State' }}</p>
          <p class="mt-1 text-sm text-slate-700">{{ publishStateLabel(page.publishState) }}</p>
        </div>
      </div>

      <div *ngIf="analytics() as analytics" class="mt-4 grid gap-3 md:grid-cols-2">
        <div class="rounded-xl border border-slate-200 bg-slate-50 p-4">
          <p class="text-xs font-semibold uppercase text-slate-500">{{ activeLanguage() === 'tr' ? 'Toplam Tıklama' : 'Total Clicks' }}</p>
          <p class="mt-1 text-sm text-slate-700">{{ analytics.summary?.totalVisits ?? 0 }}</p>
        </div>
        <div class="rounded-xl border border-slate-200 bg-slate-50 p-4">
          <p class="text-xs font-semibold uppercase text-slate-500">{{ activeLanguage() === 'tr' ? 'Tekil Tıklama' : 'Unique Clicks' }}</p>
          <p class="mt-1 text-sm text-slate-700">{{ analytics.summary?.uniqueVisitors ?? 0 }}</p>
        </div>
      </div>

      <div *ngIf="publicLinks() as links" class="mt-4 rounded-xl border border-emerald-100 bg-emerald-50 p-4 text-xs text-emerald-900">
        <p class="break-all">
          <strong>{{ activeLanguage() === 'tr' ? 'Kısa ad URL:' : 'Slug URL:' }}</strong>
          <a class="font-medium underline underline-offset-2" [href]="links.slugUrl" target="_blank" rel="noopener noreferrer">{{ links.slugUrl }}</a>
        </p>
        <p class="mt-1 break-all">
          <strong>{{ activeLanguage() === 'tr' ? 'Kimlik URL:' : 'ID URL:' }}</strong>
          <a class="font-medium underline underline-offset-2" [href]="links.idUrl" target="_blank" rel="noopener noreferrer">{{ links.idUrl }}</a>
        </p>
      </div>

      <div class="mt-4">
        <p class="mb-2 text-xs font-semibold uppercase text-slate-500">{{ activeLanguage() === 'tr' ? 'HTML Önizleme' : 'HTML Preview' }}</p>
        <div class="rounded-xl border border-slate-200 bg-white p-3">
          <iframe class="h-80 w-full rounded-lg" [src]="previewUrl(previewHtml())"></iframe>
        </div>
      </div>
    </section>

    <p *ngIf="feedback()" class="mt-4 rounded-xl border border-blue-100 bg-blue-50 px-3 py-2 text-sm text-blue-800">{{ feedback() }}</p>

    <ng-template #loadingState>
      <section class="surface-card p-5 text-sm text-slate-500">{{ activeLanguage() === 'tr' ? 'Senaryo detayı yükleniyor...' : 'Campaign detail is loading...' }}</section>
    </ng-template>
  `
})
export class CampaignDetailPageComponent implements OnDestroy {
  protected readonly campaign = signal<CampaignResult | null>(null);
  protected readonly trackingPage = signal<TrackingPageResult | null>(null);
  protected readonly analytics = signal<TrackingPageAnalyticsResult | null>(null);
  protected readonly landingPage = signal<TrackingLandingPageResult | null>(null);
  protected readonly publicLinks = signal<{ slugUrl: string; idUrl: string } | null>(null);
  protected readonly feedback = signal<string | null>(null);
  private readonly origin = typeof window !== 'undefined' ? window.location.origin : '';
  private readonly previewUrlCache = new Map<string, SafeResourceUrl>();
  private readonly objectUrls = new Set<string>();

  public constructor(
    private readonly route: ActivatedRoute,
    private readonly sanitizer: DomSanitizer,
    private readonly userPreferencesService: UserPreferencesService
  ) {
    void this.load();
  }

  public ngOnDestroy(): void {
    this.objectUrls.forEach((url) => URL.revokeObjectURL(url));
    this.objectUrls.clear();
    this.previewUrlCache.clear();
  }


  protected activeLanguage(): AppLanguage {
    return this.userPreferencesService.language();
  }
  private async load(): Promise<void> {
    const campaignId = this.route.snapshot.paramMap.get('campaignId');
    if (!campaignId) {
      this.feedback.set(this.activeLanguage() === 'tr' ? 'Senaryo kimliği bulunamadı.' : 'Campaign identifier was not found.');
      return;
    }

    try {
      const campaign = await getCampaignById(campaignId);
      this.campaign.set(campaign);
      await this.loadTrackingPage(campaign);
    } catch (error) {
      this.feedback.set(resolveApiError(error).message);
    }
  }

  protected previewHtml(): string {
    const htmlContent = this.trackingPage()?.customHtmlContent?.trim();
    if (htmlContent && htmlContent.length > 0) {
      return htmlContent;
    }

    const templateHtml = this.landingPage()?.templateHtmlContent?.trim();
    return templateHtml && templateHtml.length > 0
      ? templateHtml
      : this.activeLanguage() === 'tr'
        ? '<p style="padding:8px">Bu senaryo için HTML içerik bulunamadı.</p>'
        : '<p style="padding:8px">No HTML content was found for this campaign.</p>';
  }

  protected previewUrl(htmlContent: string): SafeResourceUrl {
    const html = htmlContent.trim();
    if (html.length === 0) {
      return this.sanitizer.bypassSecurityTrustResourceUrl('about:blank');
    }

    const existing = this.previewUrlCache.get(html);
    if (existing) {
      return existing;
    }

    const objectUrl = URL.createObjectURL(new Blob([html], { type: 'text/html;charset=utf-8' }));
    this.objectUrls.add(objectUrl);

    const safeUrl = this.sanitizer.bypassSecurityTrustResourceUrl(objectUrl);
    this.previewUrlCache.set(html, safeUrl);
    return safeUrl;
  }

  protected publishStateLabel(state: number | undefined): string {
    switch (state) {
      case 0:
        return 'Draft';
      case 1:
        return 'Published';
      case 2:
        return 'Archived';
      default:
        return 'Unknown';
    }
  }

  protected resolveStartsAtUtc(): string {
    const value = this.campaign()?.startsAtUtc ?? this.trackingPage()?.validFromUtc;
    if (!value) {
      return this.activeLanguage() === 'tr' ? 'Planlanmadı' : 'Not scheduled';
    }
    return this.formatDateTime(value);
  }

  protected resolveEndsAtUtc(): string {
    const value = this.campaign()?.endsAtUtc ?? this.trackingPage()?.validUntilUtc;
    if (!value) {
      return this.activeLanguage() === 'tr' ? 'Planlanmadı' : 'Not scheduled';
    }
    return this.formatDateTime(value);
  }

  private formatDateTime(dateStr: string): string {
    try {
      const date = new Date(dateStr);
      const isValid = !isNaN(date.getTime());
      if (!isValid) {
        return dateStr;
      }

      const locale = this.activeLanguage() === 'tr' ? 'tr-TR' : 'en-US';
      const formatted = new Intl.DateTimeFormat(locale, {
        year: 'numeric',
        month: 'short',
        day: '2-digit',
        hour: '2-digit',
        minute: '2-digit'
      }).format(date);

      return formatted;
    } catch {
      return dateStr;
    }
  }

  private async loadTrackingPage(campaign: CampaignResult): Promise<void> {
    const trackingPageId = campaign.trackingPageId;
    if (!trackingPageId) {
      return;
    }

    const page = await TrackingService.trackingPageGetById({ trackingPageId });
    this.trackingPage.set(page);
    this.analytics.set(await TrackingService.trackingPageGetAnalytics({ trackingPageId }));

    if (page.slug) {
      this.publicLinks.set({
        slugUrl: `${this.origin}/p/${page.slug}`,
        idUrl: `${this.origin}/p/${page.slug}?id=${trackingPageId}`
      });

      try {
        const landing = await PublicTrackingService.trackingPublicLandingBySlug({
          slug: page.slug,
          id: trackingPageId
        });
        this.landingPage.set(landing);
      } catch {
        this.landingPage.set(null);
      }
    }
  }
}
