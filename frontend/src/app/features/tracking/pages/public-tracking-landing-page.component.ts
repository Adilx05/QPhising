import { CommonModule } from '@angular/common';
import { Component, signal } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { PublicTrackingService, type TrackingLandingPageResult } from '../../../shared/proxy';

@Component({
  selector: 'app-public-tracking-landing-page',
  standalone: true,
  imports: [CommonModule],
  template: `
    <ng-container *ngIf="landing() as data; else missing">
      <iframe
        class="h-screen w-screen border-0"
        [srcdoc]="data.customHtmlContent || data.templateHtmlContent || '<p style=&quot;padding:12px&quot;>Landing content not configured.</p>'"
      ></iframe>
    </ng-container>
    <ng-template #missing>
      <main class="flex min-h-screen items-center justify-center bg-white p-6 text-center">
        <div>
          <h1 class="text-2xl font-semibold text-slate-900">404</h1>
          <p class="mt-2 text-sm text-slate-600">Page not found.</p>
        </div>
      </main>
    </ng-template>
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
