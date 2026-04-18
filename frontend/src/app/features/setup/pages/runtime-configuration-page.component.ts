import { CommonModule } from '@angular/common';
import { Component, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { InputTextModule } from 'primeng/inputtext';
import { PasswordModule } from 'primeng/password';
import type { RuntimeConfigurationResult } from '../../../shared/proxy';
import { resolveApiError } from '../../../core/http/api-error-handler';
import {
  getRuntimeConfigurationStatus,
  saveRuntimeConfiguration,
  updateRuntimeConfiguration,
  type RuntimeConfigurationInput
} from '../data-access';

@Component({
  selector: 'app-runtime-configuration-page',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, ButtonModule, CardModule, InputTextModule, PasswordModule],
  templateUrl: './runtime-configuration-page.component.html'
})
export class RuntimeConfigurationPageComponent {
  protected readonly isBusy = signal(false);
  protected readonly status = signal<RuntimeConfigurationResult | null>(null);
  protected readonly feedback = signal<string | null>(null);

  protected readonly form = this.formBuilder.nonNullable.group({
    databaseConnectionString: ['', [Validators.required]],
    redisConnectionString: ['', [Validators.required]],
    keycloakAuthority: ['', [Validators.required]],
    keycloakRealm: ['', [Validators.required]],
    keycloakClientId: ['', [Validators.required]],
    keycloakClientSecret: ['', [Validators.required]]
  });

  public constructor(private readonly formBuilder: FormBuilder) {
    void this.refreshStatus();
  }

  protected async saveAll(): Promise<void> {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    await this.executeAsync(async () => {
      const payload: RuntimeConfigurationInput = this.form.getRawValue();
      this.status.set(await saveRuntimeConfiguration(payload));
      this.feedback.set('Runtime konfigürasyonu güvenli şekilde kaydedildi.');
      this.form.markAsPristine();
    });
  }

  protected async updateChanged(): Promise<void> {
    const patchPayload = this.collectDirtyValues();

    if (Object.keys(patchPayload).length === 0) {
      this.feedback.set('Güncellenecek alan bulunamadı.');
      return;
    }

    await this.executeAsync(async () => {
      this.status.set(await updateRuntimeConfiguration(patchPayload));
      this.feedback.set('Değişen alanlar güvenli şekilde güncellendi.');
      this.form.markAsPristine();
    });
  }

  protected async refreshStatus(): Promise<void> {
    await this.executeAsync(async () => {
      this.status.set(await getRuntimeConfigurationStatus());
      this.feedback.set('Runtime konfigürasyon durumu güncellendi.');
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
