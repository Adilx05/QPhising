import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

import { SetupPageComponent } from './containers/setup-page.component';
import { SetupWizardPageComponent } from './containers/setup-wizard-page.component';

const routes: Routes = [
  {
    path: '',
    component: SetupPageComponent,
    children: [
      {
        path: '',
        pathMatch: 'full',
        redirectTo: 'wizard'
      },
      {
        path: 'wizard',
        component: SetupWizardPageComponent
      }
    ]
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class SetupRoutingModule {}
