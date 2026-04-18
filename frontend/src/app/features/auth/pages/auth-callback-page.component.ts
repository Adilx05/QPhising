import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { OidcAuthService } from '../../../core/auth/oidc-auth.service';

@Component({
  selector: 'app-auth-callback-page',
  standalone: true,
  template: `
    <section class="mx-auto max-w-xl rounded-2xl border border-slate-200 bg-white p-8 shadow-sm">
      <h1 class="text-xl font-semibold text-slate-900">Signing you in…</h1>
      <p class="mt-3 text-sm text-slate-600">Completing secure authentication with the identity provider.</p>
    </section>
  `
})
export class AuthCallbackPageComponent implements OnInit {
  public constructor(
    private readonly oidcAuthService: OidcAuthService,
    private readonly route: ActivatedRoute,
    private readonly router: Router
  ) {}

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
