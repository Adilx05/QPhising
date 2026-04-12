import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

import { SetupPageComponent } from './containers/setup-page.component';

const routes: Routes = [
  {
    path: '',
    component: SetupPageComponent
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class SetupRoutingModule {}
