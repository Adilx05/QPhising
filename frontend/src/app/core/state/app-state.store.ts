import { Injectable, Signal, computed, signal } from '@angular/core';

export type AppRole = 'Admin' | 'Operator' | 'Viewer';
export type ThemeMode = 'light' | 'dark';
export const APP_ROLES: readonly AppRole[] = ['Admin', 'Operator', 'Viewer'];

export type FeatureKey =
  | 'dashboard'
  | 'campaigns'
  | 'templates'
  | 'tracking'
  | 'tasks'
  | 'analytics'
  | 'exports';

export interface SessionState {
  userId: string;
  displayName: string;
  email: string;
  role: AppRole | null;
  authenticated: boolean;
}

export interface TokenClaims {
  sub?: string;
  preferred_username?: string;
  name?: string;
  email?: string;
  exp?: number;
  nbf?: number;
  realm_access?: {
    roles?: string[];
  };
  resource_access?: Record<
    string,
    {
      roles?: string[];
    }
  >;
}

const APP_ROLE_CLAIM_MAP: Readonly<Record<string, AppRole>> = {
  admin: 'Admin',
  operator: 'Operator',
  viewer: 'Viewer',
  qphising_admin: 'Admin',
  qphising_operator: 'Operator',
  qphising_viewer: 'Viewer'
};

const ANONYMOUS_SESSION: SessionState = {
  userId: '',
  displayName: 'Anonymous',
  email: '',
  role: null,
  authenticated: false
};

export interface DashboardKpi {
  title: string;
  value: string;
}

export interface DashboardTrendPoint {
  day: string;
  clicks: number;
}

export interface DashboardCampaignRow {
  name: string;
  owner: string;
  status: 'Draft' | 'Scheduled' | 'Active' | 'Ended';
  templateType: 'CredentialHarvest' | 'Attachment' | 'LandingPage';
  startDate: string;
  endDate: string;
  clickRate: number;
}

export type DashboardTrendRow = Record<string, string>;

export interface FeatureViewState {
  loading: boolean;
  error: string | null;
  activeFilter: string;
}

@Injectable({
  providedIn: 'root'
})
export class AppStateStore {
  private readonly storageKey = 'qphising-theme-mode';

  private readonly sessionState = signal<SessionState>(ANONYMOUS_SESSION);

  private readonly themeModeState = signal<ThemeMode>(this.resolveInitialTheme());

  private readonly dashboardKpisState = signal<DashboardKpi[]>([]);

  private readonly dashboardTrendState = signal<DashboardTrendPoint[]>([]);

  private readonly dashboardCampaignsState = signal<DashboardCampaignRow[]>([]);

  private readonly featureState = signal<Record<FeatureKey, FeatureViewState>>({
    dashboard: { loading: false, error: null, activeFilter: 'Last 7 days' },
    campaigns: { loading: false, error: null, activeFilter: 'All statuses' },
    templates: { loading: false, error: null, activeFilter: 'All types' },
    tracking: { loading: false, error: null, activeFilter: 'All events' },
    tasks: { loading: false, error: null, activeFilter: 'All queues' },
    analytics: { loading: false, error: null, activeFilter: 'Weekly' },
    exports: { loading: false, error: null, activeFilter: 'All formats' }
  });

  readonly session = this.sessionState.asReadonly();
  readonly themeMode = this.themeModeState.asReadonly();
  readonly dashboardKpis = this.dashboardKpisState.asReadonly();
  readonly dashboardTrend = this.dashboardTrendState.asReadonly();
  readonly dashboardCampaigns = this.dashboardCampaignsState.asReadonly();

  readonly dashboardTrendRows = computed<DashboardTrendRow[]>(() =>
    this.dashboardTrend().map((entry) => ({ day: entry.day, clicks: entry.clicks.toString() }))
  );

  readonly isAuthenticated = computed(() => this.session().authenticated);
  readonly currentRole = computed(() => this.session().role);
  readonly featureViewState: Signal<Record<FeatureKey, FeatureViewState>> = this.featureState.asReadonly();

  readonly hasViewerAccess = computed(() => this.canAccessAnyRole(['Viewer', 'Operator', 'Admin']));

