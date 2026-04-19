import { CommonModule } from '@angular/common';
import { Component, computed, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { DropdownModule } from 'primeng/dropdown';
import { InputTextModule } from 'primeng/inputtext';
import { resolveApiError } from '../../../core/http/api-error-handler';
import { AuthSessionService } from '../../../core/auth/auth-session';
import {
  CampaignLifecycleState,
  type CampaignResult,
  TemplateService,
  type TemplateResult
} from '../../../shared/proxy';
import {
  cancelCampaign,
  completeCampaign,
  createCampaign,
  listCampaigns,
  pauseCampaign,
  startCampaign
} from '../data-access';

@Component({
  selector: 'app-campaigns-page',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink, ButtonModule, DropdownModule, InputTextModule],
  template: `
    <section class="mb-6">
      <h1 class="page-title">Campaigns</h1>
      <p class="page-subtitle">Template seçerek ya da sıfırdan tracking sayfası oluşturarak yeni campaign başlatabilirsin.</p>
    </section>

    <section class="surface-card p-5">
      <div class="grid gap-3 md:grid-cols-2">
        <div>
          <label class="mb-1 block text-xs font-semibold uppercase text-slate-500">Campaign Name</label>
          <input pInputText [(ngModel)]="createForm.name" class="w-full" placeholder="Q2 Signup Simulation" />
        </div>

        <div>
          <label class="mb-1 block text-xs font-semibold uppercase text-slate-500">Template (Optional)</label>
          <p-dropdown
            [options]="templateOptions()"
            optionLabel="label"
            optionValue="value"
            [showClear]="true"
            [ngModel]="createForm.templateId"
            (ngModelChange)="createForm.templateId = $event"
            placeholder="Blank campaign page"
            styleClass="w-full"
          ></p-dropdown>
        </div>

        <div>
          <label class="mb-1 block text-xs font-semibold uppercase text-slate-500">Page Slug</label>
          <input pInputText [(ngModel)]="createForm.slug" class="w-full" placeholder="q2-signup" />
        </div>

        <div>
          <label class="mb-1 block text-xs font-semibold uppercase text-slate-500">Page Title</label>
          <input pInputText [(ngModel)]="createForm.pageTitle" class="w-full" placeholder="Q2 Signup Landing" />
        </div>

        <div class="md:col-span-2">
          <label class="mb-1 block text-xs font-semibold uppercase text-slate-500">Destination URL</label>
          <input pInputText [(ngModel)]="createForm.destinationUrl" class="w-full" placeholder="https://example.com/welcome" />
        </div>
      </div>

      <div class="mt-4">
        <button
          pButton
          type="button"
          icon="pi pi-plus"
          label="Create Campaign"
          [disabled]="!canOperate() || isBusy()"
          [loading]="isBusy()"
          (click)="create()"
        ></button>
      </div>

      <p *ngIf="feedback()" class="mt-4 rounded-xl border border-blue-100 bg-blue-50 px-3 py-2 text-sm text-blue-800">
        {{ feedback() }}
      </p>
    </section>

    <section class="mt-6 grid gap-4">
      <article *ngFor="let campaign of campaigns()" class="surface-card p-5">
        <div class="flex flex-wrap items-start justify-between gap-3">
          <div>
            <h2 class="text-lg font-semibold text-slate-900">{{ campaign.name || 'Unnamed Campaign' }}</h2>
            <p class="text-sm text-slate-500">Tracking Page: {{ campaign.trackingPageId }}</p>
          </div>

          <span class="status-chip" [ngClass]="stateChipClass(campaign.lifecycleState)">
            {{ stateLabel(campaign.lifecycleState) }}
          </span>
        </div>

        <div class="mt-4 grid gap-3 lg:grid-cols-3">
          <div class="rounded-xl border border-slate-200 bg-slate-50/70 px-4 py-3">
            <p class="text-xs font-semibold uppercase text-slate-500">Starts At (UTC)</p>
            <p class="mt-1 text-sm text-slate-700">{{ campaign.startsAtUtc || 'Not scheduled' }}</p>
          </div>

          <div class="rounded-xl border border-slate-200 bg-slate-50/70 px-4 py-3">
            <p class="text-xs font-semibold uppercase text-slate-500">Ends At (UTC)</p>
            <p class="mt-1 text-sm text-slate-700">{{ campaign.endsAtUtc || 'Not scheduled' }}</p>
          </div>
        </div>

        <div class="mt-4 flex flex-wrap gap-2">
          <a pButton type="button" size="small" severity="info" [routerLink]="['/campaigns', campaign.id]" label="Details"></a>
          <button pButton type="button" size="small" label="Start" [disabled]="!canOperate() || isBusy()" (click)="transition(campaign.id, 'start')"></button>
          <button pButton type="button" size="small" severity="secondary" label="Pause" [disabled]="!canOperate() || isBusy()" (click)="transition(campaign.id, 'pause')"></button>
          <button pButton type="button" size="small" severity="success" label="Complete" [disabled]="!canOperate() || isBusy()" (click)="transition(campaign.id, 'complete')"></button>
          <button pButton type="button" size="small" severity="danger" label="Cancel" [disabled]="!canOperate() || isBusy()" (click)="transition(campaign.id, 'cancel')"></button>
        </div>
      </article>

      <article *ngIf="campaigns().length === 0" class="surface-card p-5 text-sm text-slate-500">
        Henüz campaign kaydı bulunmuyor.
      </article>
    </section>
  `
})
export class CampaignsPageComponent {
  protected readonly isBusy = signal(false);
  protected readonly feedback = signal<string | null>(null);
  protected readonly campaigns = signal<CampaignResult[]>([]);
  protected readonly templates = signal<TemplateResult[]>([]);
  protected readonly canOperate = computed(() => this.authSessionService.hasRequiredRole('Operator'));

