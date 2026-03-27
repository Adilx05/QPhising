import { Injectable, Signal, computed, signal } from '@angular/core';

export type AppRole = 'Admin' | 'Operator' | 'Viewer';
export type ThemeMode = 'light' | 'dark';
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
  role: AppRole;
  authenticated: boolean;
}

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

  private readonly sessionState = signal<SessionState>({
    userId: 'session-001',
    displayName: 'SOC Operator',
    email: 'operator@corp.local',
    role: 'Operator',
    authenticated: true
  });

  private readonly themeModeState = signal<ThemeMode>(this.resolveInitialTheme());

  private readonly dashboardKpisState = signal<DashboardKpi[]>([
    { title: 'Total Campaigns', value: '24' },
    { title: 'Clicks (24h)', value: '696' },
    { title: 'Conversion Rate', value: '18.3%' },
    { title: 'Tasks Queued', value: '11' }
  ]);

  private readonly dashboardTrendState = signal<DashboardTrendPoint[]>([
    { day: 'Mon', clicks: 120 },
    { day: 'Tue', clicks: 145 },
    { day: 'Wed', clicks: 98 },
    { day: 'Thu', clicks: 173 },
    { day: 'Fri', clicks: 160 },
    { day: 'Sat', clicks: 134 },
    { day: 'Sun', clicks: 155 }
  ]);

  private readonly dashboardCampaignsState = signal<DashboardCampaignRow[]>([
    {
      name: 'Q2 Finance Wire Transfer',
      owner: 'Alice Warren',
      status: 'Active',
      templateType: 'CredentialHarvest',
      startDate: '2026-03-18',
      endDate: '2026-04-02',
      clickRate: 22.4
    },
    {
      name: 'Vendor Invoice Follow-up',
      owner: 'Omar Delgado',
      status: 'Scheduled',
      templateType: 'Attachment',
      startDate: '2026-03-29',
      endDate: '2026-04-07',
      clickRate: 0
    },
    {
      name: 'Benefits Enrollment Alert',
      owner: 'Rina Cho',
      status: 'Draft',
      templateType: 'LandingPage',
      startDate: '2026-04-04',
      endDate: '2026-04-18',
      clickRate: 0
    },
    {
      name: 'SSO Password Rotation',
      owner: 'Mason Reid',
      status: 'Ended',
      templateType: 'CredentialHarvest',
      startDate: '2026-02-10',
      endDate: '2026-02-25',
      clickRate: 15.1
    },
    {
      name: 'Executive Travel Security',
      owner: 'Nia Patel',
      status: 'Active',
      templateType: 'LandingPage',
      startDate: '2026-03-20',
      endDate: '2026-04-05',
      clickRate: 19.8
    }
  ]);

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

  canAccessAnyRole(roles: readonly AppRole[]): boolean {
    if (!this.isAuthenticated()) {
      return false;
    }

    return roles.includes(this.currentRole());
  }

  toggleTheme(): void {
    const nextTheme: ThemeMode = this.themeModeState() === 'dark' ? 'light' : 'dark';
    this.themeModeState.set(nextTheme);

    if (typeof localStorage !== 'undefined') {
      localStorage.setItem(this.storageKey, nextTheme);
    }

    this.applyTheme(nextTheme);
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
