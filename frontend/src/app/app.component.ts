import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { NavigationEnd, Router } from '@angular/router';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { filter } from 'rxjs/operators';
import { SetupReadinessState, SetupService } from './shared/proxy';
import { AuthSessionService } from './core/auth/auth-session';
import { OidcAuthService } from './core/auth/oidc-auth.service';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, RouterOutlet, RouterLink, RouterLinkActive],
  template: `
    <ng-container *ngIf="!isPublicLandingRoute(); else publicLandingOnly">
    <div class="app-shell min-h-screen">
      <aside class="hidden border-r border-slate-200/80 bg-white/95 lg:flex lg:w-72 lg:flex-col">
        <div class="border-b border-slate-200 px-6 py-6">
          <a class="text-2xl font-semibold tracking-tight text-slate-900" routerLink="/dashboard">QPhising</a>
          <p class="mt-2 text-sm text-slate-500">Enterprise Security Operations Console</p>
        </div>

        <section class="mx-4 mt-4 rounded-xl border border-slate-200 bg-slate-50/70 p-4">
          <p class="text-sm font-semibold text-slate-900">{{ getUserFullName() }}</p>
          <p class="mt-1 text-xs text-slate-600">{{ getUserRole() }}</p>
          <button
            type="button"
            class="mt-3 inline-flex w-full items-center justify-center gap-2 rounded-lg border border-slate-300 bg-white px-3 py-2 text-sm font-medium text-slate-700 hover:bg-slate-100"
            [disabled]="!isAuthenticated()"
            (click)="logout()"
          >
            <i class="pi pi-sign-out"></i>
            Logout
          </button>
        </section>

        <nav class="space-y-2 px-4 py-6">
          <a routerLink="/dashboard" routerLinkActive="is-active" class="nav-item">
            <i class="pi pi-home text-sm"></i>
            <span>Dashboard</span>
          </a>

          <a *ngIf="canViewTracking()" routerLink="/tracking" routerLinkActive="is-active" class="nav-item">
            <i class="pi pi-chart-line text-sm"></i>
            <span>Tracking</span>
          </a>

          <a *ngIf="canViewCampaigns()" routerLink="/campaigns" routerLinkActive="is-active" class="nav-item">
            <i class="pi pi-megaphone text-sm"></i>
            <span>Campaigns</span>
          </a>

          <a *ngIf="canViewTemplates()" routerLink="/templates" routerLinkActive="is-active" class="nav-item">
            <i class="pi pi-file-edit text-sm"></i>
            <span>Templates</span>
          </a>
        </nav>

        <section class="mt-auto space-y-2 border-t border-slate-200 px-4 py-4">
          <a *ngIf="shouldShowSetupWizard()" routerLink="/setup" routerLinkActive="is-active" class="nav-item nav-item-muted">
            <i class="pi pi-cog text-sm"></i>
            <span>Setup Wizard</span>
          </a>

          <a *ngIf="canViewConfiguration()" routerLink="/configuration" routerLinkActive="is-active" class="nav-item nav-item-muted">
            <i class="pi pi-sliders-h text-sm"></i>
            <span>Runtime Configuration</span>
          </a>
        </section>
      </aside>

      <div class="flex min-h-screen flex-1 flex-col">
        <header class="sticky top-0 z-20 border-b border-slate-200/80 bg-white/85 px-5 py-4 backdrop-blur lg:px-8">
          <div class="mx-auto flex max-w-7xl items-center justify-between">
            <a class="text-lg font-semibold text-slate-900 lg:hidden" routerLink="/dashboard">QPhising</a>

            <nav class="flex items-center gap-2 text-sm lg:hidden">
              <a class="mobile-nav-item" routerLink="/dashboard" routerLinkActive="is-active">Dashboard</a>
              <a *ngIf="canViewTracking()" class="mobile-nav-item" routerLink="/tracking" routerLinkActive="is-active">Tracking</a>
              <a *ngIf="canViewCampaigns()" class="mobile-nav-item" routerLink="/campaigns" routerLinkActive="is-active">Campaigns</a>
              <a *ngIf="canViewTemplates()" class="mobile-nav-item" routerLink="/templates" routerLinkActive="is-active">Templates</a>
              <a *ngIf="shouldShowSetupWizard()" class="mobile-nav-item" routerLink="/setup" routerLinkActive="is-active">Setup</a>
              <a *ngIf="canViewConfiguration()" class="mobile-nav-item" routerLink="/configuration" routerLinkActive="is-active">Config</a>
            </nav>

            <div class="hidden items-center gap-3 rounded-full border px-3 py-1.5 text-xs font-medium lg:flex"
                 [ngClass]="isAuthenticated() ? 'border-emerald-200 bg-emerald-50 text-emerald-700' : 'border-amber-200 bg-amber-50 text-amber-700'">
              <span class="inline-block h-2 w-2 rounded-full" [ngClass]="isAuthenticated() ? 'bg-emerald-500' : 'bg-amber-500'"></span>
              {{ isAuthenticated() ? 'Authenticated Session' : 'Unauthenticated Session' }}
            </div>
          </div>
        </header>

        <main class="mx-auto w-full max-w-7xl flex-1 px-4 py-6 lg:px-8 lg:py-8">
          <router-outlet></router-outlet>
        </main>
      </div>
    </div>
    </ng-container>

    <ng-template #publicLandingOnly>
      <router-outlet></router-outlet>
    </ng-template>
  `
})
export class AppComponent {
  private currentUrl = '';
  private setupCompleted = false;

  public constructor(
    private readonly authSessionService: AuthSessionService,
    private readonly oidcAuthService: OidcAuthService,
    private readonly router: Router
  ) {
    this.currentUrl = this.resolveInitialUrl();
    void this.refreshSetupVisibility();
    this.router.events
      .pipe(filter((event): event is NavigationEnd => event instanceof NavigationEnd))
      .subscribe((event) => {
        const previousUrl = this.currentUrl;
        this.currentUrl = event.urlAfterRedirects;

        if (previousUrl.startsWith('/setup') || event.urlAfterRedirects.startsWith('/setup')) {
          void this.refreshSetupVisibility();
        }
      });
  }

  protected isAuthenticated(): boolean {
    return this.authSessionService.getAuthSession().isAuthenticated;
  }

  protected getUserFullName(): string {
    return this.authSessionService.getUserProfile().fullName;
  }

  protected getUserRole(): string {
    return this.authSessionService.getUserProfile().primaryRole ?? 'None';
  }

  protected async logout(): Promise<void> {
    await this.oidcAuthService.logout();
  }

  protected canViewConfiguration(): boolean {
    return this.authSessionService.hasRequiredRole('Viewer');
  }

  protected canViewTracking(): boolean {
    return this.authSessionService.hasRequiredRole('Viewer');
  }

  protected canViewCampaigns(): boolean {
    return this.authSessionService.hasRequiredRole('Viewer');
  }

  protected canViewTemplates(): boolean {
    return this.authSessionService.hasRequiredRole('Viewer');
  }

  protected shouldShowSetupWizard(): boolean {
    return !this.setupCompleted;
  }

  protected isPublicLandingRoute(): boolean {
    return this.currentUrl.startsWith('/p/');
  }

  private resolveInitialUrl(): string {
    if (typeof window !== 'undefined' && window.location?.pathname) {
      return `${window.location.pathname}${window.location.search}`;
    }

    return this.router.url;
  }

  private async refreshSetupVisibility(): Promise<void> {
    try {
      const status = await SetupService.getStatusSetup();
      this.setupCompleted = status.readinessState === SetupReadinessState._2;
    } catch {
      this.setupCompleted = false;
    }
  }
}
