import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

const routes: Routes = [
  {
    path: '',
    pathMatch: 'full',
    redirectTo: 'dashboard'
  },
  {
    path: 'dashboard',
    loadChildren: () => import('./features/dashboard/dashboard.module').then((module) => module.DashboardModule)
  },
  {
    path: 'campaigns',
    loadChildren: () => import('./features/campaigns/campaigns.module').then((module) => module.CampaignsModule)
  },
  {
    path: 'templates',
    loadChildren: () => import('./features/templates/templates.module').then((module) => module.TemplatesModule)
  },
  {
    path: 'tracking',
    loadChildren: () => import('./features/tracking/tracking.module').then((module) => module.TrackingModule)
  },
  {
    path: 'tasks',
    loadChildren: () => import('./features/tasks/tasks.module').then((module) => module.TasksModule)
  },
  {
    path: 'analytics',
    loadChildren: () => import('./features/analytics/analytics.module').then((module) => module.AnalyticsModule)
  },
  {
    path: 'exports',
    loadChildren: () => import('./features/exports/exports.module').then((module) => module.ExportsModule)
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
