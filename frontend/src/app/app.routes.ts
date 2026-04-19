import { Routes } from '@angular/router';
import {
  authenticationCanActivateGuard,
  authenticationCanMatchGuard,
  setupCompletionCanActivateGuard,
  setupCompletionCanMatchGuard
} from './core/guards';
import { AuthCallbackPageComponent } from './features/auth/pages/auth-callback-page.component';
import { AuthUnauthorizedPageComponent } from './features/auth/pages/auth-unauthorized-page.component';
import { CampaignsPageComponent } from './features/campaigns/pages/campaigns-page.component';
import { CampaignDetailPageComponent } from './features/campaigns/pages/campaign-detail-page.component';
import { DashboardPageComponent } from './features/dashboard/pages/dashboard-page.component';
import { RuntimeConfigurationPageComponent } from './features/setup/pages/runtime-configuration-page.component';
import { TrackingDashboardPageComponent } from './features/tracking/pages/tracking-dashboard-page.component';
import { PublicTrackingLandingPageComponent } from './features/tracking/pages/public-tracking-landing-page.component';
import { SetupWizardPageComponent } from './features/setup/pages/setup-wizard-page.component';
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
    path: 'setup',
    component: SetupWizardPageComponent
  },
  {
    path: 'dashboard',
    component: DashboardPageComponent,
    canActivate: [authenticationCanActivateGuard, setupCompletionCanActivateGuard],
    canMatch: [authenticationCanMatchGuard, setupCompletionCanMatchGuard],
    data: {
      requiredRole: 'Viewer'
    }
  },

  {
    path: 'campaigns',
    component: CampaignsPageComponent,
    canActivate: [authenticationCanActivateGuard, setupCompletionCanActivateGuard],
    canMatch: [authenticationCanMatchGuard, setupCompletionCanMatchGuard],
    data: {
      requiredRole: 'Viewer'
    }
  },
  {
    path: 'campaigns/:campaignId',
    component: CampaignDetailPageComponent,
    canActivate: [authenticationCanActivateGuard, setupCompletionCanActivateGuard],
    canMatch: [authenticationCanMatchGuard, setupCompletionCanMatchGuard],
    data: {
      requiredRole: 'Viewer'
    }
  },

  {
    path: 'templates',
    component: TemplatesPageComponent,
    canActivate: [authenticationCanActivateGuard, setupCompletionCanActivateGuard],
    canMatch: [authenticationCanMatchGuard, setupCompletionCanMatchGuard],
    data: {
      requiredRole: 'Viewer'
    }
  },

  {
    path: 'tracking',
    component: TrackingDashboardPageComponent,
    canActivate: [authenticationCanActivateGuard, setupCompletionCanActivateGuard],
    canMatch: [authenticationCanMatchGuard, setupCompletionCanMatchGuard],
    data: {
      requiredRole: 'Viewer'
    }
  },
  {
    path: 'configuration',
    component: RuntimeConfigurationPageComponent,
    canActivate: [authenticationCanActivateGuard, setupCompletionCanActivateGuard],
    canMatch: [authenticationCanMatchGuard, setupCompletionCanMatchGuard],
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
