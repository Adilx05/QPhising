import { CommonModule } from '@angular/common';
import { Component, OnDestroy, signal } from '@angular/core';
import { DomSanitizer, SafeResourceUrl } from '@angular/platform-browser';
import { ActivatedRoute } from '@angular/router';
import { PublicTrackingService, TrackingService, type TrackingLandingPageResult } from '../../../shared/proxy';

@Component({
  selector: 'app-public-tracking-landing-page',
  standalone: true,
  imports: [CommonModule],
  template: `
    <ng-container *ngIf="landing() as data; else missing">
      <iframe
        class="h-screen w-screen border-0"
        [src]="previewUrl(data.customHtmlContent || data.templateHtmlContent || '<p style=&quot;padding:12px&quot;>Landing content not configured.</p>')"
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
export class PublicTrackingLandingPageComponent implements OnDestroy {
  private static readonly VisitSessionStorageKey = 'qphising_public_visit_session_id';
  private static readonly VisitCaptureThrottleStorageKeyPrefix = 'qphising_public_visit_capture';
  private static readonly VisitCaptureThrottleWindowMs = 10_000;
  protected readonly landing = signal<TrackingLandingPageResult | null>(null);
  private readonly previewUrlCache = new Map<string, SafeResourceUrl>();
  private readonly objectUrls = new Set<string>();

  public constructor(
    private readonly route: ActivatedRoute,
    private readonly sanitizer: DomSanitizer
  ) {
    void this.load();
  }

  public ngOnDestroy(): void {
    this.objectUrls.forEach((url) => URL.revokeObjectURL(url));
    this.objectUrls.clear();
    this.previewUrlCache.clear();
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

  private async load(): Promise<void> {
    const slug = this.route.snapshot.paramMap.get('slug');
    if (!slug) {
      return;
    }

    try {
      const landing = await PublicTrackingService.trackingPublicLandingBySlug({
        slug,
        id: this.route.snapshot.queryParamMap.get('id') ?? undefined,
        campaign: this.route.snapshot.queryParamMap.get('campaign') ?? undefined
      });
      this.landing.set(landing);
      await this.captureVisit(landing);
    } catch {
      this.landing.set(null);
    }
  }

  private async captureVisit(landing: TrackingLandingPageResult): Promise<void> {
    const trackingPageId = landing.trackingPageId;
    if (!trackingPageId) {
      return;
    }

    if (this.wasVisitRecentlyCaptured()) {
      return;
    }

    try {
      await TrackingService.trackingPageCaptureVisit({
        trackingPageId,
        requestBody: {
          occurredAtUtc: new Date().toISOString(),
          sessionId: this.getSessionId(),
          visitorFingerprint: this.buildVisitorFingerprint(),
          userAgent: navigator.userAgent,
          referrerUrl: document.referrer || window.location.href,
          ipAddressHashPolicy: landing.captureIpAddress ? (landing.ipAddressHashPolicy ?? 2) : 0,
          deduplicationWindowSeconds: 120
        }
      });
      this.markVisitCapturedNow();
    } catch {
      // Keep public page rendering resilient if visit ingestion fails.
    }
  }

  private wasVisitRecentlyCaptured(): boolean {
    const key = this.getCaptureThrottleStorageKey();
    const rawValue = window.sessionStorage.getItem(key);
    if (!rawValue) {
      return false;
    }

    const lastCapturedAt = Number.parseInt(rawValue, 10);
    if (Number.isNaN(lastCapturedAt)) {
      window.sessionStorage.removeItem(key);
      return false;
    }

    return Date.now() - lastCapturedAt < PublicTrackingLandingPageComponent.VisitCaptureThrottleWindowMs;
  }

  private markVisitCapturedNow(): void {
    window.sessionStorage.setItem(this.getCaptureThrottleStorageKey(), Date.now().toString());
  }

  private getCaptureThrottleStorageKey(): string {
    return `${PublicTrackingLandingPageComponent.VisitCaptureThrottleStorageKeyPrefix}:${window.location.pathname}${window.location.search}`;
  }

  private getSessionId(): string {
    const existing = window.localStorage.getItem(PublicTrackingLandingPageComponent.VisitSessionStorageKey);
    if (existing && existing.length > 0) {
      return existing;
    }

    const generated = this.generateId();
    window.localStorage.setItem(PublicTrackingLandingPageComponent.VisitSessionStorageKey, generated);
    return generated;
  }

  private buildVisitorFingerprint(): string {
    const language = navigator.language ?? 'unknown-language';
    const timezone = Intl.DateTimeFormat().resolvedOptions().timeZone ?? 'unknown-tz';
    const screenKey = `${window.screen.width}x${window.screen.height}x${window.screen.colorDepth}`;
    const platform = navigator.platform ?? 'unknown-platform';
    const source = `${platform}|${language}|${timezone}|${screenKey}`;
    return `fp-${this.hashString(source)}`;
  }

  private hashString(value: string): string {
    let hash = 0;
    for (let i = 0; i < value.length; i += 1) {
      hash = (hash << 5) - hash + value.charCodeAt(i);
      hash |= 0;
    }

    return Math.abs(hash).toString(16);
  }

  private generateId(): string {
    if (typeof crypto !== 'undefined' && typeof crypto.randomUUID === 'function') {
      return crypto.randomUUID();
    }

    return `${Date.now().toString(36)}-${Math.random().toString(36).slice(2, 12)}`;
  }
}
