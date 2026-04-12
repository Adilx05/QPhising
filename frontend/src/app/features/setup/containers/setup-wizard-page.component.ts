import { Component, OnDestroy, OnInit, computed, inject, signal } from '@angular/core';
import { FormBuilder, Validators } from '@angular/forms';
import { Router } from '@angular/router';

import { OpenAPI } from '../../../core/api/generated';
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
  private readonly formBuilder = inject(FormBuilder);
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
  protected readonly dbForm = this.formBuilder.group({
    useConnectionString: [false],
    host: [''],
    port: [5432, [Validators.min(1), Validators.max(65535)]],
    database: [''],
    username: [''],
    password: [''],
    connectionString: ['']
  });
  protected readonly dbOperation = signal<{
    kind: 'validate' | 'migrate' | null;
    success: boolean;
    message: string;
    category: string | null;
    pendingMigrationCount: number;
    lastAppliedMigration: string | null;
    latestKnownMigration: string | null;
    appliedMigrationCount: number;
  }>({
    kind: null,
    success: false,
    message: '',
    category: null,
    pendingMigrationCount: 0,
    lastAppliedMigration: null,
    latestKnownMigration: null,
    appliedMigrationCount: 0
  });
  protected readonly isValidatingDb = signal(false);
  protected readonly isApplyingMigrations = signal(false);
  protected readonly ssoForm = this.formBuilder.group({
    authority: ['', [Validators.required]],
    realm: ['', [Validators.required]],
    clientId: ['', [Validators.required]],
    clientSecret: ['', [Validators.required]],
    audience: ['', [Validators.required]]
  });
  protected readonly ssoOperation = signal<{
    success: boolean;
    message: string;
    technicalReason: string | null;
    fieldErrors: Record<string, string[]>;
  } | null>(null);
  protected readonly isValidatingSso = signal(false);

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

  protected async validateDatabaseConnection(): Promise<void> {
    this.isValidatingDb.set(true);
    await this.executeDbAction('/api/setup/validate-db', 'validate');
    this.isValidatingDb.set(false);
  }

  protected async applyMigrations(): Promise<void> {
    this.isApplyingMigrations.set(true);
    await this.executeDbAction('/api/setup/apply-migrations', 'migrate');
    this.isApplyingMigrations.set(false);
  }

  protected async validateSsoConfiguration(): Promise<void> {
    this.isValidatingSso.set(true);

    try {
      const response = await fetch(`${OpenAPI.BASE}/api/setup/validate-sso`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          Authorization: `Bearer ${this.setupStateService.resolveAccessToken()}`
        },
        body: JSON.stringify(this.ssoForm.getRawValue())
      });

      const payload = (await response.json()) as {
        isValid: boolean;
        message: string;
        technicalReason?: string | null;
        fieldErrors?: Record<string, string[]>;
      };

      this.ssoOperation.set({
        success: payload.isValid,
        message: payload.message,
        technicalReason: payload.technicalReason ?? null,
        fieldErrors: payload.fieldErrors ?? {}
      });

      if (payload.isValid) {
        await this.refreshStatus();
      }
    } catch (error) {
      this.ssoOperation.set({
        success: false,
        message: error instanceof Error ? error.message : 'SSO validation failed.',
        technicalReason: 'network_error',
        fieldErrors: {
          authority: ['Unable to reach SSO validation endpoint.']
        }
      });
    } finally {
      this.isValidatingSso.set(false);
    }
  }

  private startStatusPolling(): void {
    this.pollHandle = setInterval(() => {
      void this.refreshStatus();
    }, 10000);
  }

  private async executeDbAction(path: string, kind: 'validate' | 'migrate'): Promise<void> {
    try {
      const response = await fetch(`${OpenAPI.BASE}${path}`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          Authorization: `Bearer ${this.setupStateService.resolveAccessToken()}`
        },
        body: JSON.stringify(this.toDbPayload())
      });

      const payload = (await response.json()) as {
        isValid?: boolean;
        isSuccess?: boolean;
        message: string;
        errorCategory?: string | null;
        pendingMigrationCount?: number;
        lastAppliedMigration?: string | null;
        latestKnownMigration?: string | null;
        appliedMigrationCount?: number;
      };

      this.dbOperation.set({
        kind,
        success: payload.isValid ?? payload.isSuccess ?? false,
        message: payload.message,
        category: payload.errorCategory ?? null,
        pendingMigrationCount: payload.pendingMigrationCount ?? 0,
        lastAppliedMigration: payload.lastAppliedMigration ?? null,
        latestKnownMigration: payload.latestKnownMigration ?? null,
        appliedMigrationCount: payload.appliedMigrationCount ?? 0
      });
    } catch (error) {
      this.dbOperation.set({
        kind,
        success: false,
        message: error instanceof Error ? error.message : 'Database operation failed.',
        category: 'network',
        pendingMigrationCount: 0,
        lastAppliedMigration: null,
        latestKnownMigration: null,
        appliedMigrationCount: 0
      });
    }
  }

  private toDbPayload(): Record<string, unknown> {
    const formValue = this.dbForm.getRawValue();
    if (formValue.useConnectionString) {
      return {
        connectionString: formValue.connectionString
      };
    }

    return {
      host: formValue.host,
      port: formValue.port,
      database: formValue.database,
      username: formValue.username,
      password: formValue.password
    };
  }
}
