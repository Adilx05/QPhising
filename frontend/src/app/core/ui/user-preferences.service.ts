import { DOCUMENT } from '@angular/common';
import { Injectable, Renderer2, RendererFactory2, inject, signal } from '@angular/core';

export type AppLanguage = 'tr' | 'en';
export type AppTheme = 'light' | 'dark';

@Injectable({ providedIn: 'root' })
export class UserPreferencesService {
  private static readonly languageStorageKey = 'qphising.language';
  private static readonly themeStorageKey = 'qphising.theme';

  private readonly document = inject(DOCUMENT);
  private readonly renderer: Renderer2;
  protected readonly languageSignal = signal<AppLanguage>(this.resolveInitialLanguage());
  protected readonly themeSignal = signal<AppTheme>(this.resolveInitialTheme());

  public readonly language = this.languageSignal.asReadonly();
  public readonly theme = this.themeSignal.asReadonly();

  public constructor(rendererFactory: RendererFactory2) {
    this.renderer = rendererFactory.createRenderer(null, null);
    this.applyTheme(this.themeSignal());
  }

  public setLanguage(language: AppLanguage): void {
    this.languageSignal.set(language);
    this.persist(UserPreferencesService.languageStorageKey, language);
  }

  public setTheme(theme: AppTheme): void {
    this.themeSignal.set(theme);
    this.applyTheme(theme);
    this.persist(UserPreferencesService.themeStorageKey, theme);
  }

  public toggleTheme(): void {
    this.setTheme(this.themeSignal() === 'dark' ? 'light' : 'dark');
  }

  private applyTheme(theme: AppTheme): void {
    const body = this.document?.body;
    if (!body) {
      return;
    }

    if (theme === 'dark') {
      this.renderer.addClass(body, 'app-dark');
      this.renderer.setAttribute(body, 'data-theme', 'dark');
      return;
    }

    this.renderer.removeClass(body, 'app-dark');
    this.renderer.setAttribute(body, 'data-theme', 'light');
  }

  private resolveInitialLanguage(): AppLanguage {
    const stored = this.read(UserPreferencesService.languageStorageKey);
    if (stored === 'tr' || stored === 'en') {
      return stored;
    }

    const browserLanguage = this.document?.defaultView?.navigator?.language?.toLowerCase() ?? '';
    return browserLanguage.startsWith('tr') ? 'tr' : 'en';
  }

  private resolveInitialTheme(): AppTheme {
    const stored = this.read(UserPreferencesService.themeStorageKey);
    if (stored === 'dark' || stored === 'light') {
      return stored;
    }

    const prefersDark = this.document?.defaultView?.matchMedia?.('(prefers-color-scheme: dark)').matches;
    return prefersDark ? 'dark' : 'light';
  }

  private persist(key: string, value: string): void {
    try {
      this.document?.defaultView?.localStorage?.setItem(key, value);
    } catch {
      // no-op: localStorage may be unavailable in strict environments
    }
  }

  private read(key: string): string | null {
    try {
      return this.document?.defaultView?.localStorage?.getItem(key) ?? null;
    } catch {
      return null;
    }
  }
}
