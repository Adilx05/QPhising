import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { OidcAuthService } from '../../../core/auth/oidc-auth.service';
import { UserPreferencesService } from '../../../core/ui/user-preferences.service';

@Component({
  selector: 'app-auth-callback-page',
  standalone: true,
  template: `
    <section class="mx-auto max-w-xl rounded-2xl border border-slate-200 bg-white p-8 shadow-sm">
      <h1 class="text-xl font-semibold text-slate-900">{{ tx('Giriş yapılıyor…', 'Signing you in…') }}</h1>
      <p class="mt-3 text-sm text-slate-600">{{ tx('Kimlik sağlayıcı ile güvenli kimlik doğrulama tamamlanıyor.', 'Completing secure authentication with the identity provider.') }}</p>
    </section>
  `
})
export class AuthCallbackPageComponent implements OnInit {
  public constructor(
    private readonly oidcAuthService: OidcAuthService,
    private readonly route: ActivatedRoute,
    private readonly router: Router,
    private readonly userPreferencesService: UserPreferencesService
  ) {}


  protected tx(tr: string, en: string): string {
    return this.userPreferencesService.language() === 'tr' ? tr : en;
  }
  public async ngOnInit(): Promise<void> {
    const callbackUrl = new URL(globalThis.location.href);

    try {
      const returnUrl = await this.oidcAuthService.handleCallback(callbackUrl);
      await this.router.navigateByUrl(returnUrl);
    } catch {
      await this.router.navigate(['/auth/unauthorized'], {
        queryParams: {
          reason: 'callback-failed',
          returnUrl: this.route.snapshot.queryParamMap.get('returnUrl') ?? '/dashboard'
        }
      });
    }
  }
}
