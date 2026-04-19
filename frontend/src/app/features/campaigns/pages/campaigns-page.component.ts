import { CommonModule } from '@angular/common';
import { Component, computed, signal, OnDestroy } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { DomSanitizer, SafeResourceUrl } from '@angular/platform-browser';
import { ButtonModule } from 'primeng/button';
import { DropdownModule } from 'primeng/dropdown';
import { InputTextModule } from 'primeng/inputtext';
import { InputTextarea } from 'primeng/inputtextarea';
import { AuthSessionService } from '../../../core/auth/auth-session';
import { resolveApiError } from '../../../core/http/api-error-handler';
import { AppLanguage, UserPreferencesService } from '../../../core/ui/user-preferences.service';
import {
  CampaignLifecycleState,
  IpAddressHashPolicy,
  TemplateService,
  TrackingService,
  type CampaignResult,
  type TemplateResult
} from '../../../shared/proxy';
import { cancelCampaign, completeCampaign, createCampaign, deleteCampaign, listCampaigns, pauseCampaign, startCampaign } from '../data-access';

@Component({
  selector: 'app-campaigns-page',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink, ButtonModule, DropdownModule, InputTextModule, InputTextarea],
  templateUrl: './campaigns-page.component.html'
})
export class CampaignsPageComponent implements OnDestroy {
  private readonly translations: Record<AppLanguage, Record<string, string>> = {
    tr: {
      feedbackRefreshed: 'Senaryo listesi güncellendi.',
      feedbackRequired: 'Senaryo adı, sayfa kısa adı ve sayfa başlığı zorunludur.',
      feedbackCreated: 'Senaryo oluşturuldu. Genel erişim bağlantıları hazırlandı.',
      feedbackAction: 'Senaryo işlemi başarılı.',
      feedbackDeleted: 'Senaryo ve bağlı takip sayfası soft-delete ile silindi.',
      confirmDelete: 'Senaryo silinsin mi? Bağlı takip sayfası da soft-delete ile kaldırılacak.',
      previewEmpty: '<p style="padding:8px">Önizleme boş.</p>'
    },
    en: {
      feedbackRefreshed: 'Campaign list refreshed.',
      feedbackRequired: 'Campaign name, page slug, and page title are required.',
      feedbackCreated: 'Campaign created. Public links are ready.',
      feedbackAction: 'Campaign action completed successfully.',
      feedbackDeleted: 'Campaign and linked tracking page were soft-deleted.',
      confirmDelete: 'Delete this campaign? The linked tracking page will also be soft-deleted.',
      previewEmpty: '<p style="padding:8px">Preview is empty.</p>'
    }
  };

  protected readonly IpAddressHashPolicy = IpAddressHashPolicy;
  protected readonly isBusy = signal(false);
  protected readonly feedback = signal<string | null>(null);
  protected readonly publicLinks = signal<{ slugUrl: string; idUrl: string } | null>(null);
  protected readonly campaigns = signal<CampaignResult[]>([]);
  protected readonly templates = signal<TemplateResult[]>([]);
  protected readonly canOperate = computed(() => this.authSessionService.hasRequiredRole('Operator'));
  protected readonly canDelete = computed(() => this.authSessionService.hasRequiredRole('Admin'));
  private readonly previewUrlCache = new Map<string, SafeResourceUrl>();
  private readonly objectUrls = new Set<string>();

  protected readonly createForm = {
    name: '',
    slug: '',
    pageTitle: '',
    templateId: null as string | null,
    htmlContent: '',
    validFromUtc: '',
    validUntilUtc: '',
    retentionDays: 365,
    captureIpAddress: true,
    ipAddressHashPolicy: IpAddressHashPolicy._2,
    enableBotFiltering: true,
    captureUtmParameters: true
  };

  protected readonly templateOptions = computed(() =>
    this.templates()
      .filter((template) => !!template.id)
      .map((template) => ({
        value: template.id!,
        label: template.name || template.id!
      })));

  public constructor(
    private readonly authSessionService: AuthSessionService,
    private readonly userPreferencesService: UserPreferencesService,
    private readonly sanitizer: DomSanitizer
  ) {
    void this.refresh();
  }

  public ngOnDestroy(): void {
    this.revokePreviewUrls();
  }

  protected activeLanguage(): AppLanguage {
    return this.userPreferencesService.language();
  }

