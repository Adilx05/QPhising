import { Component, OnDestroy, OnInit, computed, inject, signal } from '@angular/core';
import { Router } from '@angular/router';

import { SetupStateService } from '../../../core/setup/setup-state.service';

interface SetupWizardStep {
  readonly title: string;
  readonly description: string;
  readonly endpoint: string;
}

@Component({
  standalone: false,
  selector: 'app-setup-wizard-page',
  templateUrl: './setup-wizard-page.component.html'
})
export class SetupWizardPageComponent implements OnInit, OnDestroy {
  private readonly setupStateService = inject(SetupStateService);
  private readonly router = inject(Router);
  private pollHandle: ReturnType<typeof setInterval> | null = null;

  protected readonly activeStepIndex = signal(0);
  protected readonly setupState = this.setupStateService.state;
  protected readonly steps: readonly SetupWizardStep[] = [
    {
      title: 'Database Validation',
      description: 'Verify database connectivity and schema readiness for production modules.',
      endpoint: 'POST /api/setup/validate-db'
    },
    {
      title: 'SSO Validation',
      description: 'Validate Keycloak realm and admin-level token exchange integrity.',
      endpoint: 'POST /api/setup/validate-sso'
    },
    {
      title: 'Finalize Setup',
      description: 'Commit setup state and unlock dashboard and operational modules.',
      endpoint: 'POST /api/setup/finalize'
    }
  ];

  protected readonly canGoBack = computed(() => this.activeStepIndex() > 0);
  protected readonly canGoNext = computed(() => this.activeStepIndex() < this.steps.length - 1);

  async ngOnInit(): Promise<void> {
    await this.refreshStatus();
    this.startStatusPolling();
  }

  ngOnDestroy(): void {
    if (this.pollHandle !== null) {
      clearInterval(this.pollHandle);
      this.pollHandle = null;
    }
  }

  protected goBack(): void {
    if (!this.canGoBack()) {
      return;
    }

    this.activeStepIndex.update((index) => index - 1);
  }

  protected goNext(): void {
    if (!this.canGoNext()) {
      return;
    }

    this.activeStepIndex.update((index) => index + 1);
  }

  protected async refreshStatus(): Promise<void> {
    const state = await this.setupStateService.refreshStatus();

    if (state.isCompleted) {
      await this.router.navigate(['/dashboard']);
    }
  }

  private startStatusPolling(): void {
    this.pollHandle = setInterval(() => {
      void this.refreshStatus();
    }, 10000);
  }
}
