import { DOCUMENT } from '@angular/common';
import { Injectable, inject, signal } from '@angular/core';

export type AppLanguage = 'tr' | 'en';

@Injectable({ providedIn: 'root' })
export class UserPreferencesService {
  private static readonly languageStorageKey = 'qphising.language';

  private readonly document = inject(DOCUMENT);
  protected readonly languageSignal = signal<AppLanguage>(this.resolveInitialLanguage());

  public readonly language = this.languageSignal.asReadonly();

  public setLanguage(language: AppLanguage): void {
    this.languageSignal.set(language);
    this.persist(UserPreferencesService.languageStorageKey, language);
  }


  private resolveInitialLanguage(): AppLanguage {
    const stored = this.read(UserPreferencesService.languageStorageKey);
    if (stored === 'tr' || stored === 'en') {
      return stored;
    }

    const browserLanguage = this.document?.defaultView?.navigator?.language?.toLowerCase() ?? '';
    return browserLanguage.startsWith('tr') ? 'tr' : 'en';
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
