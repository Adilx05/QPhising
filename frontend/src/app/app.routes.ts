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
import { DashboardPageComponent } from './features/dashboard/pages/dashboard-page.component';
import { RuntimeConfigurationPageComponent } from './features/setup/pages/runtime-configuration-page.component';
import { SetupWizardPageComponent } from './features/setup/pages/setup-wizard-page.component';

export const appRoutes: Routes = [
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
    redirectTo: 'dashboard'
  }
];
