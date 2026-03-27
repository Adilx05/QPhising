import { Injectable, Signal, computed, signal } from '@angular/core';

export type AppRole = 'Admin' | 'Operator' | 'Viewer';
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
  private readonly sessionState = signal<SessionState>({
    userId: 'session-001',
    displayName: 'SOC Operator',
    email: 'operator@corp.local',
    role: 'Operator',
    authenticated: true
  });

  private readonly dashboardKpisState = signal<DashboardKpi[]>([
    { title: 'Campaigns', value: '24' },
    { title: 'Clicks (24h)', value: '696' },
    { title: 'Conversion Rate', value: '18.3%' }
  ]);

  private readonly dashboardTrendRowsState = signal<DashboardTrendRow[]>([
    { day: 'Mon', clicks: '120' },
    { day: 'Tue', clicks: '145' },
    { day: 'Wed', clicks: '98' },
    { day: 'Thu', clicks: '173' },
    { day: 'Fri', clicks: '160' }
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
  readonly dashboardKpis = this.dashboardKpisState.asReadonly();
  readonly dashboardTrendRows = this.dashboardTrendRowsState.asReadonly();
  readonly isAuthenticated = computed(() => this.session().authenticated);
  readonly currentRole = computed(() => this.session().role);
  readonly featureViewState: Signal<Record<FeatureKey, FeatureViewState>> = this.featureState.asReadonly();

  readonly hasViewerAccess = computed(() => {
    const role = this.currentRole();
    return role === 'Viewer' || role === 'Operator' || role === 'Admin';
  });

  readonly hasOperatorAccess = computed(() => {
    const role = this.currentRole();
    return role === 'Operator' || role === 'Admin';
  });

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
}
