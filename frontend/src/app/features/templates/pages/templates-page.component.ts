import { CommonModule } from '@angular/common';
import { Component, OnDestroy, computed, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { DomSanitizer, SafeResourceUrl } from '@angular/platform-browser';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { resolveApiError } from '../../../core/http/api-error-handler';
import { AuthSessionService } from '../../../core/auth/auth-session';
import { UserPreferencesService } from '../../../core/ui/user-preferences.service';
import { TemplateLifecycleState, type TemplateResult } from '../../../shared/proxy';
import {
  archiveTemplate,
  createTemplate,
  deleteTemplate,
  listTemplates,
  publishTemplate,
  updateTemplate,
  type UpsertTemplateInput
} from '../data-access';

interface TemplateDraft {
  name: string;
  htmlContent: string;
  description: string;
  tags: string;
}

@Component({
  selector: 'app-templates-page',
  standalone: true,
  imports: [CommonModule, FormsModule, ButtonModule, InputTextModule],
  template: `
    <section class="mb-6">
      <h1 class="page-title">{{ tx('HTML Şablon Yönetimi', 'HTML Template Management') }}</h1>
      <p class="page-subtitle">{{ tx('Bu ekran e-posta yerine doğrudan HTML sayfa şablonları üretmek için kullanılır.', 'This page is used to manage direct HTML page templates (not email templates).') }}</p>
    </section>

    <section class="surface-card p-5">
      <h2 class="text-base font-semibold text-slate-900">{{ tx('HTML Şablon Oluştur', 'Create HTML Template') }}</h2>
      <div class="mt-3 grid gap-3 md:grid-cols-2">
        <input pInputText [(ngModel)]="createForm.name" class="w-full" [placeholder]="tx('Şablon adı', 'Template name')" />
        <input pInputText [(ngModel)]="createForm.description" class="w-full" [placeholder]="tx('Açıklama', 'Description')" />
        <input pInputText [(ngModel)]="createForm.tags" class="w-full md:col-span-2" [placeholder]="tx('Etiketler (virgülle)', 'Tags (comma-separated)')" />
        <textarea [(ngModel)]="createForm.htmlContent" class="min-h-48 w-full rounded-xl border border-slate-300 px-3 py-2 text-sm md:col-span-2" placeholder="<html>...</html>"></textarea>
      </div>

      <div class="mt-4 rounded-xl border border-slate-200 bg-slate-50 p-3 md:col-span-2">
        <p class="text-xs font-semibold uppercase text-slate-500">{{ tx('Canlı Önizleme', 'Live Preview') }}</p>
        <iframe class="mt-2 h-64 w-full rounded-lg border border-slate-200 bg-white" [src]="previewUrl(createForm.htmlContent)"></iframe>
      </div>

      <button
        class="mt-3"
        pButton
        type="button"
        icon="pi pi-plus"
        [label]="tx('Oluştur', 'Create')"
        [disabled]="!canOperate() || isBusy()"
        [loading]="isBusy()"
        (click)="create()"
      ></button>

      <p *ngIf="feedback()" class="mt-4 rounded-xl border border-blue-100 bg-blue-50 px-3 py-2 text-sm text-blue-800">
        {{ feedback() }}
      </p>
    </section>

    <section class="mt-6 grid gap-4">
      <article *ngFor="let templateItem of templates()" class="surface-card p-5">
        <div class="flex flex-wrap items-start justify-between gap-3">
          <div>
            <h3 class="text-lg font-semibold text-slate-900">{{ templateItem.name || tx('Adsız Şablon', 'Unnamed Template') }}</h3>
            <p class="text-sm text-slate-500">{{ tx('Kimlik', 'ID') }}: {{ templateItem.id }}</p>
          </div>

          <span class="status-chip" [ngClass]="stateChipClass(templateItem.lifecycleState)">
            {{ stateLabel(templateItem.lifecycleState) }}
          </span>
        </div>

        <div class="mt-3 grid gap-3 lg:grid-cols-2">
          <div class="rounded-xl border border-slate-200 bg-slate-50/70 p-3">
            <p class="text-xs font-semibold uppercase text-slate-500">{{ tx('Etiketler', 'Tags') }}</p>
            <p class="mt-1 text-sm text-slate-700">{{ (templateItem.tags || []).join(', ') || '-' }}</p>
          </div>
          <div class="rounded-xl border border-slate-200 bg-slate-50/70 p-3">
            <p class="text-xs font-semibold uppercase text-slate-500">{{ tx('Açıklama', 'Description') }}</p>
            <p class="mt-1 text-sm text-slate-700">{{ templateItem.description || '-' }}</p>
          </div>
        </div>

        <div class="mt-4 rounded-xl border border-slate-200 p-4">
          <p class="text-xs font-semibold uppercase text-slate-500">{{ tx('Şablonu Güncelle', 'Update Template') }}</p>
          <div class="mt-2 grid gap-2 md:grid-cols-2">
            <input pInputText class="w-full" [placeholder]="tx('Şablon adı', 'Template name')" [ngModel]="editDrafts()[templateItem.id || ''].name || ''" (ngModelChange)="setDraftField(templateItem.id, 'name', $event)" />
            <input pInputText class="w-full" [placeholder]="tx('Açıklama', 'Description')" [ngModel]="editDrafts()[templateItem.id || ''].description || ''" (ngModelChange)="setDraftField(templateItem.id, 'description', $event)" />
            <input pInputText class="w-full md:col-span-2" [placeholder]="tx('Etiketler (virgülle)', 'Tags (comma-separated)')" [ngModel]="editDrafts()[templateItem.id || ''].tags || ''" (ngModelChange)="setDraftField(templateItem.id, 'tags', $event)" />
            <textarea class="min-h-48 w-full rounded-xl border border-slate-300 px-3 py-2 text-sm md:col-span-2" placeholder="<html>...</html>" [ngModel]="editDrafts()[templateItem.id || ''].htmlContent || ''" (ngModelChange)="setDraftField(templateItem.id, 'htmlContent', $event)"></textarea>
          </div>

          <div class="mt-4 rounded-xl border border-slate-200 bg-slate-50 p-3">
            <p class="text-xs font-semibold uppercase text-slate-500">{{ tx('Önizleme', 'Preview') }}</p>
            <iframe class="mt-2 h-64 w-full rounded-lg border border-slate-200 bg-white" [src]="previewUrl(editDrafts()[templateItem.id || ''].htmlContent || '')"></iframe>
          </div>

          <div class="mt-3 flex flex-wrap gap-2">
            <button pButton type="button" size="small" [label]="tx('Kaydet', 'Save')" [disabled]="!canOperate() || isBusy()" (click)="save(templateItem.id)"></button>
            <button pButton type="button" size="small" severity="danger" [label]="tx('Sil', 'Delete')" [disabled]="!canOperate() || isBusy()" (click)="remove(templateItem.id)"></button>
          </div>
        </div>
      </article>

      <article *ngIf="templates().length === 0" class="surface-card p-5 text-sm text-slate-500">
        {{ tx('Henüz şablon kaydı bulunmuyor.', 'No templates found yet.') }}
      </article>
    </section>
  `
})
export class TemplatesPageComponent implements OnDestroy {
  protected readonly isBusy = signal(false);
  protected readonly feedback = signal<string | null>(null);
  protected readonly templates = signal<TemplateResult[]>([]);
  protected readonly editDrafts = signal<Record<string, TemplateDraft>>({});
  protected readonly canOperate = computed(() => this.authSessionService.hasRequiredRole('Operator'));
  private readonly previewUrlCache = new Map<string, SafeResourceUrl>();
  private readonly objectUrls = new Set<string>();

  protected readonly createForm: TemplateDraft = {
    name: '',
    htmlContent: '',
    description: '',
    tags: ''
  };

  public constructor(
    private readonly authSessionService: AuthSessionService,
    private readonly sanitizer: DomSanitizer,
    private readonly userPreferencesService: UserPreferencesService
  ) {
    void this.refresh();
  }

  protected tx(tr: string, en: string): string {
    return this.userPreferencesService.language() === 'tr' ? tr : en;
  }

  public ngOnDestroy(): void {
    this.revokePreviewUrls();
  }

  protected async create(): Promise<void> {
    const input = this.toUpsertInput(this.createForm);
    if (!input) {
      return;
    }

    await this.execute(async () => {
      await createTemplate(input);
      this.createForm.name = '';
      this.createForm.htmlContent = '';
      this.createForm.description = '';
      this.createForm.tags = '';
      this.feedback.set(this.tx('HTML şablonu oluşturuldu.', 'HTML template created.'));
      await this.refresh();
    });
  }

  protected async save(templateId: string | undefined): Promise<void> {
    if (!templateId) {
      return;
    }

    const draft = this.editDrafts()[templateId];
    const input = draft ? this.toUpsertInput(draft) : null;
    if (!input) {
      return;
    }

    await this.execute(async () => {
      const updated = await updateTemplate(templateId, input);
      this.applyUpdatedTemplate(updated);
      this.feedback.set(this.tx('Şablon güncellendi.', 'Template updated.'));
    });
  }

  protected async publish(templateId: string | undefined): Promise<void> {
    if (!templateId) {
      return;
    }

    await this.execute(async () => {
      this.applyUpdatedTemplate(await publishTemplate(templateId));
      this.feedback.set(this.tx('Şablon yayımlandı.', 'Template published.'));
    });
  }

  protected async archive(templateId: string | undefined): Promise<void> {
    if (!templateId) {
      return;
    }

    await this.execute(async () => {
      this.applyUpdatedTemplate(await archiveTemplate(templateId));
      this.feedback.set(this.tx('Şablon arşivlendi.', 'Template archived.'));
    });
  }

  protected async remove(templateId: string | undefined): Promise<void> {
    if (!templateId) {
      return;
    }

    await this.execute(async () => {
      await deleteTemplate(templateId);
      this.templates.set(this.templates().filter((item) => item.id !== templateId));
      this.editDrafts.update((drafts) => {
        const nextDrafts = { ...drafts };
        delete nextDrafts[templateId];
        return nextDrafts;
      });
      this.feedback.set(this.tx('Şablon silindi.', 'Template deleted.'));
    });
  }

  protected setDraftField(templateId: string | undefined, field: keyof TemplateDraft, value: string): void {
    if (!templateId) {
      return;
    }

    const existingDraft = this.editDrafts()[templateId] ?? this.mapTemplateToDraft(this.templates().find((item) => item.id === templateId));
    this.editDrafts.update((drafts) => ({
      ...drafts,
      [templateId]: {
        ...existingDraft,
        [field]: value
      }
    }));
  }

  protected stateLabel(state: TemplateLifecycleState | undefined): string {
    switch (state) {
      case TemplateLifecycleState._1:
        return this.tx('Yayımlandı', 'Published');
      case TemplateLifecycleState._2:
        return this.tx('Arşivlendi', 'Archived');
      default:
        return this.tx('Taslak', 'Draft');
    }
  }

  protected stateChipClass(state: TemplateLifecycleState | undefined): string {
    switch (state) {
      case TemplateLifecycleState._1:
        return 'status-chip--success';
      case TemplateLifecycleState._2:
        return 'status-chip--muted';
      default:
        return 'status-chip--warning';
    }
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

  private async refresh(): Promise<void> {
    await this.execute(async () => {
      const templates = await listTemplates();
      this.templates.set(templates);
      this.editDrafts.set(
        templates.reduce<Record<string, TemplateDraft>>((drafts, templateItem) => {
          if (templateItem.id) {
            drafts[templateItem.id] = this.mapTemplateToDraft(templateItem);
          }
          return drafts;
        }, {})
      );
      this.feedback.set(this.tx('Şablon listesi güncellendi.', 'Template list refreshed.'));
    });
  }

  private mapTemplateToDraft(templateItem: TemplateResult | undefined): TemplateDraft {
    return {
      name: templateItem?.name ?? '',
      htmlContent: templateItem?.htmlContent ?? '',
      description: templateItem?.description ?? '',
      tags: (templateItem?.tags ?? []).join(', ')
    };
  }

  private toUpsertInput(draft: TemplateDraft): UpsertTemplateInput | null {
    const name = draft.name.trim();
    const htmlContent = draft.htmlContent.trim();

    if (name.length === 0 || htmlContent.length === 0) {
      this.feedback.set(this.tx('Şablon adı ve HTML içeriği zorunludur.', 'Template name and HTML content are required.'));
      return null;
    }

    return {
      name,
      htmlContent,
      description: draft.description.trim(),
      tags: draft.tags
        .split(',')
        .map((tag) => tag.trim())
        .filter((tag) => tag.length > 0)
    };
  }

  private applyUpdatedTemplate(updatedTemplate: TemplateResult): void {
    this.templates.set(
      this.templates().map((item) => (item.id === updatedTemplate.id ? updatedTemplate : item))
    );

    const updatedId = updatedTemplate.id;
    if (typeof updatedId === 'string' && updatedId.length > 0) {
      this.editDrafts.update((drafts) => ({
        ...drafts,
        [updatedId]: this.mapTemplateToDraft(updatedTemplate)
      }));
    }
  }

  private async execute(action: () => Promise<void>): Promise<void> {
    if (this.isBusy()) {
      return;
    }

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
    this.objectUrls.forEach((url) => URL.revokeObjectURL(url));
    this.objectUrls.clear();
    this.previewUrlCache.clear();
  }
}
