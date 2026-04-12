import { Component, EventEmitter, Input, Output } from '@angular/core';
import { FormGroup } from '@angular/forms';

import { SetupStateSnapshot } from '../../../core/setup/setup-state.service';

interface SetupWizardStep {
  readonly title: string;
  readonly description: string;
  readonly endpoint: string;
}

@Component({
  standalone: false,
  selector: 'app-setup-wizard-shell',
  templateUrl: './setup-wizard-shell.component.html'
})
export class SetupWizardShellComponent {
  @Input({ required: true }) steps: readonly SetupWizardStep[] = [];
  @Input({ required: true }) activeStepIndex = 0;
  @Input({ required: true }) canGoBack = false;
  @Input({ required: true }) canGoNext = false;
  @Input({ required: true }) state!: SetupStateSnapshot;
  @Input({ required: true }) dbForm!: FormGroup;
  @Input({ required: true }) dbOperation!: {
    kind: 'validate' | 'migrate' | null;
    success: boolean;
    message: string;
    category: string | null;
    pendingMigrationCount: number;
    lastAppliedMigration: string | null;
    latestKnownMigration: string | null;
    appliedMigrationCount: number;
  };
  @Input({ required: true }) isValidatingDb = false;
  @Input({ required: true }) isApplyingMigrations = false;
  @Input({ required: true }) ssoForm!: FormGroup;
  @Input({ required: true }) ssoOperation!: {
    success: boolean;
    message: string;
    technicalReason: string | null;
    fieldErrors: Record<string, string[]>;
  } | null;
  @Input({ required: true }) isValidatingSso = false;

  @Output() readonly back = new EventEmitter<void>();
  @Output() readonly next = new EventEmitter<void>();
  @Output() readonly refresh = new EventEmitter<void>();
  @Output() readonly testDbConnection = new EventEmitter<void>();
  @Output() readonly applyMigrations = new EventEmitter<void>();
  @Output() readonly validateSso = new EventEmitter<void>();


  protected get currentStep(): SetupWizardStep {
    return (
      this.steps[this.activeStepIndex] ??
      this.steps[0] ?? {
        title: 'Setup',
        description: 'Setup steps are loading.',
        endpoint: 'N/A'
      }
    );
  }

  protected toPrimeStepModel(): Array<{ label: string }> {
    return this.steps.map((step) => ({ label: step.title }));
  }

  protected getActionableErrorMessage(category: string | null): string {
    switch (category) {
      case 'auth':
        return 'Authentication failed. Check DB username/password and credential permissions.';
      case 'network':
        return 'Network issue detected. Verify host/port, firewall rules, and DB service availability.';
      case 'db_not_found':
        return 'Database was not found. Create the database or fix the database name in the form.';
      default:
        return 'Review details and adjust configuration before retrying.';
    }
  }

  protected getSsoErrorEntries(fieldErrors: Record<string, string[]>): Array<{ field: string; message: string }> {
    return Object.entries(fieldErrors).flatMap(([field, messages]) =>
      messages.map((message) => ({ field, message }))
    );
  }
}
