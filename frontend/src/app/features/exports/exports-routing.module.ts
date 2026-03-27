import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

import { ExportsPageComponent } from './containers/exports-page.component';

const routes: Routes = [{ path: '', component: ExportsPageComponent }];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class ExportsRoutingModule {}
