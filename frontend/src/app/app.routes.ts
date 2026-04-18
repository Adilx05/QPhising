import { Routes } from '@angular/router';
import {
  authenticationCanActivateGuard,
  authenticationCanMatchGuard,
  setupCompletionCanActivateGuard,
  setupCompletionCanMatchGuard
} from './core/guards';
import { DashboardPageComponent } from './features/dashboard/pages/dashboard-page.component';
import { RuntimeConfigurationPageComponent } from './features/setup/pages/runtime-configuration-page.component';
import { SetupWizardPageComponent } from './features/setup/pages/setup-wizard-page.component';

export const appRoutes: Routes = [
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
