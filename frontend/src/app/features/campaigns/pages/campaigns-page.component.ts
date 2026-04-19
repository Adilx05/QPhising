import { CommonModule } from '@angular/common';
import { Component, computed, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { DropdownModule } from 'primeng/dropdown';
import { InputTextModule } from 'primeng/inputtext';
import { InputTextarea } from 'primeng/inputtextarea';
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
  imports: [CommonModule, FormsModule, RouterLink, ButtonModule, DropdownModule, InputTextModule, InputTextarea],
  templateUrl: './campaigns-page.component.html'
})
export class CampaignsPageComponent {
  protected readonly isBusy = signal(false);
  protected readonly feedback = signal<string | null>(null);
  protected readonly publicLinks = signal<{ slugUrl: string; idUrl: string } | null>(null);
  protected readonly campaigns = signal<CampaignResult[]>([]);
  protected readonly templates = signal<TemplateResult[]>([]);
  protected readonly canOperate = computed(() => this.authSessionService.hasRequiredRole('Operator'));

  protected readonly createForm = {
    name: '',
    slug: '',
    pageTitle: '',
    templateId: null as string | null,
    htmlContent: '',
    validFromUtc: '',
    validUntilUtc: ''
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

    if (!name || !trackingPageSlug || !trackingPageTitle) {
      this.feedback.set('Campaign name, page slug ve page title zorunludur.');
      return;
    }

    await this.execute(async () => {
      const created = await createCampaign({
        name,
        trackingPageSlug,
        trackingPageTitle,
        trackingPageDescription: null,
        templateId: this.createForm.templateId,
        htmlContent: this.createForm.htmlContent.trim() || null,
        validFromUtc: this.toUtcIso(this.createForm.validFromUtc),
        validUntilUtc: this.toUtcIso(this.createForm.validUntilUtc)
      });

      this.publicLinks.set({
        slugUrl: `/p/${trackingPageSlug}?campaign=${encodeURIComponent(name)}`,
        idUrl: `/p/${trackingPageSlug}?id=${created.trackingPageId}`
      });

      this.createForm.name = '';
      this.createForm.slug = '';
      this.createForm.pageTitle = '';
      this.createForm.templateId = null;
      this.createForm.htmlContent = '';
      this.createForm.validFromUtc = '';
      this.createForm.validUntilUtc = '';
      this.feedback.set('Campaign oluşturuldu. Public linkler hazırlandı.');
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

  protected previewHtml(): string {
    const customHtml = this.createForm.htmlContent.trim();
    if (customHtml.length > 0) {
      return customHtml;
    }

    const selectedTemplateHtml = this.templates()
      .find((template) => template.id === this.createForm.templateId)
      ?.htmlContent
      ?.trim();

    return selectedTemplateHtml && selectedTemplateHtml.length > 0
      ? selectedTemplateHtml
      : '<p style="padding:8px">Preview boş.</p>';
  }

  private toUtcIso(value: string): string | null {
    return value ? new Date(value).toISOString() : null;
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
