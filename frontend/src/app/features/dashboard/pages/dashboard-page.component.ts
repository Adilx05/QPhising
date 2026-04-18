import { CommonModule } from '@angular/common';
import { Component, signal } from '@angular/core';
import { ButtonModule } from 'primeng/button';
import {
  SetupReadinessState,
  type RuntimeConfigurationResult,
  type SetupGuardDecisionResult,
  type SetupStatusResult
} from '../../../shared/proxy';
import { resolveApiError } from '../../../core/http/api-error-handler';
import { getRuntimeConfigurationStatus, getSetupGuardDecision, getSetupStatus } from '../../setup/data-access';

@Component({
  selector: 'app-dashboard-page',
  standalone: true,
  imports: [CommonModule, ButtonModule],
  template: `
    <section class="mb-6">
      <h1 class="page-title">Security Operations Dashboard</h1>
      <p class="page-subtitle">
        Setup ve runtime durumları doğrudan backend API sonuçlarından okunur.
      </p>
    </section>

    <div class="grid gap-4 md:grid-cols-2 xl:grid-cols-3">
      <article class="surface-card p-5">
        <p class="text-xs font-semibold uppercase tracking-wide text-slate-500">Setup Access State</p>
        <p class="mt-3 text-2xl font-semibold text-slate-900">{{ guardDecision()?.accessState ?? 'Unknown' }}</p>
        <span class="mt-3 status-chip" [ngClass]="guardAllowsMainApp() ? 'status-chip-success' : 'status-chip-warning'">
          {{ guardAllowsMainApp() ? 'Main App Enabled' : 'Setup Required' }}
        </span>
      </article>

      <article class="surface-card p-5">
        <p class="text-xs font-semibold uppercase tracking-wide text-slate-500">Setup Readiness</p>
        <p class="mt-3 text-2xl font-semibold text-slate-900">{{ setupStatus()?.readinessState ?? 'Unknown' }}</p>
        <span class="mt-3 status-chip" [ngClass]="setupCompleted() ? 'status-chip-success' : 'status-chip-muted'">
          {{ setupCompleted() ? 'Completed' : 'Incomplete' }}
        </span>
      </article>

      <article class="surface-card p-5">
        <p class="text-xs font-semibold uppercase tracking-wide text-slate-500">Protected Runtime</p>
        <p class="mt-3 text-2xl font-semibold text-slate-900">
          {{ runtimeStatus()?.isReadyForProtectedRuntime ? 'Ready' : 'Not Ready' }}
        </p>
        <span
          class="mt-3 status-chip"
          [ngClass]="runtimeStatus()?.isReadyForProtectedRuntime ? 'status-chip-success' : 'status-chip-warning'"
        >
          {{ runtimeStatus()?.updatedAtUtc ? 'Updated' : 'No Persisted Configuration' }}
        </span>
      </article>
    </div>

    <section class="surface-card mt-6 p-5">
      <div class="flex flex-wrap items-center justify-between gap-3">
        <div>
          <h2 class="text-base font-semibold text-slate-900">Environment Health Overview</h2>
          <p class="mt-1 text-sm text-slate-500">Bu özet gerçek setup/configuration endpoint cevaplarını gösterir.</p>
        </div>

        <button pButton type="button" label="Yenile" icon="pi pi-refresh" [loading]="isBusy()" (click)="refresh()"></button>
      </div>

      <div class="mt-4 grid gap-3 md:grid-cols-3">
        <div class="rounded-xl border border-slate-200 bg-slate-50/80 px-4 py-3">
          <p class="text-xs font-semibold uppercase text-slate-500">Database</p>
          <p class="mt-1 text-sm font-medium text-slate-700">{{ runtimeStatus()?.isDatabaseConfigured ? 'Configured' : 'Missing' }}</p>
        </div>
        <div class="rounded-xl border border-slate-200 bg-slate-50/80 px-4 py-3">
          <p class="text-xs font-semibold uppercase text-slate-500">Redis</p>
          <p class="mt-1 text-sm font-medium text-slate-700">{{ runtimeStatus()?.isRedisConfigured ? 'Configured' : 'Missing' }}</p>
        </div>
        <div class="rounded-xl border border-slate-200 bg-slate-50/80 px-4 py-3">
          <p class="text-xs font-semibold uppercase text-slate-500">Keycloak</p>
          <p class="mt-1 text-sm font-medium text-slate-700">{{ runtimeStatus()?.isKeycloakConfigured ? 'Configured' : 'Missing' }}</p>
        </div>
      </div>

      <p *ngIf="feedback()" class="mt-4 rounded-xl border border-blue-100 bg-blue-50 px-3 py-2 text-sm text-blue-800">
        {{ feedback() }}
      </p>
    </section>
  `
})
export class DashboardPageComponent {
  protected readonly isBusy = signal(false);
  protected readonly feedback = signal<string | null>(null);
  protected readonly guardDecision = signal<SetupGuardDecisionResult | null>(null);
  protected readonly setupStatus = signal<SetupStatusResult | null>(null);
  protected readonly runtimeStatus = signal<RuntimeConfigurationResult | null>(null);

  public constructor() {
    void this.refresh();
  }

  protected guardAllowsMainApp(): boolean {
    return this.guardDecision()?.allowMainApplication === true;
  }

  protected setupCompleted(): boolean {
    return this.setupStatus()?.readinessState === SetupReadinessState._2;
  }

  protected async refresh(): Promise<void> {
    this.feedback.set(null);
    this.isBusy.set(true);

    try {
      const [guardDecision, setupStatus, runtimeStatus] = await Promise.all([
        getSetupGuardDecision(),
        getSetupStatus(),
        getRuntimeConfigurationStatus()
      ]);

      this.guardDecision.set(guardDecision);
      this.setupStatus.set(setupStatus);
      this.runtimeStatus.set(runtimeStatus);
      this.feedback.set('Dashboard state başarıyla güncellendi.');
    } catch (error) {
      this.feedback.set(resolveApiError(error).message);
    } finally {
      this.isBusy.set(false);
    }
  }
}
