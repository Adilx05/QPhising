import { Component } from '@angular/core';

@Component({
  selector: 'app-unauthorized-page',
  standalone: false,
  template: `
    <section class="mx-auto mt-20 max-w-2xl rounded-lg border border-amber-200 bg-amber-50 p-8 text-amber-900 shadow-sm">
      <h2 class="text-2xl font-semibold">Access restricted</h2>
      <p class="mt-3 text-sm">
        Your current session does not have permission to view this page. Contact an administrator if you believe this is an error.
      </p>
      <a routerLink="/dashboard" class="mt-6 inline-block rounded bg-amber-600 px-4 py-2 text-sm font-semibold text-white">
        Return to dashboard
      </a>
    </section>
  `
})
export class UnauthorizedPageComponent {}
