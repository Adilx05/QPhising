import { Routes } from '@angular/router';
import {
  authenticationCanActivateGuard,
  authenticationCanMatchGuard
} from './core/guards';
import { AuthCallbackPageComponent } from './features/auth/pages/auth-callback-page.component';
import { AuditLogsPageComponent } from './features/audit/pages/audit-logs-page.component';
import { AuthUnauthorizedPageComponent } from './features/auth/pages/auth-unauthorized-page.component';
import { CampaignsPageComponent } from './features/campaigns/pages/campaigns-page.component';
import { CampaignDetailPageComponent } from './features/campaigns/pages/campaign-detail-page.component';
import { DashboardPageComponent } from './features/dashboard/pages/dashboard-page.component';
import { ReportsPageComponent } from './features/reports/pages/reports-page.component';
import { TrackingDashboardPageComponent } from './features/tracking/pages/tracking-dashboard-page.component';
import { PublicTrackingLandingPageComponent } from './features/tracking/pages/public-tracking-landing-page.component';
import { TemplatesPageComponent } from './features/templates/pages/templates-page.component';

export const appRoutes: Routes = [
  {
    path: 'p/:slug',
    component: PublicTrackingLandingPageComponent
  },
  {
    path: 'auth/callback',
    component: AuthCallbackPageComponent
  },
  {
    path: 'auth/unauthorized',
    component: AuthUnauthorizedPageComponent
  },
  {
    path: 'dashboard',
    component: DashboardPageComponent,
    canActivate: [authenticationCanActivateGuard],
    canMatch: [authenticationCanMatchGuard],
    data: {
      requiredRole: 'Viewer'
    }
  },

  {
    path: 'campaigns',
    component: CampaignsPageComponent,
    canActivate: [authenticationCanActivateGuard],
    canMatch: [authenticationCanMatchGuard],
    data: {
      requiredRole: 'Viewer'
    }
  },
  {
    path: 'campaigns/:campaignId',
    component: CampaignDetailPageComponent,
    canActivate: [authenticationCanActivateGuard],
    canMatch: [authenticationCanMatchGuard],
    data: {
      requiredRole: 'Viewer'
    }
  },

  {
    path: 'templates',
    component: TemplatesPageComponent,
    canActivate: [authenticationCanActivateGuard],
    canMatch: [authenticationCanMatchGuard],
    data: {
      requiredRole: 'Viewer'
    }
  },



  {
    path: 'audit-logs',
    component: AuditLogsPageComponent,
    canActivate: [authenticationCanActivateGuard],
    canMatch: [authenticationCanMatchGuard],
    data: {
      requiredRole: 'Operator'
    }
  },
  {
    path: 'reports',
    component: ReportsPageComponent,
    canActivate: [authenticationCanActivateGuard],
    canMatch: [authenticationCanMatchGuard],
    data: {
      requiredRole: 'Viewer'
    }
  },
  {
    path: 'tracking',
    component: TrackingDashboardPageComponent,
    canActivate: [authenticationCanActivateGuard],
    canMatch: [authenticationCanMatchGuard],
    data: {
      requiredRole: 'Viewer'
    }
  },
  {
    path: '',
    pathMatch: 'full',
    redirectTo: 'dashboard'
  },
  {
    path: '**',
    component: PublicTrackingLandingPageComponent
  }
];
