import { CommonModule } from '@angular/common';
import { Component, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { PasswordModule } from 'primeng/password';
import type { RuntimeConfigurationResult } from '../../../shared/proxy';
import { AuthSessionService } from '../../../core/auth/auth-session';
import { resolveApiError } from '../../../core/http/api-error-handler';
import { UserPreferencesService } from '../../../core/ui/user-preferences.service';
import {
  getRuntimeConfigurationStatus,
  saveRuntimeConfiguration,
  updateRuntimeConfiguration,
  type RuntimeConfigurationInput
} from '../data-access';

@Component({
  selector: 'app-runtime-configuration-page',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, ButtonModule, InputTextModule, PasswordModule],
  templateUrl: './runtime-configuration-page.component.html'
})
export class RuntimeConfigurationPageComponent {
  protected readonly isBusy = signal(false);
  protected readonly status = signal<RuntimeConfigurationResult | null>(null);
  protected readonly feedback = signal<string | null>(null);

  protected readonly form = this.formBuilder.nonNullable.group({
    databaseConnectionString: ['', [Validators.required]],
    keycloakAuthority: ['', [Validators.required]],
    keycloakRealm: ['', [Validators.required]],
    keycloakClientId: ['', [Validators.required]],
    keycloakClientSecret: ['', [Validators.required]]
  });

  public constructor(
    private readonly formBuilder: FormBuilder,
    private readonly authSessionService: AuthSessionService,
    private readonly userPreferencesService: UserPreferencesService
  ) {
    void this.refreshStatus();
  }


  protected tx(tr: string, en: string): string {
    return this.userPreferencesService.language() === 'tr' ? tr : en;
  }

  protected canUpdateRuntimeConfiguration(): boolean {
    return this.authSessionService.hasRequiredRole('Operator');
  }

  protected canSaveRuntimeConfiguration(): boolean {
    return this.authSessionService.hasRequiredRole('Admin');
  }

  protected async saveAll(): Promise<void> {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    await this.executeAsync(async () => {
      const payload: RuntimeConfigurationInput = this.form.getRawValue();
      this.status.set(await saveRuntimeConfiguration(payload));
      this.feedback.set(this.tx('Runtime yapılandırması güvenli şekilde kaydedildi.', 'Runtime configuration saved securely.'));
      this.form.markAsPristine();
    });
  }

  protected async updateChanged(): Promise<void> {
    const patchPayload = this.collectDirtyValues();

    if (Object.keys(patchPayload).length === 0) {
      this.feedback.set(this.tx('Güncellenecek alan bulunamadı.', 'No fields to update.'));
      return;
    }

    await this.executeAsync(async () => {
      this.status.set(await updateRuntimeConfiguration(patchPayload));
      this.feedback.set(this.tx('Değişen alanlar güvenli şekilde güncellendi.', 'Changed fields updated securely.'));
      this.form.markAsPristine();
    });
  }

  protected async refreshStatus(): Promise<void> {
    await this.executeAsync(async () => {
      this.status.set(await getRuntimeConfigurationStatus());
      this.feedback.set(this.tx('Runtime yapılandırma durumu güncellendi.', 'Runtime configuration status refreshed.'));
    });
  }

  private collectDirtyValues(): Partial<RuntimeConfigurationInput> {
    const patchPayload: Partial<RuntimeConfigurationInput> = {};

    (Object.keys(this.form.controls) as Array<keyof RuntimeConfigurationInput>).forEach((key) => {
      const control = this.form.controls[key];
      if (!control.dirty) {
        return;
      }

      const value = control.getRawValue().trim();
      if (value.length > 0) {
        patchPayload[key] = value;
      }
    });

    return patchPayload;
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
