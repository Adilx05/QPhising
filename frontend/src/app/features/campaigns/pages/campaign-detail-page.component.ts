import { CommonModule } from '@angular/common';
import { Component, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { resolveApiError } from '../../../core/http/api-error-handler';
import { type CampaignResult } from '../../../shared/proxy';
import { getCampaignById } from '../data-access';

@Component({
  selector: 'app-campaign-detail-page',
  standalone: true,
  imports: [CommonModule, RouterLink, ButtonModule],
  template: `
    <section class="mb-6 flex items-center justify-between gap-3">
      <div>
        <h1 class="page-title">Campaign Detail</h1>
        <p class="page-subtitle">Campaign ve bağlı tracking page referansını buradan izleyebilirsin.</p>
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
    </section>

    <p *ngIf="feedback()" class="mt-4 rounded-xl border border-blue-100 bg-blue-50 px-3 py-2 text-sm text-blue-800">{{ feedback() }}</p>

    <ng-template #loadingState>
      <section class="surface-card p-5 text-sm text-slate-500">Campaign detayı yükleniyor...</section>
    </ng-template>
  `
})
export class CampaignDetailPageComponent {
  protected readonly campaign = signal<CampaignResult | null>(null);
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
      this.campaign.set(await getCampaignById(campaignId));
    } catch (error) {
      this.feedback.set(resolveApiError(error).message);
    }
  }
}