  protected readonly createForm = {
    name: '',
    slug: '',
    pageTitle: '',
    destinationUrl: '',
    templateId: null as string | null
  };

  protected readonly templateOptions = computed(() =>
    this.templates()
      .filter((template) => !!template.id)
      .map((template) => ({
        value: template.id!,
        label: template.name || template.id!
      })));

  public constructor(private readonly authSessionService: AuthSessionService) {
    void this.refresh();
  }

  protected async refresh(): Promise<void> {
    await this.execute(async () => {
      const [campaigns, templates] = await Promise.all([listCampaigns(), TemplateService.templateList()]);
      this.campaigns.set(campaigns);
      this.templates.set(templates);
      this.feedback.set('Campaign listesi güncellendi.');
    });
  }

  protected async create(): Promise<void> {
    const name = this.createForm.name.trim();
    const trackingPageSlug = this.createForm.slug.trim();
    const trackingPageTitle = this.createForm.pageTitle.trim();
    const destinationUrl = this.createForm.destinationUrl.trim();

    if (!name || !trackingPageSlug || !trackingPageTitle || !destinationUrl) {
      this.feedback.set('Campaign name, page slug, page title ve destination URL zorunludur.');
      return;
    }

    await this.execute(async () => {
      await createCampaign({
        name,
        trackingPageSlug,
        trackingPageTitle,
        destinationUrl,
        trackingPageDescription: null,
        templateId: this.createForm.templateId
      });

      this.createForm.name = '';
      this.createForm.slug = '';
      this.createForm.pageTitle = '';
      this.createForm.destinationUrl = '';
      this.createForm.templateId = null;
      this.feedback.set('Campaign oluşturuldu.');
      this.campaigns.set(await listCampaigns());
    });
  }

  protected async transition(
    campaignId: string | undefined,
    action: 'start' | 'pause' | 'complete' | 'cancel'
  ): Promise<void> {
    if (!campaignId) {
      return;
    }

    await this.execute(async () => {
      const updatedCampaign = await this.transitionCampaign(campaignId, action);
      this.applyUpdatedCampaign(updatedCampaign);
      this.feedback.set(`Campaign ${action} işlemi başarılı.`);
    });
  }

  protected stateLabel(state: CampaignLifecycleState | undefined): string {
    switch (state) {
      case CampaignLifecycleState._0:
        return 'Draft';
      case CampaignLifecycleState._1:
        return 'Scheduled';
      case CampaignLifecycleState._2:
        return 'Active';
      case CampaignLifecycleState._3:
        return 'Paused';
      case CampaignLifecycleState._4:
        return 'Completed';
      case CampaignLifecycleState._5:
        return 'Cancelled';
      default:
        return 'Unknown';
    }
  }

  protected stateChipClass(state: CampaignLifecycleState | undefined): string {
    switch (state) {
      case CampaignLifecycleState._4:
        return 'status-chip-success';
      case CampaignLifecycleState._2:
        return 'status-chip-warning';
      case CampaignLifecycleState._5:
        return 'status-chip-muted';
      default:
        return 'status-chip-default';
    }
  }

  private async transitionCampaign(
    campaignId: string,
    action: 'start' | 'pause' | 'complete' | 'cancel'
  ): Promise<CampaignResult> {
    switch (action) {
      case 'start':
        return startCampaign(campaignId);
      case 'pause':
        return pauseCampaign(campaignId);
      case 'complete':
        return completeCampaign(campaignId);
      case 'cancel':
        return cancelCampaign(campaignId);
    }
  }

  private applyUpdatedCampaign(updatedCampaign: CampaignResult): void {
    const campaigns = this.campaigns();
    const updatedId = updatedCampaign.id;

    if (!updatedId) {
      void this.refresh();
      return;
    }

    const updated = campaigns.some((campaign) => campaign.id === updatedId)
      ? campaigns.map((campaign) => (campaign.id === updatedId ? updatedCampaign : campaign))
      : [updatedCampaign, ...campaigns];

    this.campaigns.set(updated);
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
