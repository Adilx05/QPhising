import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

import { routeAccessGuard, routeMatchAccessGuard } from './core/auth/route-access.guard';
import {
  setupCompletionGuard,
  setupCompletionMatchGuard,
  setupPageGuard,
  setupPageMatchGuard
} from './core/setup/setup-completion.guard';
import { UnauthorizedPageComponent } from './core/auth/unauthorized-page.component';

const routes: Routes = [
  {
    path: '',
    pathMatch: 'full',
    redirectTo: 'setup/wizard'
  },
  {
    path: 'setup',
    canActivate: [routeAccessGuard, setupPageGuard],
    canMatch: [routeMatchAccessGuard, setupPageMatchGuard],
    data: { roles: ['Admin'] },
    loadChildren: () => import('./features/setup/setup.module').then((module) => module.SetupModule)
  },
  {
    path: 'dashboard',
    canActivate: [routeAccessGuard, setupCompletionGuard],
    canMatch: [routeMatchAccessGuard, setupCompletionMatchGuard],
    data: { roles: ['Viewer', 'Operator', 'Admin'] },
    loadChildren: () => import('./features/dashboard/dashboard.module').then((module) => module.DashboardModule)
  },
  {
    path: 'campaigns',
    canActivate: [routeAccessGuard, setupCompletionGuard],
    canMatch: [routeMatchAccessGuard, setupCompletionMatchGuard],
    data: { roles: ['Viewer', 'Operator', 'Admin'] },
    loadChildren: () => import('./features/campaigns/campaigns.module').then((module) => module.CampaignsModule)
  },
  {
    path: 'templates',
    canActivate: [routeAccessGuard, setupCompletionGuard],
    canMatch: [routeMatchAccessGuard, setupCompletionMatchGuard],
    data: { roles: ['Viewer', 'Operator', 'Admin'] },
    loadChildren: () => import('./features/templates/templates.module').then((module) => module.TemplatesModule)
  },
  {
    path: 'tracking',
    canActivate: [routeAccessGuard, setupCompletionGuard],
    canMatch: [routeMatchAccessGuard, setupCompletionMatchGuard],
    data: { roles: ['Viewer', 'Operator', 'Admin'] },
    loadChildren: () => import('./features/tracking/tracking.module').then((module) => module.TrackingModule)
  },
  {
    path: 'tasks',
    canActivate: [routeAccessGuard, setupCompletionGuard],
    canMatch: [routeMatchAccessGuard, setupCompletionMatchGuard],
    data: { roles: ['Operator', 'Admin'] },
    loadChildren: () => import('./features/tasks/tasks.module').then((module) => module.TasksModule)
  },
  {
    path: 'analytics',
    canActivate: [routeAccessGuard, setupCompletionGuard],
    canMatch: [routeMatchAccessGuard, setupCompletionMatchGuard],
    data: { roles: ['Viewer', 'Operator', 'Admin'] },
    loadChildren: () => import('./features/analytics/analytics.module').then((module) => module.AnalyticsModule)
  },
  {
    path: 'exports',
    canActivate: [routeAccessGuard, setupCompletionGuard],
    canMatch: [routeMatchAccessGuard, setupCompletionMatchGuard],
    data: { roles: ['Viewer', 'Operator', 'Admin'] },
    loadChildren: () => import('./features/exports/exports.module').then((module) => module.ExportsModule)
  },
  {
    path: 'unauthorized',
    component: UnauthorizedPageComponent
  },
  {
    path: '**',
    redirectTo: 'setup/wizard'
  }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule {}
