import { CommonModule } from '@angular/common';
import { Component, computed, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { PasswordModule } from 'primeng/password';
import { SetupReadinessState, type SetupDependencyTestResult, type SetupStatusResult } from '../../../shared/proxy';
import { resolveApiError } from '../../../core/http/api-error-handler';
import { UserPreferencesService } from '../../../core/ui/user-preferences.service';
import {
  getSetupStatus,
  saveSetupConfiguration,
  testDatabaseConnection,
  testKeycloakConnection
} from '../data-access/setup-flow.client';

@Component({
  selector: 'app-setup-wizard-page',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, ButtonModule, InputTextModule, PasswordModule],
  templateUrl: './setup-wizard-page.component.html'
})
export class SetupWizardPageComponent {
  protected readonly isBusy = signal(false);
  protected readonly status = signal<SetupStatusResult | null>(null);
  protected readonly feedback = signal<string | null>(null);
  protected readonly dbTestResult = signal<SetupDependencyTestResult | null>(null);
  protected readonly keycloakTestResult = signal<SetupDependencyTestResult | null>(null);
  protected readonly setupCompleted = computed(() => this.status()?.readinessState === SetupReadinessState._2);

  protected readonly form = this.formBuilder.nonNullable.group({
    databaseConnectionString: ['', [Validators.required]],
    keycloakAuthority: ['', [Validators.required]],
    keycloakRealm: ['', [Validators.required]],
    keycloakClientId: ['', [Validators.required]],
    keycloakClientSecret: ['', [Validators.required]]
  });

  public constructor(
    private readonly formBuilder: FormBuilder,
    private readonly userPreferencesService: UserPreferencesService
  ) {
    void this.loadStatus();
  }

  protected tx(tr: string, en: string): string {
    return this.userPreferencesService.language() === 'tr' ? tr : en;
  }

  protected async testDatabase(): Promise<void> {
    await this.executeAsync(async () => {
      this.dbTestResult.set(await testDatabaseConnection(this.form.controls.databaseConnectionString.getRawValue()));
      this.feedback.set(this.tx('Veritabanı bağlantı testi tamamlandı.', 'Database connection test completed.'));
    });
  }

  protected async testKeycloak(): Promise<void> {
    await this.executeAsync(async () => {
      const { keycloakAuthority, keycloakRealm, keycloakClientId, keycloakClientSecret } = this.form.getRawValue();

      this.keycloakTestResult.set(
        await testKeycloakConnection(keycloakAuthority, keycloakRealm, keycloakClientId, keycloakClientSecret)
      );
      this.feedback.set(this.tx('Keycloak bağlantı testi tamamlandı.', 'Keycloak connection test completed.'));
    });
  }

  protected async save(): Promise<void> {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    await this.executeAsync(async () => {
      const value = this.form.getRawValue();
      this.status.set(await saveSetupConfiguration(value));
      this.feedback.set(this.tx('Kurulum ayarları başarıyla kaydedildi.', 'Setup settings saved successfully.'));
    });
  }

  private async loadStatus(): Promise<void> {
    await this.executeAsync(async () => {
      this.status.set(await getSetupStatus());
    });
  }

  private async executeAsync(work: () => Promise<void>): Promise<void> {
    this.feedback.set(null);
    this.isBusy.set(true);

    try {
      await work();
    } catch (error) {
      this.feedback.set(this.resolveErrorMessage(error));
    } finally {
      this.isBusy.set(false);
    }
  }

  private resolveErrorMessage(error: unknown): string {
    return resolveApiError(error).message;
  }
}
