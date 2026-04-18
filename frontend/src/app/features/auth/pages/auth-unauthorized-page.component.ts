import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { OidcAuthService } from '../../../core/auth/oidc-auth.service';

@Component({
  selector: 'app-auth-unauthorized-page',
  standalone: true,
  imports: [CommonModule],
  template: `
    <section class="mx-auto max-w-xl rounded-2xl border border-rose-200 bg-white p-8 shadow-sm">
      <h1 class="text-xl font-semibold text-rose-700">Access denied</h1>
      <p class="mt-3 text-sm text-slate-600">
        You are authenticated but do not have the required role for this area.
      </p>
      <p *ngIf="reason" class="mt-2 text-xs text-slate-500">Reason: {{ reason }}</p>

      <div class="mt-6 flex gap-3">
        <button
          type="button"
          class="rounded-lg bg-slate-900 px-4 py-2 text-sm font-medium text-white hover:bg-slate-700"
          (click)="signInAgain()">
          Sign in with another account
        </button>

        <button
          type="button"
          class="rounded-lg border border-slate-300 px-4 py-2 text-sm font-medium text-slate-700 hover:bg-slate-100"
          (click)="goToDashboard()">
          Back to dashboard
        </button>
      </div>
    </section>
  `
})
export class AuthUnauthorizedPageComponent {
  protected readonly reason: string | null;

  public constructor(
    private readonly route: ActivatedRoute,
    private readonly router: Router,
    private readonly oidcAuthService: OidcAuthService
  ) {
    this.reason = this.route.snapshot.queryParamMap.get('reason');
  }

  protected async signInAgain(): Promise<void> {
    const returnUrl = this.route.snapshot.queryParamMap.get('returnUrl') ?? '/dashboard';
    await this.oidcAuthService.login(returnUrl);
  }

  protected async goToDashboard(): Promise<void> {
    await this.router.navigateByUrl('/dashboard');
  }
}
