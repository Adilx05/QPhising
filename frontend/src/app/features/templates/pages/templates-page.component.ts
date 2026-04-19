import { CommonModule } from '@angular/common';
import { Component, computed, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { resolveApiError } from '../../../core/http/api-error-handler';
import { AuthSessionService } from '../../../core/auth/auth-session';
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
  subject: string;
  body: string;
  description: string;
  tags: string;
}

@Component({
  selector: 'app-templates-page',
  standalone: true,
  imports: [CommonModule, FormsModule, ButtonModule, InputTextModule],
  template: `
    <section class="mb-6">
      <h1 class="page-title">Template Management</h1>
      <p class="page-subtitle">Template işlemleri generated TemplateService proxy çağrıları üzerinden yürütülür.</p>
    </section>

    <section class="surface-card p-5">
      <h2 class="text-base font-semibold text-slate-900">Create Template</h2>
      <div class="mt-3 grid gap-3 md:grid-cols-2">
        <input pInputText [(ngModel)]="createForm.name" class="w-full" placeholder="Template name" />
        <input pInputText [(ngModel)]="createForm.subject" class="w-full" placeholder="Page headline" />
        <input pInputText [(ngModel)]="createForm.description" class="w-full md:col-span-2" placeholder="Description" />
        <input pInputText [(ngModel)]="createForm.body" class="w-full md:col-span-2" placeholder="Body" />
        <input pInputText [(ngModel)]="createForm.tags" class="w-full md:col-span-2" placeholder="Tags (comma-separated)" />
      </div>

      <button
        class="mt-3"
        pButton
        type="button"
        icon="pi pi-plus"
        label="Create"
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
            <h3 class="text-lg font-semibold text-slate-900">{{ templateItem.name || 'Unnamed Template' }}</h3>
            <p class="text-sm text-slate-500">ID: {{ templateItem.id }}</p>
          </div>

          <span class="status-chip" [ngClass]="stateChipClass(templateItem.lifecycleState)">
            {{ stateLabel(templateItem.lifecycleState) }}
          </span>
        </div>

        <div class="mt-3 grid gap-3 lg:grid-cols-2">
          <div class="rounded-xl border border-slate-200 bg-slate-50/70 p-3">
            <p class="text-xs font-semibold uppercase text-slate-500">Subject</p>
            <p class="mt-1 text-sm text-slate-700">{{ templateItem.subject || '-' }}</p>
          </div>

          <div class="rounded-xl border border-slate-200 bg-slate-50/70 p-3">
            <p class="text-xs font-semibold uppercase text-slate-500">Tags</p>
            <p class="mt-1 text-sm text-slate-700">{{ (templateItem.tags || []).join(', ') || '-' }}</p>
          </div>
        </div>

        <div class="mt-4 rounded-xl border border-slate-200 p-4">
          <p class="text-xs font-semibold uppercase text-slate-500">Update Template</p>
          <div class="mt-2 grid gap-2 md:grid-cols-2">
            <input
              pInputText
              class="w-full"
              placeholder="Template name"
              [ngModel]="editDrafts()[templateItem.id || ''].name || ''"
              (ngModelChange)="setDraftField(templateItem.id, 'name', $event)"
            />
            <input
              pInputText
              class="w-full"
              placeholder="Subject"
              [ngModel]="editDrafts()[templateItem.id || ''].subject || ''"
              (ngModelChange)="setDraftField(templateItem.id, 'subject', $event)"
            />
            <input
              pInputText
              class="w-full md:col-span-2"
              placeholder="Description"
              [ngModel]="editDrafts()[templateItem.id || ''].description || ''"
              (ngModelChange)="setDraftField(templateItem.id, 'description', $event)"
            />
            <input
              pInputText
              class="w-full md:col-span-2"
              placeholder="Body"
              [ngModel]="editDrafts()[templateItem.id || ''].body || ''"
              (ngModelChange)="setDraftField(templateItem.id, 'body', $event)"
            />
            <input
              pInputText
              class="w-full md:col-span-2"
              placeholder="Tags (comma-separated)"
              [ngModel]="editDrafts()[templateItem.id || ''].tags || ''"
              (ngModelChange)="setDraftField(templateItem.id, 'tags', $event)"
            />
          </div>

          <div class="mt-3 flex flex-wrap gap-2">
            <button pButton type="button" size="small" label="Save" [disabled]="!canOperate() || isBusy()" (click)="save(templateItem.id)"></button>
            <button pButton type="button" size="small" severity="success" label="Publish" [disabled]="!canOperate() || isBusy()" (click)="publish(templateItem.id)"></button>
            <button pButton type="button" size="small" severity="secondary" label="Archive" [disabled]="!canOperate() || isBusy()" (click)="archive(templateItem.id)"></button>
            <button pButton type="button" size="small" severity="danger" label="Delete" [disabled]="!canOperate() || isBusy()" (click)="remove(templateItem.id)"></button>
          </div>
        </div>
      </article>

      <article *ngIf="templates().length === 0" class="surface-card p-5 text-sm text-slate-500">
        Henüz template kaydı bulunmuyor.
      </article>
    </section>
  `
})
export class TemplatesPageComponent {
  protected readonly isBusy = signal(false);
  protected readonly feedback = signal<string | null>(null);
  protected readonly templates = signal<TemplateResult[]>([]);
  protected readonly editDrafts = signal<Record<string, TemplateDraft>>({});
  protected readonly canOperate = computed(() => this.authSessionService.hasRequiredRole('Operator'));

