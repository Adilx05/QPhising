import { Component } from '@angular/core';
import { RouterLink, RouterOutlet } from '@angular/router';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, RouterLink],
  template: `
    <div class="min-h-screen bg-slate-100">
      <header class="border-b border-slate-200 bg-white px-6 py-4">
        <div class="mx-auto flex max-w-6xl items-center justify-between">
          <a class="text-xl font-semibold text-slate-900" routerLink="/dashboard">QPhising</a>
          <nav class="flex items-center gap-3 text-sm">
            <a class="rounded px-3 py-2 text-slate-700 hover:bg-slate-100" routerLink="/setup">Setup</a>
            <a class="rounded px-3 py-2 text-slate-700 hover:bg-slate-100" routerLink="/configuration">Configuration</a>
            <a class="rounded px-3 py-2 text-slate-700 hover:bg-slate-100" routerLink="/dashboard">Dashboard</a>
          </nav>
        </div>
      </header>
      <main class="mx-auto max-w-6xl p-6">
        <router-outlet></router-outlet>
      </main>
    </div>
  `
})
export class AppComponent {}
