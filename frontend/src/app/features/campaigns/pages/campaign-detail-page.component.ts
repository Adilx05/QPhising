import { CommonModule } from '@angular/common';
import { Component, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { resolveApiError } from '../../../core/http/api-error-handler';
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
        <h1 class="page-title">Campaign Detail</h1>
        <p class="page-subtitle">Campaign, bağlı tracking page, public linkler ve içerik önizlemesini görüntüleyebilirsin.</p>
      </div>
      <a pButton type="button" severity="secondary" [routerLink]="['/campaigns']" label="Back"></a>
    </section>

    <section *ngIf="campaign(); else loadingState" class="surface-card p-5">
      <h2 class="text-xl font-semibold text-slate-900">{{ campaign()?.name }}</h2>
      <div class="mt-4 grid gap-3 md:grid-cols-2">
        <div class="rounded-xl border border-slate-200 bg-slate-50 p-4">
          <p class="text-xs font-semibold uppercase text-slate-500">Campaign Id</p>
          <p class="mt-1 text-sm text-slate-700">{{ campaign()?.id }}</p>
        </div>
        <div class="rounded-xl border border-slate-200 bg-slate-50 p-4">
          <p class="text-xs font-semibold uppercase text-slate-500">Tracking Page Id</p>
          <p class="mt-1 text-sm text-slate-700">{{ campaign()?.trackingPageId }}</p>
        </div>
      </div>

      <div *ngIf="trackingPage() as page" class="mt-4 grid gap-3 md:grid-cols-2">
        <div class="rounded-xl border border-slate-200 bg-slate-50 p-4">
          <p class="text-xs font-semibold uppercase text-slate-500">Slug</p>
          <p class="mt-1 text-sm text-slate-700">{{ page.slug }}</p>
        </div>
        <div class="rounded-xl border border-slate-200 bg-slate-50 p-4">
          <p class="text-xs font-semibold uppercase text-slate-500">Publish State</p>
          <p class="mt-1 text-sm text-slate-700">{{ publishStateLabel(page.publishState) }}</p>
        </div>
      </div>

      <div *ngIf="analytics() as analytics" class="mt-4 grid gap-3 md:grid-cols-2">
        <div class="rounded-xl border border-slate-200 bg-slate-50 p-4">
          <p class="text-xs font-semibold uppercase text-slate-500">Total Clicks</p>
          <p class="mt-1 text-sm text-slate-700">{{ analytics.summary?.totalVisits ?? 0 }}</p>
        </div>
        <div class="rounded-xl border border-slate-200 bg-slate-50 p-4">
          <p class="text-xs font-semibold uppercase text-slate-500">Unique Clicks</p>
          <p class="mt-1 text-sm text-slate-700">{{ analytics.summary?.uniqueVisitors ?? 0 }}</p>
        </div>
      </div>

      <div *ngIf="publicLinks() as links" class="mt-4 rounded-xl border border-emerald-100 bg-emerald-50 p-4 text-xs text-emerald-900">
        <p><strong>Slug URL:</strong> {{ links.slugUrl }}</p>
        <p><strong>ID URL:</strong> {{ links.idUrl }}</p>
      </div>

      <div class="mt-4">
        <p class="mb-2 text-xs font-semibold uppercase text-slate-500">HTML Preview</p>
        <div class="rounded-xl border border-slate-200 bg-white p-3">
          <iframe class="h-80 w-full rounded-lg" [srcdoc]="previewHtml()"></iframe>
        </div>
      </div>
    </section>

    <p *ngIf="feedback()" class="mt-4 rounded-xl border border-blue-100 bg-blue-50 px-3 py-2 text-sm text-blue-800">{{ feedback() }}</p>

    <ng-template #loadingState>
      <section class="surface-card p-5 text-sm text-slate-500">Campaign detayı yükleniyor...</section>
    </ng-template>
  `
})
export class CampaignDetailPageComponent {
  protected readonly campaign = signal<CampaignResult | null>(null);
  protected readonly trackingPage = signal<TrackingPageResult | null>(null);
  protected readonly analytics = signal<TrackingPageAnalyticsResult | null>(null);
  protected readonly landingPage = signal<TrackingLandingPageResult | null>(null);
  protected readonly publicLinks = signal<{ slugUrl: string; idUrl: string } | null>(null);
  protected readonly feedback = signal<string | null>(null);

  public constructor(private readonly route: ActivatedRoute) {
    void this.load();
  }

  private async load(): Promise<void> {
    const campaignId = this.route.snapshot.paramMap.get('campaignId');
    if (!campaignId) {
      this.feedback.set('Campaign kimliği bulunamadı.');
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
      : '<p style="padding:8px">Bu campaign için HTML içerik bulunamadı.</p>';
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
        slugUrl: `/p/${page.slug}`,
        idUrl: `/p/${page.slug}?id=${trackingPageId}`
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
