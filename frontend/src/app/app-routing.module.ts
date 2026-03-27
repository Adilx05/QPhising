import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

import { routeAccessGuard, routeMatchAccessGuard } from './core/auth/route-access.guard';
import { UnauthorizedPageComponent } from './core/auth/unauthorized-page.component';

const routes: Routes = [
  {
    path: '',
    pathMatch: 'full',
    redirectTo: 'dashboard'
  },
  {
    path: 'dashboard',
    canActivate: [routeAccessGuard],
    canMatch: [routeMatchAccessGuard],
    data: { roles: ['Viewer', 'Operator', 'Admin'] },
    loadChildren: () => import('./features/dashboard/dashboard.module').then((module) => module.DashboardModule)
  },
  {
    path: 'campaigns',
    canActivate: [routeAccessGuard],
    canMatch: [routeMatchAccessGuard],
    data: { roles: ['Viewer', 'Operator', 'Admin'] },
    loadChildren: () => import('./features/campaigns/campaigns.module').then((module) => module.CampaignsModule)
  },
  {
    path: 'templates',
    canActivate: [routeAccessGuard],
    canMatch: [routeMatchAccessGuard],
    data: { roles: ['Viewer', 'Operator', 'Admin'] },
    loadChildren: () => import('./features/templates/templates.module').then((module) => module.TemplatesModule)
  },
  {
    path: 'tracking',
    canActivate: [routeAccessGuard],
    canMatch: [routeMatchAccessGuard],
    data: { roles: ['Viewer', 'Operator', 'Admin'] },
    loadChildren: () => import('./features/tracking/tracking.module').then((module) => module.TrackingModule)
  },
  {
    path: 'tasks',
    canActivate: [routeAccessGuard],
    canMatch: [routeMatchAccessGuard],
    data: { roles: ['Operator', 'Admin'] },
    loadChildren: () => import('./features/tasks/tasks.module').then((module) => module.TasksModule)
  },
  {
    path: 'analytics',
    canActivate: [routeAccessGuard],
    canMatch: [routeMatchAccessGuard],
    data: { roles: ['Viewer', 'Operator', 'Admin'] },
    loadChildren: () => import('./features/analytics/analytics.module').then((module) => module.AnalyticsModule)
  },
  {
    path: 'exports',
    canActivate: [routeAccessGuard],
    canMatch: [routeMatchAccessGuard],
    data: { roles: ['Viewer', 'Operator', 'Admin'] },
    loadChildren: () => import('./features/exports/exports.module').then((module) => module.ExportsModule)
  },
  {
    path: 'unauthorized',
    component: UnauthorizedPageComponent
  },
  {
    path: '**',
    redirectTo: 'dashboard'
  }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule {}