  protected readonly createForm: TemplateDraft = {
    name: '',
    subject: '',
    body: '',
    description: '',
    tags: ''
  };

  public constructor(private readonly authSessionService: AuthSessionService) {
    void this.refresh();
  }

  protected async create(): Promise<void> {
    const input = this.toUpsertInput(this.createForm);
    if (!input) {
      return;
    }

    await this.execute(async () => {
      await createTemplate(input);
      this.createForm.name = '';
      this.createForm.subject = '';
      this.createForm.body = '';
      this.createForm.description = '';
      this.createForm.tags = '';
      this.feedback.set('Template oluşturuldu.');
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
      this.feedback.set('Template güncellendi.');
    });
  }

  protected async publish(templateId: string | undefined): Promise<void> {
    if (!templateId) {
      return;
    }

    await this.execute(async () => {
      this.applyUpdatedTemplate(await publishTemplate(templateId));
      this.feedback.set('Template publish edildi.');
    });
  }

  protected async archive(templateId: string | undefined): Promise<void> {
    if (!templateId) {
      return;
    }

    await this.execute(async () => {
      this.applyUpdatedTemplate(await archiveTemplate(templateId));
      this.feedback.set('Template archive edildi.');
    });
  }

  protected async remove(templateId: string | undefined): Promise<void> {
    if (!templateId) {
      return;
    }

    await this.execute(async () => {
      await deleteTemplate(templateId);
      this.templates.set(this.templates().filter((item) => item.id !== templateId));
      const drafts = { ...this.editDrafts() };
      delete drafts[templateId];
      this.editDrafts.set(drafts);
      this.feedback.set('Template silindi.');
    });
  }

  protected setDraftField(templateId: string | undefined, field: keyof TemplateDraft, value: string): void {
    if (!templateId) {
      return;
    }

    const existingDraft = this.editDrafts()[templateId] ?? this.mapTemplateToDraft(this.templates().find((item) => item.id === templateId));

    this.editDrafts.set({
      ...this.editDrafts(),
      [templateId]: {
        ...existingDraft,
        [field]: value
      }
    });
  }

  protected stateLabel(state: TemplateLifecycleState | undefined): string {
    switch (state) {
      case TemplateLifecycleState._1:
        return 'Published';
      case TemplateLifecycleState._2:
        return 'Archived';
      default:
        return 'Draft';
    }
  }

  protected stateChipClass(state: TemplateLifecycleState | undefined): string {
    switch (state) {
      case TemplateLifecycleState._1:
        return 'status-chip-success';
      case TemplateLifecycleState._2:
        return 'status-chip-muted';
      default:
        return 'status-chip-warning';
    }
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

      this.feedback.set('Template listesi güncellendi.');
    });
  }

  private mapTemplateToDraft(templateItem: TemplateResult | undefined): TemplateDraft {
    return {
      name: templateItem?.name ?? '',
      subject: templateItem?.subject ?? '',
      body: templateItem?.body ?? '',
      description: templateItem?.description ?? '',
      tags: (templateItem?.tags ?? []).join(', ')
    };
  }

  private toUpsertInput(draft: TemplateDraft): UpsertTemplateInput | null {
    const name = draft.name.trim();
    const subject = draft.subject.trim();
    const body = draft.body.trim();

    if (name.length === 0 || subject.length === 0 || body.length === 0) {
      this.feedback.set('Template name, subject ve body alanları zorunludur.');
      return null;
    }

    return {
      name,
      subject,
      body,
      description: draft.description.trim(),
      tags: draft.tags
        .split(',')
        .map((item) => item.trim())
        .filter((item) => item.length > 0)
    };
  }

  private applyUpdatedTemplate(updatedTemplate: TemplateResult): void {
    this.templates.set(
      this.templates().map((item) => (item.id === updatedTemplate.id ? updatedTemplate : item))
    );

    if (updatedTemplate.id) {
      this.editDrafts.set({
        ...this.editDrafts(),
        [updatedTemplate.id]: this.mapTemplateToDraft(updatedTemplate)
      });
    }
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