  readonly hasOperatorAccess = computed(() => this.canAccessAnyRole(['Operator', 'Admin']));

  constructor() {
    this.applyTheme(this.themeModeState());
  }

  hydrateSessionFromTokenClaims(claims: TokenClaims | null | undefined): void {
    if (!claims) {
      this.resetSession();
      return;
    }

    const userId = claims.sub?.trim() ?? '';
    const resolvedRole = this.resolveRoleFromClaims(claims);

    this.sessionState.set({
      userId,
      displayName: this.resolveDisplayName(claims),
      email: claims.email?.trim() ?? '',
      role: resolvedRole,
      authenticated: userId.length > 0
    });
  }

  resetSession(): void {
    this.sessionState.set(ANONYMOUS_SESSION);
  }

  canAccessAnyRole(roles: readonly AppRole[]): boolean {
    const currentRole = this.currentRole();

    if (!this.isAuthenticated() || !currentRole) {
      return false;
    }

    return roles.includes(currentRole);
  }

  toggleTheme(): void {
    const nextTheme: ThemeMode = this.themeModeState() === 'dark' ? 'light' : 'dark';
    this.themeModeState.set(nextTheme);

    if (typeof localStorage !== 'undefined') {
      localStorage.setItem(this.storageKey, nextTheme);
    }

    this.applyTheme(nextTheme);
  }


  setDashboardData(payload: {
    kpis: DashboardKpi[];
    trend: DashboardTrendPoint[];
    campaigns: DashboardCampaignRow[];
  }): void {
    this.dashboardKpisState.set(payload.kpis);
    this.dashboardTrendState.set(payload.trend);
    this.dashboardCampaignsState.set(payload.campaigns);
  }

  updateFeatureFilter(feature: FeatureKey, activeFilter: string): void {
    this.featureState.update((state) => ({
      ...state,
      [feature]: {
        ...state[feature],
        activeFilter
      }
    }));
  }

  setFeatureLoading(feature: FeatureKey, loading: boolean): void {
    this.featureState.update((state) => ({
      ...state,
      [feature]: {
        ...state[feature],
        loading
      }
    }));
  }

  setFeatureError(feature: FeatureKey, error: string | null): void {
    this.featureState.update((state) => ({
      ...state,
      [feature]: {
        ...state[feature],
        error
      }
    }));
  }

  private resolveRoleFromClaims(claims: TokenClaims): AppRole | null {
    const claimRoles = this.resolveClaimRoles(claims);
    if (claimRoles.length === 0) {
      return null;
    }

    for (const role of claimRoles) {
      const normalizedRole = APP_ROLE_CLAIM_MAP[role.trim().toLowerCase()];
      if (normalizedRole) {
        return normalizedRole;
      }
    }

    return null;
  }

  private resolveDisplayName(claims: TokenClaims): string {
    return claims.name?.trim() || claims.preferred_username?.trim() || claims.email?.trim() || 'Authenticated User';
  }

  private resolveClaimRoles(claims: TokenClaims): string[] {
    const aggregatedRoles = new Set<string>();
    const appendRoles = (roles: string[] | undefined): void => {
      if (!Array.isArray(roles)) {
        return;
      }

      for (const role of roles) {
        const normalized = role.trim();
        if (normalized.length > 0) {
          aggregatedRoles.add(normalized);
        }
      }
    };

    appendRoles(claims.realm_access?.roles);

    if (claims.resource_access) {
      for (const resource of Object.values(claims.resource_access)) {
        appendRoles(resource?.roles);
      }
    }

    return [...aggregatedRoles];
  }

  private resolveInitialTheme(): ThemeMode {
    if (typeof localStorage !== 'undefined') {
      const storedTheme = localStorage.getItem(this.storageKey);
      if (storedTheme === 'light' || storedTheme === 'dark') {
        return storedTheme;
      }
    }

    if (typeof window !== 'undefined' && window.matchMedia('(prefers-color-scheme: dark)').matches) {
      return 'dark';
    }

    return 'light';
  }

  private applyTheme(theme: ThemeMode): void {
    if (typeof document === 'undefined') {
      return;
    }

    document.documentElement.classList.toggle('dark', theme === 'dark');
  }
}
