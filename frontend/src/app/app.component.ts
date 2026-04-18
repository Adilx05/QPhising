import { Component } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, RouterLink, RouterLinkActive],
  template: `
    <div class="app-shell min-h-screen">
      <aside class="hidden border-r border-slate-200/80 bg-white/95 lg:flex lg:w-72 lg:flex-col">
        <div class="border-b border-slate-200 px-6 py-6">
          <a class="text-2xl font-semibold tracking-tight text-slate-900" routerLink="/dashboard">QPhising</a>
          <p class="mt-2 text-sm text-slate-500">Enterprise Security Operations Console</p>
        </div>

        <nav class="flex-1 space-y-2 px-4 py-6">
          <a
            routerLink="/dashboard"
            routerLinkActive="is-active"
            class="nav-item"
          >
            <i class="pi pi-home text-sm"></i>
            <span>Dashboard</span>
          </a>

          <a
            routerLink="/setup"
            routerLinkActive="is-active"
            class="nav-item"
          >
            <i class="pi pi-cog text-sm"></i>
            <span>Setup Wizard</span>
          </a>

          <a
            routerLink="/configuration"
            routerLinkActive="is-active"
            class="nav-item"
          >
            <i class="pi pi-sliders-h text-sm"></i>
            <span>Runtime Configuration</span>
          </a>
        </nav>
      </aside>

      <div class="flex min-h-screen flex-1 flex-col">
        <header class="sticky top-0 z-20 border-b border-slate-200/80 bg-white/85 px-5 py-4 backdrop-blur lg:px-8">
          <div class="mx-auto flex max-w-7xl items-center justify-between">
            <a class="text-lg font-semibold text-slate-900 lg:hidden" routerLink="/dashboard">QPhising</a>

            <nav class="flex items-center gap-2 text-sm lg:hidden">
              <a class="mobile-nav-item" routerLink="/dashboard" routerLinkActive="is-active">Dashboard</a>
              <a class="mobile-nav-item" routerLink="/setup" routerLinkActive="is-active">Setup</a>
              <a class="mobile-nav-item" routerLink="/configuration" routerLinkActive="is-active">Config</a>
            </nav>

            <div class="hidden items-center gap-3 rounded-full border border-emerald-200 bg-emerald-50 px-3 py-1.5 text-xs font-medium text-emerald-700 lg:flex">
              <span class="inline-block h-2 w-2 rounded-full bg-emerald-500"></span>
              Platform Online
            </div>
          </div>
        </header>

        <main class="mx-auto w-full max-w-7xl flex-1 px-4 py-6 lg:px-8 lg:py-8">
          <router-outlet></router-outlet>
        </main>
      </div>
    </div>
  `
})
export class AppComponent {}
