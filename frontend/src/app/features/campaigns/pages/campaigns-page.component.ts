import { CommonModule } from '@angular/common';
import { Component, computed, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { resolveApiError } from '../../../core/http/api-error-handler';
import { AuthSessionService } from '../../../core/auth/auth-session';
import {
  CampaignLifecycleState ,
  type CampaignResult
} from '../../../shared/proxy';
import {
  cancelCampaign,
  completeCampaign,
  createCampaign,
  listCampaigns,
  pauseCampaign,
  scheduleCampaign,
  startCampaign
} from '../data-access';

@Component({
  selector: 'app-campaigns-page',
  standalone: true,
  imports: [CommonModule, FormsModule, ButtonModule, InputTextModule],
  template: `
    <section class="mb-6">
      <h1 class="page-title">Campaign Management</h1>
      <p class="page-subtitle">Campaign verileri doğrudan generated CampaignService proxy çağrılarıyla yönetilir.</p>
    </section>

    <section class="surface-card p-5">
      <div class="flex flex-wrap items-end gap-3">
        <div class="min-w-56 flex-1">
          <label class="mb-1 block text-xs font-semibold uppercase text-slate-500">Campaign Name</label>
          <input pInputText [(ngModel)]="createForm.name" class="w-full" placeholder="Quarterly Simulation" />
        </div>

        <div class="min-w-56 flex-1">
          <label class="mb-1 block text-xs font-semibold uppercase text-slate-500">Template Id</label>
          <input pInputText [(ngModel)]="createForm.templateId" class="w-full" placeholder="00000000-0000-0000-0000-000000000000" />
        </div>

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
            <p class="text-sm text-slate-500">ID: {{ campaign.id }}</p>
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

        <div class="mt-4 grid gap-3 xl:grid-cols-2">
          <div class="rounded-xl border border-slate-200 p-4">
            <p class="text-xs font-semibold uppercase text-slate-500">Schedule</p>
            <div class="mt-2 grid gap-2 sm:grid-cols-2">
              <input
                pInputText
                class="w-full"
                placeholder="2026-06-01T09:00:00Z"
                [ngModel]="scheduleDrafts()[campaign.id || ''].startsAtUtc || ''"
                (ngModelChange)="setScheduleDraft(campaign.id, 'startsAtUtc', $event)"
              />
              <input
                pInputText
                class="w-full"
                placeholder="2026-06-01T17:00:00Z"
                [ngModel]="scheduleDrafts()[campaign.id || ''].endsAtUtc || ''"
                (ngModelChange)="setScheduleDraft(campaign.id, 'endsAtUtc', $event)"
              />
            </div>
            <button
              class="mt-2"
              pButton
              type="button"
              icon="pi pi-calendar-plus"
              label="Apply Schedule"
              [disabled]="!canOperate() || isBusy()"
              (click)="schedule(campaign.id)"
            ></button>
          </div>
        </div>

        <div class="mt-4 flex flex-wrap gap-2">
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
  protected readonly scheduleDrafts = signal<Record<string, { startsAtUtc: string; endsAtUtc: string }>>({});
  protected readonly canOperate = computed(() => this.authSessionService.hasRequiredRole('Operator'));

  protected readonly createForm = {
    name: '',
    templateId: ''
  };

  public constructor(private readonly authSessionService: AuthSessionService) {
    void this.refresh();
  }

  protected async refresh(): Promise<void> {
    await this.execute(async () => {
      this.campaigns.set(await listCampaigns());
      this.feedback.set('Campaign listesi güncellendi.');
    });
  }

  protected async create(): Promise<void> {
    const name = this.createForm.name.trim();
    const templateId = this.createForm.templateId.trim();

    if (name.length === 0 || templateId.length === 0) {
      this.feedback.set('Campaign name ve template id zorunludur.');
      return;
    }

    await this.execute(async () => {
      await createCampaign({ name, templateId });
      this.createForm.name = '';
      this.createForm.templateId = '';
      this.feedback.set('Campaign oluşturuldu.');
      this.campaigns.set(await listCampaigns());
    });
  }

  protected async schedule(campaignId: string | undefined): Promise<void> {
    if (!campaignId) {
      return;
    }

    const draft = this.scheduleDrafts()[campaignId];
    const startsAtUtc = draft?.startsAtUtc?.trim() ?? '';

    if (startsAtUtc.length === 0) {
      this.feedback.set('Schedule için startsAtUtc zorunludur.');
      return;
    }

    await this.execute(async () => {
      const updatedCampaign = await scheduleCampaign({
        campaignId,
        startsAtUtc,
        endsAtUtc: draft?.endsAtUtc?.trim() || null
      });
      this.applyUpdatedCampaign(updatedCampaign);
      this.feedback.set('Campaign schedule güncellendi.');
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

  protected setScheduleDraft(
    campaignId: string | undefined,
    field: 'startsAtUtc' | 'endsAtUtc',
    value: string
  ): void {
    if (!campaignId) {
      return;
    }

    const current = this.scheduleDrafts()[campaignId] ?? { startsAtUtc: '', endsAtUtc: '' };

    this.scheduleDrafts.set({
      ...this.scheduleDrafts(),
      [campaignId]: {
        ...current,
        [field]: value
      }
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
