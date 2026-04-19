import { CommonModule } from '@angular/common';
import { Component, signal } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { PublicTrackingService, type TrackingLandingPageResult } from '../../../shared/proxy';

@Component({
  selector: 'app-public-tracking-landing-page',
  standalone: true,
  imports: [CommonModule],
  template: `
    <section class="mx-auto max-w-5xl p-4">
      <article *ngIf="landing() as data; else missing">
        <h1 class="mb-2 text-2xl font-semibold text-slate-900">{{ data.title }}</h1>
        <p class="mb-4 text-sm text-slate-500">{{ data.description }}</p>
        <iframe class="h-[70vh] w-full rounded-xl border border-slate-200" [srcdoc]="data.customHtmlContent || data.templateHtmlContent || '<p style=&quot;padding:12px&quot;>Landing content not configured.</p>'"></iframe>
      </article>
      <ng-template #missing>
        <article class="rounded-xl border border-slate-200 bg-white p-6 text-center">
          <h1 class="text-xl font-semibold text-slate-900">404</h1>
          <p class="text-sm text-slate-500">Tracking page bulunamadı veya pasif.</p>
        </article>
      </ng-template>
    </section>
  `
})
export class PublicTrackingLandingPageComponent {
  protected readonly landing = signal<TrackingLandingPageResult | null>(null);

  public constructor(private readonly route: ActivatedRoute) {
    void this.load();
  }

  private async load(): Promise<void> {
    const slug = this.route.snapshot.paramMap.get('slug');
    if (!slug) {
      return;
    }

    try {
      this.landing.set(await PublicTrackingService.trackingPublicLandingBySlug({
        slug,
        id: this.route.snapshot.queryParamMap.get('id') ?? undefined,
        campaign: this.route.snapshot.queryParamMap.get('campaign') ?? undefined
      }));
    } catch {
      this.landing.set(null);
    }
  }
}