  protected t(key: string): string {
    const language = this.userPreferencesService.language();
    return this.translations[language][key] ?? key;
  }

  protected async refresh(): Promise<void> {
    await this.execute(async () => {
      const [campaigns, templates] = await Promise.all([listCampaigns(), TemplateService.templateList()]);
      this.campaigns.set(campaigns);
      this.templates.set(templates);
      this.feedback.set(this.t('feedbackRefreshed'));
    });
  }

  protected async create(): Promise<void> {
    const name = this.createForm.name.trim();
    const trackingPageSlug = this.createForm.slug.trim();
    const trackingPageTitle = this.createForm.pageTitle.trim();

    if (!name || !trackingPageSlug || !trackingPageTitle) {
      this.feedback.set(this.t('feedbackRequired'));
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

      if (created.trackingPageId) {
        const page = await TrackingService.trackingPageGetById({ trackingPageId: created.trackingPageId });
        await TrackingService.trackingPageUpdate({
          trackingPageId: created.trackingPageId,
          requestBody: {
            slug: page.slug ?? trackingPageSlug,
            title: page.title ?? trackingPageTitle,
            description: page.description ?? null,
            templateId: page.templateId ?? this.createForm.templateId,
            customHtmlContent: page.customHtmlContent ?? (this.createForm.htmlContent.trim() || null),
            validFromUtc: page.validFromUtc ?? this.toUtcIso(this.createForm.validFromUtc),
            validUntilUtc: page.validUntilUtc ?? this.toUtcIso(this.createForm.validUntilUtc),
            retentionDays: this.createForm.retentionDays,
            captureIpAddress: this.createForm.captureIpAddress,
            ipAddressHashPolicy: this.createForm.captureIpAddress ? this.createForm.ipAddressHashPolicy : IpAddressHashPolicy._0,
            enableBotFiltering: this.createForm.enableBotFiltering,
            captureUtmParameters: this.createForm.captureUtmParameters
          }
        });
        await TrackingService.trackingPagePublish({ trackingPageId: created.trackingPageId });
      }

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
      this.createForm.retentionDays = 365;
      this.createForm.captureIpAddress = true;
      this.createForm.ipAddressHashPolicy = IpAddressHashPolicy._2;
      this.createForm.enableBotFiltering = true;
      this.createForm.captureUtmParameters = true;
      this.feedback.set(this.t('feedbackCreated'));
      this.campaigns.set(await listCampaigns());
    });
  }

  protected async transition(campaignId: string | undefined, action: 'start' | 'pause' | 'complete' | 'cancel'): Promise<void> {
    if (!campaignId) {
      return;
    }

    await this.execute(async () => {
      const updatedCampaign = await this.transitionCampaign(campaignId, action);
      this.applyUpdatedCampaign(updatedCampaign);
      this.feedback.set(this.t('feedbackAction'));
    });
  }

  protected async removeCampaign(campaignId: string | undefined): Promise<void> {
    if (!campaignId) {
      return;
    }

    if (!window.confirm(this.t('confirmDelete'))) {
      return;
    }

    await this.execute(async () => {
      await deleteCampaign(campaignId);
      this.campaigns.set(this.campaigns().filter((campaign) => campaign.id !== campaignId));
      this.feedback.set(this.t('feedbackDeleted'));
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

  protected previewUrl(): SafeResourceUrl {
    const customHtml = this.createForm.htmlContent.trim();
    let html = '';

    if (customHtml.length > 0) {
      html = customHtml;
    } else {
      const selectedTemplateHtml = this.templates()
        .find((template) => template.id === this.createForm.templateId)
        ?.htmlContent
        ?.trim();

      if (selectedTemplateHtml && selectedTemplateHtml.length > 0) {
        html = selectedTemplateHtml;
      } else {
        html = this.t('previewEmpty');
      }
    }

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

  private toUtcIso(value: string): string | null {
    return value ? new Date(value).toISOString() : null;
  }

  private async transitionCampaign(campaignId: string, action: 'start' | 'pause' | 'complete' | 'cancel'): Promise<CampaignResult> {
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

  private revokePreviewUrls(): void {
    for (const url of this.objectUrls) {
      URL.revokeObjectURL(url);
    }
    this.objectUrls.clear();
    this.previewUrlCache.clear();
  }
}
