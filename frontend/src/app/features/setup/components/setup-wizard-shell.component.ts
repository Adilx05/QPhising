import { Component, EventEmitter, Input, Output } from '@angular/core';

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

  @Output() readonly back = new EventEmitter<void>();
  @Output() readonly next = new EventEmitter<void>();
  @Output() readonly refresh = new EventEmitter<void>();


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
}
