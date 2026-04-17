import { Routes } from '@angular/router';
import {
  setupCompletionCanActivateGuard,
  setupCompletionCanMatchGuard
} from './core/guards/setup-completion.guard';
import { DashboardPageComponent } from './features/dashboard/pages/dashboard-page.component';
import { SetupWizardPageComponent } from './features/setup/pages/setup-wizard-page.component';

export const appRoutes: Routes = [
  {
    path: 'setup',
    component: SetupWizardPageComponent
  },
  {
    path: 'dashboard',
    component: DashboardPageComponent,
    canActivate: [setupCompletionCanActivateGuard],
    canMatch: [setupCompletionCanMatchGuard]
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
