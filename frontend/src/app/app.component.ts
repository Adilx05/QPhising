import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { NavigationEnd, Router } from '@angular/router';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { filter } from 'rxjs/operators';
import { AuthSessionService } from './core/auth/auth-session';
import { OidcAuthService } from './core/auth/oidc-auth.service';
import { AppLanguage, UserPreferencesService } from './core/ui/user-preferences.service';
import { SetupReadinessState, SetupService } from './shared/proxy';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterOutlet, RouterLink, RouterLinkActive],
  template: `
    <ng-container *ngIf="!isPublicLandingRoute(); else publicLandingOnly">
    <div class="app-shell min-h-screen">
      <aside class="hidden border-r border-slate-200/80 bg-white/95 lg:flex lg:w-72 lg:flex-col">
        <div class="border-b border-slate-200 px-6 py-6">
          <a class="text-2xl font-semibold tracking-tight text-slate-900" routerLink="/dashboard">QPhising</a>
          <p class="mt-2 text-sm text-slate-500">{{ t('shellSubtitle') }}</p>
        </div>

        <section *ngIf="isAuthenticated()" class="mx-4 mt-4 rounded-xl border border-slate-200 bg-slate-50/70 p-4">
          <p class="text-sm font-semibold text-slate-900">{{ getUserFullName() }}</p>
          <p class="mt-1 text-xs text-slate-600">{{ getUserRole() }}</p>
          <button
            type="button"
            class="mt-3 inline-flex w-full items-center justify-center gap-2 rounded-lg border border-slate-300 bg-white px-3 py-2 text-sm font-medium text-slate-700 hover:bg-slate-100"
            (click)="logout()"
          >
            <i class="pi pi-sign-out"></i>
            {{ t('logout') }}
          </button>
        </section>

        <nav class="space-y-2 px-4 py-6">
          <a routerLink="/dashboard" routerLinkActive="is-active" class="nav-item">
            <i class="pi pi-home text-sm"></i>
            <span>{{ t('dashboard') }}</span>
          </a>

          <a *ngIf="canViewTracking()" routerLink="/tracking" routerLinkActive="is-active" class="nav-item">
            <i class="pi pi-chart-line text-sm"></i>
            <span>{{ t('tracking') }}</span>
          </a>

          <a *ngIf="canViewCampaigns()" routerLink="/campaigns" routerLinkActive="is-active" class="nav-item">
            <i class="pi pi-megaphone text-sm"></i>
            <span>{{ t('campaigns') }}</span>
          </a>

          <a *ngIf="canViewTemplates()" routerLink="/templates" routerLinkActive="is-active" class="nav-item">
            <i class="pi pi-file-edit text-sm"></i>
            <span>{{ t('templates') }}</span>
          </a>
        </nav>

        <section class="mt-auto space-y-2 border-t border-slate-200 px-4 py-4">
          <a *ngIf="shouldShowSetupWizard()" routerLink="/setup" routerLinkActive="is-active" class="nav-item nav-item-muted">
            <i class="pi pi-cog text-sm"></i>
            <span>{{ t('setupWizard') }}</span>
          </a>

          <a *ngIf="canViewConfiguration()" routerLink="/configuration" routerLinkActive="is-active" class="nav-item nav-item-muted">
            <i class="pi pi-sliders-h text-sm"></i>
            <span>{{ t('runtimeConfig') }}</span>
          </a>
        </section>
      </aside>

      <div class="flex min-h-screen flex-1 flex-col">
        <header class="sticky top-0 z-20 border-b border-slate-200/80 bg-white/85 px-5 py-4 backdrop-blur lg:px-8">
          <div class="mx-auto flex max-w-7xl items-center justify-between">
            <a class="text-lg font-semibold text-slate-900 lg:hidden" routerLink="/dashboard">QPhising</a>

            <nav class="flex items-center gap-2 text-sm lg:hidden">
              <a class="mobile-nav-item" routerLink="/dashboard" routerLinkActive="is-active">{{ t('dashboard') }}</a>
              <a *ngIf="canViewTracking()" class="mobile-nav-item" routerLink="/tracking" routerLinkActive="is-active">{{ t('tracking') }}</a>
              <a *ngIf="canViewCampaigns()" class="mobile-nav-item" routerLink="/campaigns" routerLinkActive="is-active">{{ t('campaigns') }}</a>
              <a *ngIf="canViewTemplates()" class="mobile-nav-item" routerLink="/templates" routerLinkActive="is-active">{{ t('templates') }}</a>
              <a *ngIf="shouldShowSetupWizard()" class="mobile-nav-item" routerLink="/setup" routerLinkActive="is-active">{{ t('setup') }}</a>
              <a *ngIf="canViewConfiguration()" class="mobile-nav-item" routerLink="/configuration" routerLinkActive="is-active">{{ t('configShort') }}</a>
            </nav>

            <div class="flex items-center gap-2">
              <select
                class="rounded-lg border border-slate-300 bg-white px-2 py-1.5 text-xs font-medium text-slate-700"
                [ngModel]="activeLanguage()"
                (ngModelChange)="setLanguage($event)"
                [attr.aria-label]="t('language')"
              >
                <option value="tr">Türkçe</option>
                <option value="en">English</option>
              </select>

              <button
                type="button"
                class="inline-flex items-center gap-2 rounded-lg border border-slate-300 bg-white px-2.5 py-1.5 text-xs font-medium text-slate-700 hover:bg-slate-100"
                (click)="toggleTheme()"
              >
                <i class="pi" [ngClass]="isDarkTheme() ? 'pi-sun' : 'pi-moon'"></i>
                {{ isDarkTheme() ? t('lightMode') : t('darkMode') }}
              </button>

              <div class="hidden items-center gap-3 rounded-full border px-3 py-1.5 text-xs font-medium lg:flex"
                  [ngClass]="isAuthenticated() ? 'border-emerald-200 bg-emerald-50 text-emerald-700' : 'border-amber-200 bg-amber-50 text-amber-700'">
                <span class="inline-block h-2 w-2 rounded-full" [ngClass]="isAuthenticated() ? 'bg-emerald-500' : 'bg-amber-500'"></span>
                {{ isAuthenticated() ? t('sessionAuthenticated') : t('sessionUnauthenticated') }}
              </div>
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
  private readonly translations: Record<AppLanguage, Record<string, string>> = {
    tr: {
      shellSubtitle: 'Kurumsal Güvenlik Operasyon Konsolu',
      logout: 'Çıkış Yap',
      dashboard: 'Gösterge Paneli',
      tracking: 'Takip',
      campaigns: 'Senaryolar',
      templates: 'Şablonlar',
      setupWizard: 'Kurulum Sihirbazı',
      runtimeConfig: 'Çalışma Zamanı Ayarları',
      setup: 'Kurulum',
      configShort: 'Ayarlar',
      language: 'Dil',
      darkMode: 'Koyu Mod',
      lightMode: 'Açık Mod',
      sessionAuthenticated: 'Oturum Açık',
      sessionUnauthenticated: 'Oturum Kapalı'
    },
    en: {
      shellSubtitle: 'Enterprise Security Operations Console',
      logout: 'Logout',
      dashboard: 'Dashboard',
      tracking: 'Tracking',
      campaigns: 'Campaigns',
      templates: 'Templates',
      setupWizard: 'Setup Wizard',
      runtimeConfig: 'Runtime Configuration',
      setup: 'Setup',
      configShort: 'Config',
      language: 'Language',
      darkMode: 'Dark Mode',
      lightMode: 'Light Mode',
      sessionAuthenticated: 'Authenticated Session',
      sessionUnauthenticated: 'Unauthenticated Session'
    }
  };

  private currentUrl = '';
  private setupCompleted = false;

  public constructor(
    private readonly authSessionService: AuthSessionService,
    private readonly oidcAuthService: OidcAuthService,
    private readonly userPreferencesService: UserPreferencesService,
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

  protected activeLanguage(): AppLanguage {
    return this.userPreferencesService.language();
  }

  protected isDarkTheme(): boolean {
    return this.userPreferencesService.theme() === 'dark';
  }

  protected setLanguage(language: AppLanguage): void {
    this.userPreferencesService.setLanguage(language);
  }

  protected toggleTheme(): void {
    this.userPreferencesService.toggleTheme();
  }

  protected t(key: string): string {
    const language = this.userPreferencesService.language();
    return this.translations[language][key] ?? key;
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
